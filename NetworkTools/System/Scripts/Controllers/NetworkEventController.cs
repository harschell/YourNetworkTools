using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace YourNetworkingTools
{
	public delegate void NetworkEventHandler(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list);

	/******************************************
	 * 
	 * BasicEventController
	 * 
	 * Class used to dispatch events through all the system
	 * 
	 * @author Esteban Gallardo
	 */
	public class NetworkEventController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		// SYSTEM
		public const string EVENT_SYSTEM_INITIALITZATION_LOCAL_COMPLETED = "EVENT_SYSTEM_INITIALITZATION_LOCAL_COMPLETED";
		public const string EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED = "EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED";
		public const string EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS = "EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS";
		public const string EVENT_SYSTEM_DESTROY_NETWORK_SCREENS = "EVENT_SYSTEM_DESTROY_NETWORK_SCREENS";
		public const string EVENT_SYSTEM_PLAYER_HAS_BEEN_DESTROYED = "EVENT_SYSTEM_PLAYER_HAS_BEEN_DESTROYED";
		public const string EVENT_STREAMSERVER_REPORT_CLOSED_STREAM = "EVENT_STREAMSERVER_REPORT_CLOSED_STREAM";

		public const string EVENT_SYSTEM_VARIABLE_CREATE_REMOTE = "EVENT_SYSTEM_VARIABLE_CREATE_REMOTE";
		public const string EVENT_SYSTEM_VARIABLE_CREATE_LOCAL = "EVENT_SYSTEM_VARIABLE_CREATE_LOCAL";
		public const string EVENT_SYSTEM_VARIABLE_SET = "EVENT_SYSTEM_VARIABLE_SET";
		public const string EVENT_SYSTEM_VARIABLE_DESTROY = "EVENT_SYSTEM_VARIABLE_DESTROY";

		// CommunicationsController
		public const string EVENT_COMMUNICATIONSCONTROLLER_REGISTER_INITIAL_DATA_ON_SERVER = "EVENT_COMMUNICATIONSCONTROLLER_REGISTER_INITIAL_DATA_ON_SERVER";
		public const string EVENT_COMMUNICATIONSCONTROLLER_SEND_MESSAGE_FROM_SERVER_TO_CLIENTS = "EVENT_COMMUNICATIONSCONTROLLER_SEND_MESSAGE_FROM_SERVER_TO_CLIENTS";
		public const string EVENT_COMMUNICATIONSCONTROLLER_SEND_MESSAGE_CLIENT_TO_SERVER = "EVENT_COMMUNICATIONSCONTROLLER_SEND_MESSAGE_CLIENT_TO_SERVER";
		public const string EVENT_COMMUNICATIONSCONTROLLER_REQUEST_TO_CREATE_NETWORK_OBJECT = "EVENT_COMMUNICATIONSCONTROLLER_REQUEST_TO_CREATE_NETWORK_OBJECT";
		public const string EVENT_COMMUNICATIONSCONTROLLER_CREATION_CONFIRMATION_NETWORK_OBJECT = "EVENT_COMMUNICATIONSCONTROLLER_CREATION_CONFIRMATION_NETWORK_OBJECT";
		public const string EVENT_COMMUNICATIONSCONTROLLER_CREATION_CONFIRMATION_REGISTRATION = "EVENT_COMMUNICATIONSCONTROLLER_CREATION_CONFIRMATION_REGISTRATION";
		public const string EVENT_COMMUNICATIONSCONTROLLER_REGISTER_ALL_NETWORK_PREFABS = "EVENT_COMMUNICATIONSCONTROLLER_REGISTER_ALL_NETWORK_PREFABS";
		public const string EVENT_COMMUNICATIONSCONTROLLER_REGISTER_PREFAB = "EVENT_COMMUNICATIONSCONTROLLER_REGISTER_PREFAB";

		// PlayerConnectionController
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_CREATE_NETWORK_OBJECT = "EVENT_PLAYERCONNECTIONCONTROLLER_CREATE_NETWORK_OBJECT";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_DESTROY_NETWORK_OBJECT = "EVENT_PLAYERCONNECTIONCONTROLLER_DESTROY_NETWORK_OBJECT";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_DESTROY_CONFIRMATION_NETWORK_OBJECT = "EVENT_PLAYERCONNECTIONCONTROLLER_DESTROY_CONFIRMATION_NETWORK_OBJECT";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_UPDATE_PROPERTY = "EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_UPDATE_PROPERTY";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_MESSAGE = "EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_MESSAGE";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_RPC_MESSAGE = "EVENT_PLAYERCONNECTIONCONTROLLER_RPC_MESSAGE";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_TEXTURE = "EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_TEXTURE";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_REGISTER_VARIABLE_COMPLETED = "EVENT_PLAYERCONNECTIONCONTROLLER_REGISTER_VARIABLE_COMPLETED";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_KICK_OUT_PLAYER = "EVENT_PLAYERCONNECTIONCONTROLLER_KICK_OUT_PLAYER";
		public const string EVENT_PLAYERCONNECTIONCONTROLLER_CONFIRMATION_KICKED_OUT_PLAYER = "EVENT_PLAYERCONNECTIONCONTROLLER_CONFIRMATION_KICKED_OUT_PLAYER";

		// PlayerConnectionData
		public const string EVENT_PLAYERCONNECTIONDATA_USER_CONNECTED = "EVENT_PLAYERCONNECTIONDATA_USER_CONNECTED";
		public const string EVENT_PLAYERCONNECTIONDATA_USER_DISCONNECTED = "EVENT_PLAYERCONNECTIONDATA_USER_DISCONNECTED";
		public const string EVENT_PLAYERCONNECTIONDATA_NETWORK_ADDRESS = "EVENT_PLAYERCONNECTIONDATA_NETWORK_ADDRESS";

		// WORLD CONTROLLER
		public const string EVENT_WORLDOBJECTCONTROLLER_REMOTE_CREATION_CONFIRMATION = "EVENT_WORLDOBJECTCONTROLLER_REMOTE_CREATION_CONFIRMATION";
		public const string EVENT_WORLDOBJECTCONTROLLER_LOCAL_CREATION_CONFIRMATION = "EVENT_WORLDOBJECTCONTROLLER_LOCAL_CREATION_CONFIRMATION";
		public const string EVENT_WORLDOBJECTCONTROLLER_INITIAL_DATA = "EVENT_WORLDOBJECTCONTROLLER_INITIAL_DATA";
		public const string EVENT_WORLDOBJECTCONTROLLER_DESTROY_REQUEST = "EVENT_WORLDOBJECTCONTROLLER_DESTROY_REQUEST";
		public const string EVENT_WORLDOBJECTCONTROLLER_DESTROY_CONFIRMATION = "EVENT_WORLDOBJECTCONTROLLER_DESTROY_CONFIRMATION";

		// NETWORK VARIABLE
		public const string EVENT_NETWORKVARIABLE_STATE_REPORT = "EVENT_NETWORKVARIABLE_STATE_REPORT";
		public const string EVENT_NETWORKVARIABLE_REGISTER_NEW = "EVENT_NETWORKVARIABLE_REGISTER_NEW";

		// GENERIC EVENTS
		public const string EVENT_SIMPLE_TEXT_MESSAGE = "EVENT_SIMPLE_TEXT_MESSAGE";

		// REGISTER PREFAB TYPES
		public const string REGISTER_PREFABS_OBJECTS = "WorldObjects";

		// CLASS NAME WHICH CONTAINS THE SPECIFIC PROGRAM NETWORK PREFAB OBJECTS
		public const string CLASS_WORLDOBJECTCONTROLLER_NAME = "WorldObjectController";

		// BASIC TYPES
		public const string BASIC_TYPE_NETWORK_INTEGER_NAME = "NetworkIntegerData";
		public const string BASIC_TYPE_NETWORK_FLOAT_NAME = "NetworkFloatData";
		public const string BASIC_TYPE_NETWORK_STRING_NAME = "NetworkStringData";
		public const string BASIC_TYPE_NETWORK_VECTOR3_NAME = "NetworkVector3Data";

		public event NetworkEventHandler NetworkEvent;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static NetworkEventController instance;

		public static NetworkEventController Instance
		{
			get
			{
				if (!instance)
				{
					instance = GameObject.FindObjectOfType(typeof(NetworkEventController)) as NetworkEventController;
					if (!instance)
					{
						GameObject container = new GameObject();
						DontDestroyOnLoad(container);
						container.name = "NetworkEventController";
						instance = container.AddComponent(typeof(NetworkEventController)) as NetworkEventController;
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
		private NetworkEventController()
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
				DispatchLocalEvent(EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS);
				DestroyObject(instance.gameObject);
				instance = null;
			}
		}

		// -------------------------------------------
		/* 
		 * Will dispatch a network event
		 */
		public void DispatchNetworkEvent(string _nameEvent, params string[] _list)
		{
			// Debug.Log("[NETWORK]_nameEvent=" + _nameEvent);
			if (NetworkEvent != null) NetworkEvent(_nameEvent, false, YourNetworkTools.Instance.GetUniversalNetworkID(), -1, _list);
		}

		// -------------------------------------------
		/* 
		 * Will dispatch a custom event
		 */
		public void DispatchCustomNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params string[] _list)
		{
			// Debug.Log("[NETWORK]_nameEvent=" + _nameEvent);
			if (NetworkEvent != null) NetworkEvent(_nameEvent, _isLocalEvent, _networkOriginID, _networkTargetID, _list);
		}

		// -------------------------------------------
		/* 
		 * Will dispatch a local event
		 */
		public void DispatchLocalEvent(string _nameEvent, params object[] _list)
		{
			// Debug.Log("_nameEvent=" + _nameEvent);
			if (NetworkEvent != null) NetworkEvent(_nameEvent, true, -99, -99, _list);
		}

		// -------------------------------------------
		/* 
		 * Will add a new delayed local event to the queue
		 */
		public void DelayLocalEvent(string _nameEvent, float _time, params object[] _list)
		{
			listEvents.Add(new AppEventData(_nameEvent, AppEventData.CONFIGURATION_INTERNAL_EVENT, true, -99, _time, _list));
		}

		// -------------------------------------------
		/* 
		 * Clone a delayed event
		 */
		public void DelayBasicEvent(AppEventData _timeEvent)
		{
			listEvents.Add(new AppEventData(_timeEvent.NameEvent, AppEventData.CONFIGURATION_INTERNAL_EVENT, _timeEvent.IsLocalEvent, _timeEvent.NetworkID, _timeEvent.Time, _timeEvent.ListParameters));
		}

		// -------------------------------------------
		/* 
		 * Will dispatch a delayed network event
		 */
		public void DelayNetworkEvent(string _nameEvent, float _time, params string[] _list)
		{
			listEvents.Add(new AppEventData(_nameEvent, AppEventData.CONFIGURATION_INTERNAL_EVENT, false, YourNetworkTools.Instance.GetUniversalNetworkID(), _time, _list));
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
					NetworkEvent(eventData.NameEvent, eventData.IsLocalEvent, eventData.NetworkID, -1, eventData.ListParameters);
					eventData.Destroy();
					listEvents.RemoveAt(i);
					break;
				}
			}
		}
	}
}