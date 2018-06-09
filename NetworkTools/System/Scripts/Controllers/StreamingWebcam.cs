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
	 * StreamingWebcam
	 * 
	 * Stream the webcam from a selected Hololens
	 * 
	 * @author Esteban Gallardo
	 */
	public class StreamingWebcam : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_STREAMINGWEBCAM_IMAGE_DATA = "EVENT_STREAMINGWEBCAM_IMAGE_DATA";

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static StreamingWebcam instance;

		public static StreamingWebcam Instance
		{
			get
			{
				if (!instance)
				{
					instance = GameObject.FindObjectOfType(typeof(StreamingWebcam)) as StreamingWebcam;
					if (!instance)
					{
						GameObject container = new GameObject();
						container.name = "StreamingWebcam";
						instance = container.AddComponent(typeof(StreamingWebcam)) as StreamingWebcam;
					}
				}
				return instance;
			}
		}

		private bool m_isInitialized = false;

		private float m_time = 0;
		private WebCamTexture m_webcamTexture;
		private Texture2D m_imageTexture2D;

		// -------------------------------------------
		/* 
		 * Gets the stream of the hololens webcam
		 */
		public void Initialize()
		{
			m_isInitialized = true;
		}

		// -------------------------------------------
		/* 
		 * Start the webstreaming
		 */
		void Start()
		{
			if (m_webcamTexture != null)
			{
				return;
			}

			m_webcamTexture = new WebCamTexture();

			try
			{
				WebCamDevice[] devices = WebCamTexture.devices;
				if (devices.Length > 0)
				{
					for (int i = 0; i < devices.Length; i++)
					{
						Debug.Log("DEVICE[" + i + "]=" + devices[i].name + "************************");
						if (devices[i].name.Length > 0)
						{
							m_webcamTexture.deviceName = devices[i].name;
							m_webcamTexture.Play();
							m_imageTexture2D = new Texture2D(m_webcamTexture.width, m_webcamTexture.height);
							return;
						}
						else
						{
							m_webcamTexture = null;
						}
					}
				}
				else
				{
					m_webcamTexture = null;
				}
			}
			catch (Exception err)
			{
				m_webcamTexture = null;
			};
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
		}

		// -------------------------------------------
		/* 
		 * Send data to server
		 */
		void Update()
		{
			if (m_webcamTexture != null)
			{
				m_time += Time.deltaTime;
				if (m_time > 0.3)
				{
					m_time = 0;
					if (m_isInitialized)
					{
						m_imageTexture2D.SetPixels32(m_webcamTexture.GetPixels32());
						byte[] dataStream = m_imageTexture2D.EncodeToJPG(75);

						// MAKE BINARY DATA PACKET WITH IMAGE
						NetworkEventController.Instance.DispatchNetworkBinaryDataEvent(EVENT_STREAMINGWEBCAM_IMAGE_DATA,
																				BitConverter.GetBytes(m_webcamTexture.width),
																				BitConverter.GetBytes(m_webcamTexture.height),
																				dataStream);
					}
				}
			}
		}
	}
}
