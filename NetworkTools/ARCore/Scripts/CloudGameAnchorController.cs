using System;
using System.Collections.Generic;
#if ENABLE_GOOGLE_ARCORE
using GoogleARCore;
using GoogleARCore.CrossPlatform;
#endif
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

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
#if ENABLE_GOOGLE_ARCORE
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_CLOUDGAMEANCHOR_SETUP_ANCHOR = "EVENT_CLOUDGAMEANCHOR_SETUP_ANCHOR";
		public const string EVENT_CLOUDGAMEANCHOR_UPDATE_CAMERA = "EVENT_CLOUDGAMEANCHOR_UPDATE_CAMERA";

		public const bool ENABLE_CUBE_REFERENCE_ON_ANCHOR	= true;
		public const bool ENABLE_ARCORE_CLOUD_SHARED		= false;
		public const bool ENABLE_ARCORE_START_GAME_WORLD	= true;

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
		public const string NAME_CLOUD_ANCHOR_ID		= "NAME_CLOUD_ANCHOR_ID";
		public const string NAME_CLOUD_VECTOR_BASE		= "NAME_CLOUD_VECTOR_BASE";
		public const string NAME_CLOUD_ANCHOR_POSITION = "NAME_CLOUD_ANCHOR_POSITION";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------
		public GUISkin SkinCloud;
		public GameObject FitToScanOverlay;
		public Camera FirstPersonCamera;
		public Camera GameCamera;

		public float ScaleVRWorldXZ = 4;
		public float ScaleVRWorldY = 2;

		[Header("ARCore")]
		public GameObject ARCoreRoot; /// The root for ARCore-specific GameObjects in the scene.
		public GameObject PlaneGenerator;
		public GameObject PointViewer;
		public Text TextMessage;

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
		private GameObject m_goReferencePose;
		private Vector3 m_positionARCorePlayer;
		private NetworkString m_networkCloudId;
		private NetworkVector3 m_networkVectorBaseServer;
		private NetworkVector3 m_networkAnchorBaseServer;
		private bool m_enableSetUpAnchor = false;

		// IMAGE DICTIONARY TRACKED IMAGES
		private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();

		// PLAYER TRACKING
		private bool m_trackingStarted = false;
		private Vector3 m_prevARPosePosition = Vector3.zero;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------
		public bool HasBeenInitialized
		{
			get { return m_hasBeenInitialized; }
		}

		// -------------------------------------------
		/* 
		 * Awake
		 */
		void Awake()
		{
			Utilities.DebugLogError("CloudGameAnchorController::Awake");
			NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
		}

		// -------------------------------------------
		/* 
		* DisableARCore
		*/
		public void DisableARCore(bool _activation)
		{
			GameCamera.enabled = true;
			FirstPersonCamera.enabled = false;
			FitToScanOverlay.SetActive(false);
			TextMessage.gameObject.SetActive(false);
			PointViewer.SetActive(false);
			if (m_goReferenceAnchor!=null) m_goReferenceAnchor.GetComponent<Renderer>().enabled = false;
			this.gameObject.SetActive(_activation);
		}

		// -------------------------------------------
		/* 
		* EnableARCore
		*/
		public void EnableARCore()
		{
			GameCamera.enabled = false;
			FirstPersonCamera.enabled = true;
			FitToScanOverlay.SetActive(true);
			TextMessage.gameObject.SetActive(true);
			PointViewer.SetActive(true);
			this.gameObject.SetActive(true);
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
				PlaneGenerator.SetActive(false);
			}

			if (GameCamera.gameObject.GetComponent<Rigidbody>() != null)
			{
				GameCamera.gameObject.GetComponent<Rigidbody>().useGravity = false;
				GameCamera.gameObject.GetComponent<Rigidbody>().isKinematic = true;
			}
			GameCamera.enabled = false;

			LanguageController.Instance.Initialize();
			TextMessage.text = LanguageController.Instance.GetText("arcore.message.to.synchronize");

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
				PlaceCubeReferenceAnchor(m_lastPlacedAnchor.transform.position, m_lastPlacedAnchor.transform);

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

					PlaceCubeReferenceAnchor(image.CenterPose.position, anchor.transform);

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
			m_enableSetUpAnchor = false;
			if (ENABLE_ARCORE_CLOUD_SHARED)
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
					m_networkVectorBaseServer = new NetworkVector3();
					Vector3 vectorBase = Frame.Pose.position - result.Anchor.transform.position;
					m_networkVectorBaseServer.InitRemote(YourNetworkTools.Instance.GetUniversalNetworkID(), NAME_CLOUD_VECTOR_BASE, vectorBase);
					m_networkAnchorBaseServer = new NetworkVector3();
					m_networkAnchorBaseServer.InitRemote(YourNetworkTools.Instance.GetUniversalNetworkID(), NAME_CLOUD_ANCHOR_POSITION, result.Anchor.transform.position);
				});
			}
			else
			{
				m_hasBeenInitialized = true;
			}
		}

		// -------------------------------------------
		/* 
		 * Connects with a cloud anchor id from Google Platform
		 */
		private void ResolveAnchorFromId(string _cloudAnchorId)
		{
			m_enableSetUpAnchor = false;
			if (ENABLE_ARCORE_CLOUD_SHARED)
			{
				XPSession.ResolveCloudAnchor(_cloudAnchorId).ThenAction((System.Action<CloudAnchorResult>)(result =>
				{
					if (result.Response != CloudServiceResponse.Success)
					{
						BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_CLOUDGAMEANCHOR_SETUP_ANCHOR, false);
						return;
					}

					if (!m_hasBeenInitialized)
					{
						m_hasBeenInitialized = true;
						m_lastResolvedAnchor = result.Anchor;
						PlaceCubeReferenceAnchor(m_lastResolvedAnchor.transform.position, m_lastResolvedAnchor.transform);
					}
				}));
			}
			else
			{				
				m_hasBeenInitialized = true;
			}
		}

		// -------------------------------------------
		/* 
		 * PlaceCubeReferenceAnchor
		 */
		private void PlaceCubeReferenceAnchor(Vector3 _position, Transform _parent)
		{
			if (ENABLE_CUBE_REFERENCE_ON_ANCHOR)
			{
				if (m_goReferenceAnchor == null)
				{
					m_goReferenceAnchor = GameObject.CreatePrimitive(PrimitiveType.Cube);
					m_goReferenceAnchor.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
					m_goReferenceAnchor.transform.position = _position;
					m_goReferenceAnchor.transform.parent = _parent;
				}
			}

			m_goReferencePose = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			m_goReferencePose.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
			m_goReferencePose.transform.parent = _parent;
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
			return m_hasBeenInitialized || (m_lastPlacedAnchor != null) || (m_lastResolvedAnchor != null);
#endif
		}

		// -------------------------------------------
		/* 
		 * UpdatePositionGameWorld
		 */
		private void UpdatePositionGameWorld()
		{
#if !UNITY_EDITOR
			m_goReferencePose.transform.position = Frame.Pose.position;
			m_positionARCorePlayer = m_goReferencePose.transform.localPosition;
#else
			m_positionARCorePlayer = Vector3.zero;
#endif
			if (!m_trackingStarted)
			{
				m_trackingStarted = true;
				m_prevARPosePosition = Utilities.Clone(m_positionARCorePlayer);
				if (ENABLE_ARCORE_START_GAME_WORLD)
				{					
					FirstPersonCamera.enabled = false;
#if !UNITY_EDITOR
					if (FirstPersonCamera.GetComponent<ARCoreBackgroundRenderer>() != null)
					{
						FirstPersonCamera.GetComponent<ARCoreBackgroundRenderer>().enabled = false;
					}	
					PointViewer.SetActive(false);
#endif
					if (m_goReferenceAnchor != null) m_goReferenceAnchor.GetComponent<Renderer>().enabled = false;
					GameCamera.enabled = true;
					BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_CLOUDGAMEANCHOR_SETUP_ANCHOR, true);
				}
				FitToScanOverlay.SetActive(false);
			}
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
				UpdatePositionGameWorld();
