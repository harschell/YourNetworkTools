using System;
using System.Collections.Generic;
#if ENABLE_GOOGLE_ARCORE
using GoogleARCore;
using GoogleARCore.CrossPlatform;
using GoogleARCore.Examples.Common;
#endif
using UnityEngine;
using YourCommonTools;

#if ENABLE_GOOGLE_ARCORE
namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * CloudGameAnchorController
	 * 
	 * Controller of the class that deals with 
	 * retrieving the position of the player in 
	 * relation to the anchor
	 *
	 * @author Esteban Gallardo
	 */
	public class CloudGameAnchorController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_CLOUDGAMEANCHOR_SETUP_ANCHOR = "EVENT_CLOUDGAMEANCHOR_SETUP_ANCHOR";
		public const string EVENT_CLOUDGAMEANCHOR_UPDATE_CAMERA = "EVENT_CLOUDGAMEANCHOR_UPDATE_CAMERA";

		public const bool ENABLE_CUBE_REFERENCE_ON_ANCHOR = false;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static CloudGameAnchorController _instance;

		public static CloudGameAnchorController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(CloudGameAnchorController)) as CloudGameAnchorController;
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const string NAME_CLOUD_ANCHOR_ID = "NAME_CLOUD_ANCHOR_ID";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------
		public GameObject FitToScanOverlay;
		public Camera FirstPersonCamera;
		public Camera GameCamera;

		public float ScaleVRWorldXZ = 4;
		public float ScaleVRWorldY = 2;

		[Header("ARCore")]
		public GameObject ARCoreRoot; /// The root for ARCore-specific GameObjects in the scene.

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private bool m_IsQuitting = false;
		private bool m_enableImageDetection = false;
		private bool m_hasBeenInitialized = false;

		// ANCHORS		
		private Component m_lastPlacedAnchor = null;
		private XPAnchor m_lastResolvedAnchor = null;
		private GameObject m_goReferenceAnchor;
		private NetworkString m_networkCloudId;
		private bool m_enableSetUpAnchor = false;

		// IMAGE DICTIONARY TRACKED IMAGES
		private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();

		// PLAYER TRACKING
		private bool m_trackingStarted = false;
		private Vector3 m_playerInitialPosition = Vector3.zero;
		private Vector3 m_prevARPosePosition = Vector3.zero;

		// -------------------------------------------
		/* 
		 * Awake
		 */
		void Awake()
		{
			NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
		}

		// -------------------------------------------
		/* 
		* Start
		*/
		public void Start()
		{
			if (GameObject.FindObjectOfType<ARCoreSession>().SessionConfig.PlaneFindingMode == DetectedPlaneFindingMode.Disabled)
			{
				m_enableImageDetection = true;
			}
			else
			{
				m_enableImageDetection = false;
			}

			if (m_enableImageDetection)
			{
				if (GameObject.FindObjectOfType<DetectedPlaneGenerator>() != null)
				{
					GameObject.FindObjectOfType<DetectedPlaneGenerator>().gameObject.SetActive(false);
				}
			}

			GameCamera.enabled = false;

			ResetStatus();
		}

		// -------------------------------------------
		/* 
		 * OnDestroy
		 */
		void OnDestroy()
		{
			NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
		}

		// -------------------------------------------
		/* 
		 * AnchorByPlane
		 */
		private void AnchorByPlane()
		{
			Touch touch;
			if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
			{
				return;
			}

			// Raycast against the location the player touched to search for planes.
			TrackableHit hit;
			if (Frame.Raycast(touch.position.x, touch.position.y, TrackableHitFlags.PlaneWithinPolygon, out hit))
			{
				m_lastPlacedAnchor = hit.Trackable.CreateAnchor(hit.Pose);
			}

			if (m_lastPlacedAnchor != null)
			{
				if (ENABLE_CUBE_REFERENCE_ON_ANCHOR)
				{
					m_goReferenceAnchor = GameObject.CreatePrimitive(PrimitiveType.Cube);
					m_goReferenceAnchor.transform.position = m_lastPlacedAnchor.transform.position;
					m_goReferenceAnchor.transform.parent = m_lastPlacedAnchor.transform;
				}

				// (HOSTING) SAVE CLOUD ANCHOR
				HostLastPlacedAnchor();
			}
		}

		// -------------------------------------------
		/* 
		 * AnchorByImage
		 */
		private void AnchorByImage()
		{
			// Get updated augmented images for this frame.
			Session.GetTrackables<AugmentedImage>(m_TempAugmentedImages, TrackableQueryFilter.Updated);

			foreach (var image in m_TempAugmentedImages)
			{
				if (image.TrackingState == TrackingState.Tracking)
				{
					// Create an anchor to ensure that ARCore keeps tracking this augmented image.
					Anchor anchor = image.CreateAnchor(image.CenterPose);
					m_lastPlacedAnchor = anchor;

					if (ENABLE_CUBE_REFERENCE_ON_ANCHOR)
					{
						m_goReferenceAnchor = GameObject.CreatePrimitive(PrimitiveType.Cube);
						m_goReferenceAnchor.transform.position = image.CenterPose.position;
						m_goReferenceAnchor.transform.parent = anchor.transform;
					}

					// (HOSTING) SAVE CLOUD ANCHOR
					HostLastPlacedAnchor();
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Register a cloud anchor in Google Platform
		 */
		private void HostLastPlacedAnchor()
		{
			var anchor = (Anchor)m_lastPlacedAnchor;
			XPSession.CreateCloudAnchor(anchor).ThenAction(result =>
			{
				if (result.Response != CloudServiceResponse.Success)
				{
					BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_CLOUDGAMEANCHOR_SETUP_ANCHOR, false);
					return;
				}

				// NETWORK CLOUD ID
				m_hasBeenInitialized = true;
				m_networkCloudId = new NetworkString();
				m_networkCloudId.InitRemote(YourNetworkTools.Instance.GetUniversalNetworkID(), NAME_CLOUD_ANCHOR_ID, result.Anchor.CloudId);
				BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_CLOUDGAMEANCHOR_SETUP_ANCHOR, true);
			});
		}

		// -------------------------------------------
		/* 
		 * Connects with a cloud anchor id from Google Platform
		 */
		private void ResolveAnchorFromId(string _cloudAnchorId)
		{
			XPSession.ResolveCloudAnchor(_cloudAnchorId).ThenAction((System.Action<CloudAnchorResult>)(result =>
			{
				if (result.Response != CloudServiceResponse.Success)
				{
					BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_CLOUDGAMEANCHOR_SETUP_ANCHOR, false);
					return;
				}

				m_hasBeenInitialized = true;
				m_lastResolvedAnchor = result.Anchor;
				if (ENABLE_CUBE_REFERENCE_ON_ANCHOR)
				{
					m_goReferenceAnchor = GameObject.CreatePrimitive(PrimitiveType.Cube);
					m_goReferenceAnchor.transform.parent = result.Anchor.transform;
				}
				BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_CLOUDGAMEANCHOR_SETUP_ANCHOR, true);
			}));
		}

		// -------------------------------------------
		/* 
		 * ResetStatus
		 */
		private void ResetStatus()
		{
			// Reset internal status.
			if (m_lastPlacedAnchor != null)
			{
				Destroy(m_lastPlacedAnchor);
			}
			m_lastPlacedAnchor = null;

			if (m_lastPlacedAnchor != null)
			{
				Destroy(m_lastResolvedAnchor);
			}
			m_lastResolvedAnchor = null;
		}

		// -------------------------------------------
		/* 
		 * Check and update the application lifecycle.
		 */
		private void UpdateApplicationLifecycle()
		{
			// Exit the app when the 'back' button is pressed.
			var sleepTimeout = SleepTimeout.NeverSleep;

			Screen.sleepTimeout = sleepTimeout;

			if (m_IsQuitting)
			{
				return;
			}

			// Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
			if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
			{
				ShowAndroidToastMessage("Camera permission is needed to run this application.");
				m_IsQuitting = true;
			}
			else if (Session.Status.IsError())
			{
				ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
				m_IsQuitting = true;
			}
		}

		// -------------------------------------------
		/* 
		 * Show an Android toast message.
		 */
		private void ShowAndroidToastMessage(string message)
		{
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

			if (unityActivity != null)
			{
				AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
				unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
				{
					AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
						message, 0);
					toastObject.Call("show");
				}));
			}
		}

		// -------------------------------------------
		/* 
		 * IsAnchorSetUp
		 */
		private bool IsAnchorSetUp()
		{
#if UNITY_EDITOR
			return m_hasBeenInitialized;
#else
			return (m_lastPlacedAnchor != null) || (m_lastResolvedAnchor != null);
#endif
		}

		// -------------------------------------------
		/* 
		 * UpdateGameCamera
		 * 
		 * Update player position in the game world in relation to the anchor
		 */
		private void UpdateGameCamera()
		{
			if (IsAnchorSetUp())
			{
#if UNITY_EDITOR
				if (!m_trackingStarted)
				{
					m_trackingStarted = true;
					FirstPersonCamera.enabled = false;
					FitToScanOverlay.SetActive(false);
					if (GameObject.FindObjectOfType<ARCoreBackgroundRenderer>() != null)
					{
						GameObject.FindObjectOfType<ARCoreBackgroundRenderer>().enabled = false;
					}
				}
#else
				Vector3 posPlayer = Vector3.zero;
				if (m_lastPlacedAnchor != null)
				{
					posPlayer = m_lastPlacedAnchor.transform.position - Frame.Pose.position;
				}
				else
				{
					posPlayer = m_lastResolvedAnchor.transform.position - Frame.Pose.position;
				}

				if (!m_trackingStarted)
				{
					m_trackingStarted = true;
					FirstPersonCamera.enabled = false;
					FitToScanOverlay.SetActive(false);
					m_prevARPosePosition = posPlayer;
					m_playerInitialPosition = posPlayer;
					if (GameObject.FindObjectOfType<ARCoreBackgroundRenderer>() != null)
					{
						GameObject.FindObjectOfType<ARCoreBackgroundRenderer>().enabled = false;
					}
				}

				// Remember the previous position so we can apply deltas
				Vector3 posRealWorld = (m_playerInitialPosition - posPlayer);
				Vector3 posVRWorld = new Vector3(posRealWorld.x * ScaleVRWorldXZ,
												posRealWorld.y * ScaleVRWorldY,
												posRealWorld.z * ScaleVRWorldXZ);

				BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_CLOUDGAMEANCHOR_UPDATE_CAMERA, FirstPersonCamera.transform.forward, posVRWorld);
#endif
			}
		}

		// -------------------------------------------
		/* 
		 * OnNetworkEvent
		 */
		private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, object[] _list)
		{
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_INITIALITZATION_LOCAL_COMPLETED)
			{
				if (YourNetworkTools.Instance.IsServer)
				{
					m_enableSetUpAnchor = true;
				}
			}
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_VARIABLE_CREATE_LOCAL)
			{
				INetworkVariable objData = (INetworkVariable)_list[0];

				if (objData.Name == NAME_CLOUD_ANCHOR_ID)
				{
					m_hasBeenInitialized = true;
					m_networkCloudId = (NetworkString)objData;
#if UNITY_EDITOR
					m_lastResolvedAnchor = new XPAnchor();
					BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_CLOUDGAMEANCHOR_SETUP_ANCHOR, true);
#else
					ResolveAnchorFromId((string)m_networkCloudId.GetValue());
#endif
				}
			}
		}

		// -------------------------------------------
		/* 
		* Update
		*/
		public void Update()
		{
			UpdateApplicationLifecycle();

			// UPDATE PLAYER POSITION IN THE GAME WORLD
			UpdateGameCamera();

			// IGNORE IF THE ANCHOR HAS BEEN SET UP
			if (m_enableSetUpAnchor)
			{
#if UNITY_EDITOR
				m_hasBeenInitialized = true;
				m_lastPlacedAnchor = new Anchor();
				m_networkCloudId = new NetworkString();
				m_networkCloudId.InitRemote(YourNetworkTools.Instance.GetUniversalNetworkID(), NAME_CLOUD_ANCHOR_ID, "IdAnchorFake");
				BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_CLOUDGAMEANCHOR_SETUP_ANCHOR, true);
#else
				if (m_lastPlacedAnchor != null)
				{
					return;
				}

				// SET THE ANCHOR ONE TIME
				if (m_enableImageDetection)
				{
					AnchorByImage();
				}
				else
				{
					AnchorByPlane();
				}
#endif
			}
		}
	}
}
#endif
