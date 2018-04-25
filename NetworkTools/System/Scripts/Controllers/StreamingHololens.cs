#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * StreamingHololens
	 * 
	 * Stream the webcam from a selected Hololens
	 * 
	 * @author Esteban Gallardo
	 */
	public class StreamingHololens : MonoBehaviour
	{
		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static StreamingHololens instance;

		public static StreamingHololens Instance
		{
			get
			{
				if (!instance)
				{
					instance = GameObject.FindObjectOfType(typeof(StreamingHololens)) as StreamingHololens;
					if (!instance)
					{
						GameObject container = new GameObject();
						container.name = "StreamingHololens";
						instance = container.AddComponent(typeof(StreamingHololens)) as StreamingHololens;
					}
				}
				return instance;
			}
		}

		private bool m_isInitialized = false;

		private float m_time = 0;
		private UnityEngine.XR.WSA.WebCam.PhotoCapture m_photoCaptureObject = null;
		private UnityEngine.XR.WSA.WebCam.CameraParameters m_cameraParameters;

		// -------------------------------------------
		/* 
		 * Gets the stream of the hololens webcam
		 */
		public void Initialize()
		{
			NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
		}

		// -------------------------------------------
		/* 
		 * Start the webstreaming
		 */
		void Start()
		{
			UnityEngine.XR.WSA.WebCam.PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
		}

		// -------------------------------------------
		/* 
		 * OnPhotoCaptureCreated
		 */
		private void OnPhotoCaptureCreated(UnityEngine.XR.WSA.WebCam.PhotoCapture captureObject)
		{
			m_photoCaptureObject = captureObject;

			Resolution cameraResolution = UnityEngine.XR.WSA.WebCam.PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

			UnityEngine.XR.WSA.WebCam.CameraParameters m_cameraParameters = new UnityEngine.XR.WSA.WebCam.CameraParameters();
			m_cameraParameters.hologramOpacity = 0.0f;
			m_cameraParameters.cameraResolutionWidth = cameraResolution.width;
			m_cameraParameters.cameraResolutionHeight = cameraResolution.height;
			m_cameraParameters.pixelFormat = UnityEngine.XR.WSA.WebCam.CapturePixelFormat.BGRA32;
		}

		// -------------------------------------------
		/* 
		 * OnStoppedPhotoMode
		 */
		private void OnStoppedPhotoMode(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result)
		{
			m_photoCaptureObject.Dispose();
			m_photoCaptureObject = null;
		}

		// -------------------------------------------
		/* 
		 * OnPhotoModeStarted
		 */
		private void OnPhotoModeStarted(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result)
		{
			if (result.success)
			{
				m_photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
			}
			else
			{
				Debug.Log("Unable to start photo mode!");
			}
		}

		// -------------------------------------------
		/* 
		 * OnCapturedPhotoToMemory
		 */
		public void OnCapturedPhotoToMemory(UnityEngine.XR.WSA.WebCam.PhotoCapture.PhotoCaptureResult result, UnityEngine.XR.WSA.WebCam.PhotoCaptureFrame photoCaptureFrame)
		{
			if (result.success)
			{
				Texture2D imageTexture2D = new Texture2D(1, 1);
				photoCaptureFrame.UploadImageDataToTexture(imageTexture2D);
				byte[] dataStream = imageTexture2D.EncodeToJPG(75);
				Debug.Log("DIMENSIONS[" + imageTexture2D.width + "," + imageTexture2D.height + "]::dataStream=" + dataStream.Length);

				// MAKE BINARY DATA PACKET WITH IMAGE
				NetworkEventController.Instance.DispatchBinaryDataEvent(ScreenGenericServerManagerView.EVENT_SCREENGENERIC_IMAGE_DATA,
																		BitConverter.GetBytes(imageTexture2D.width),
																		BitConverter.GetBytes(imageTexture2D.height),
																		dataStream);

				m_photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
			}
			else
			{
				Debug.Log("Failed to get photo memory frame");
			}
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		private void OnDestroy()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		protected virtual void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if ((_nameEvent == NetworkEventController.EVENT_SYSTEM_DESTROY_NETWORK_SCREENS)
				|| (_nameEvent == NetworkEventController.EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS))
			{
				Destroy();
			}
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_INITIALITZATION_LOCAL_COMPLETED)
			{
				m_isInitialized = true;
			}
		}

		// -------------------------------------------
		/* 
		* Send data to server
		*/
		void Update()
		{
			m_time += Time.deltaTime;
			if (m_time > 0.3)
			{
				m_time = 0;
				if (m_isInitialized)
				{
					m_photoCaptureObject.StartPhotoModeAsync(m_cameraParameters, OnPhotoModeStarted);
				}
			}
		}
	}
}
#endif