#else
				UpdatePositionGameWorld();

				Vector3 posRealWorld = m_positionARCorePlayer;
				Vector3 posVRWorld = new Vector3(posRealWorld.x * ScaleVRWorldXZ,
												posRealWorld.y * ScaleVRWorldY,
												posRealWorld.z * ScaleVRWorldXZ);

				BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_CLOUDGAMEANCHOR_UPDATE_CAMERA, FirstPersonCamera.transform.forward, posVRWorld);
#endif
			}
		}

		// -------------------------------------------
		/* 
		 * WaitForARCoreValid
		 */
		public void WaitForARCoreValid()
		{
			if (Session.Status.IsValid() && (Session.Status == SessionStatus.Tracking))
			{
				ResolveAnchorFromId((string)m_networkCloudId.GetValue());
			}
			else
			{
				Invoke("WaitForARCoreValid", 1);
			}
		}

		// -------------------------------------------
		/* 
		 * OnNetworkEvent
		 */
		private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, object[] _list)
		{
			if (!this.gameObject.activeSelf) return;

			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_INITIALITZATION_LOCAL_COMPLETED)
			{
				if (ENABLE_ARCORE_CLOUD_SHARED)
				{
					if (YourNetworkTools.Instance.IsServer)
					{
						m_enableSetUpAnchor = true;
					}
				}
				else
				{
					m_enableSetUpAnchor = true;
				}				
			}
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_VARIABLE_CREATE_LOCAL)
			{
				if (!ENABLE_ARCORE_CLOUD_SHARED) return;

				if (!YourNetworkTools.Instance.IsServer)
				{
					INetworkVariable objData = (INetworkVariable)_list[0];
					bool check = false;
					if (objData.Name == NAME_CLOUD_ANCHOR_ID)
					{
						m_networkCloudId = (NetworkString)objData;
						check = true;
					}
					if (objData.Name == NAME_CLOUD_VECTOR_BASE)
					{
						m_networkVectorBaseServer = (NetworkVector3)objData;
						check = true;
					}
					if (objData.Name == NAME_CLOUD_ANCHOR_POSITION)
					{
						m_networkAnchorBaseServer = (NetworkVector3)objData;
						check = true;
					}
					if (check)
					{
						if ((m_networkCloudId != null) && (m_networkVectorBaseServer != null) && (m_networkAnchorBaseServer!=null))
						{
							Debug.Log("**************************START JOINING PROCESS**************************");
#if !UNITY_EDITOR
						WaitForARCoreValid();
#endif
						}
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		* OnGUI
		*/
		void OnGUI()
		{
			/*
			GUI.skin = SkinCloud;
			if (m_positionARCorePlayer != null)
			{
				GUI.Label(new Rect(new Rect(0, 0, 1000, 50)),  "AR=" + m_positionARCorePlayer.ToString() + "::REF=" + m_goReferencePose.transform.position.ToString());
			}
			*/
		}

		// -------------------------------------------
		/* 
		* Update
		*/
		public void Update()
		{
			if (!this.gameObject.activeSelf) return;

			UpdateApplicationLifecycle();

			// UPDATE PLAYER POSITION IN THE GAME WORLD
			UpdateGameCamera();

			// IGNORE IF THE ANCHOR HAS BEEN SET UP
			if (m_enableSetUpAnchor)
			{
#if UNITY_EDITOR
				m_enableSetUpAnchor = false;
				m_hasBeenInitialized = true;
				m_lastPlacedAnchor = new Anchor();
				m_networkCloudId = new NetworkString();
				m_networkCloudId.InitRemote(YourNetworkTools.Instance.GetUniversalNetworkID(), NAME_CLOUD_ANCHOR_ID, "IdAnchorFake");
				m_networkVectorBaseServer = new NetworkVector3();
				m_networkVectorBaseServer.InitRemote(YourNetworkTools.Instance.GetUniversalNetworkID(), NAME_CLOUD_VECTOR_BASE, new Vector3(1,2,3));
				m_networkAnchorBaseServer = new NetworkVector3();
				m_networkAnchorBaseServer.InitRemote(YourNetworkTools.Instance.GetUniversalNetworkID(), NAME_CLOUD_ANCHOR_POSITION, new Vector3(66,99,69));
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
#endif
				}
			}
