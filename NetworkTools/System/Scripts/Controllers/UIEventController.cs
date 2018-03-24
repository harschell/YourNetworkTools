using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace YourNetworkingTools
{

	public delegate void UIEventHandler(string _nameEvent, params object[] _list);

	/******************************************
	 * 
	 * UIEventController
	 * 
	 * Class used to dispatch events related to the UI
	 * 
	 * @author Esteban Gallardo
	 */
	public class UIEventController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		// ScreenController
		public const string EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN = "EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN";
		public const string EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN = "EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN";
		public const string EVENT_SCREENMANAGER_DESTROY_SCREEN = "EVENT_SCREENMANAGER_DESTROY_SCREEN";
		public const string EVENT_SCREENMANAGER_DESTROY_ALL_SCREEN = "EVENT_SCREENMANAGER_DESTROY_ALL_SCREEN";
		public const string EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON = "EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON";
		public const string EVENT_GENERIC_MESSAGE_INFO_OK_BUTTON = "EVENT_GENERIC_MESSAGE_INFO_OK_BUTTON";

		// ScreenMainCommandCenterView 
		public const string EVENT_SCREENMAINCOMMANDCENTER_REGISTER_LOG = "EVENT_SCREENMAINCOMMANDCENTER_REGISTER_LOG";
		public const string EVENT_SCREENMAINCOMMANDCENTER_LIST_USERS = "EVENT_SCREENMAINCOMMANDCENTER_LIST_USERS";
		public const string EVENT_SCREENMAINCOMMANDCENTER_ACTIVATION_VISUAL_INTERFACE = "EVENT_SCREENMAINCOMMANDCENTER_ACTIVATION_VISUAL_INTERFACE";
		public const string EVENT_SCREENMAINCOMMANDCENTER_TEXTURE_REMOTE_STREAMING_DATA = "EVENT_SCREENMAINCOMMANDCENTER_TEXTURE_REMOTE_STREAMING_DATA";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string SCREEN_LOADING = "SCREEN_LOADING";
		public const string SCREEN_INFORMATION = "SCREEN_INFORMATION";

		public event UIEventHandler UIEvent;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static UIEventController instance;

		public static UIEventController Instance
		{
			get
			{
				if (!instance)
				{
					instance = GameObject.FindObjectOfType(typeof(UIEventController)) as UIEventController;
					if (!instance)
					{
						GameObject container = new GameObject();
						container.name = "UIEventController";
						instance = container.AddComponent(typeof(UIEventController)) as UIEventController;
					}
				}
				return instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private List<AppEventData> listEvents = new List<AppEventData>();

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		private UIEventController()
		{
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		void OnDestroy()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			if (instance != null)
			{
				DestroyObject(instance.gameObject);
				instance = null;
			}
		}

		// -------------------------------------------
		/* 
		 * Will dispatch a UI event
		 */
		public void DispatchUIEvent(string _nameEvent, params object[] _list)
		{
			// Debug.Log("[UI]_nameEvent=" + _nameEvent);
			if (UIEvent != null) UIEvent(_nameEvent, _list);
		}

		// -------------------------------------------
		/* 
		 * Will dispatch a delayed UI event
		 */
		public void DelayUIEvent(string _nameEvent, float _time, params object[] _list)
		{
			listEvents.Add(new AppEventData(_nameEvent, -1, true, -1, _time, _list));
		}

		// -------------------------------------------
		/* 
		 * Will process the queue of delayed events 
		 */
		void Update()
		{
			// DELAYED EVENTS
			for (int i = 0; i < listEvents.Count; i++)
			{
				AppEventData eventData = listEvents[i];
				eventData.Time -= Time.deltaTime;
				if (eventData.Time <= 0)
				{
					UIEvent(eventData.NameEvent, eventData.ListParameters);
					eventData.Destroy();
					listEvents.RemoveAt(i);
					break;
				}
			}
		}
	}
}
