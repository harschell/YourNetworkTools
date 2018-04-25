using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.VR;

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * CommunicationsController
	 * 
	 * Manages the system communications
	 * 
	 * @author Esteban Gallardo
	 */
	public class CommunicationsController : MonoBehaviour
	{
		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const string MESSAGE_TYPE_NEW_CONNECTION = "NEW_CONNECTION";
		public const string MESSAGE_TYPE_DISCONNECTION = "DISCONNECTION";
		public const string MESSAGE_TYPE_INFORMATION = "INFORMATION";
		public const string MESSAGE_TYPE_REGISTER_PREFAB = "REGISTER_PREFAB";
		public const string MESSAGE_TYPE_CREATE_OBJECT = "CREATE_OBJECT";
		public const string MESSAGE_TYPE_IP_ADDRESS = "IP_ADDRESS";
		public const string MESSAGE_TYPE_EVENT = "EVENT";

		public const string DATAFIELD_UID = "uid";
		public const string DATAFIELD_TYPE = "type";
		public const string DATAFIELD_DATA = "data";
		public const string DATAFIELD_POSITION = "position";
		public const string DATAFIELD_EVENT = "event";
		public const string DATAFIELD_ORIGIN_ID = "origin";
		public const string DATAFIELD_TARGET_ID = "target";

		public const string TOKEN_PARAMETER_SEPARATOR = "<p>";
		public const string TOKEN_LINE_SEPARATOR = "<br>";

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static CommunicationsController _instance;

		public static CommunicationsController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(CommunicationsController)) as CommunicationsController;
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PUBLIC MEMEBERS
		// ----------------------------------------------
		public int LocalNetworkID = -1;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private bool m_isInited = false;
		private bool m_isServer = true;
		private int m_networkID = -1;
		private bool m_visualInterfaceActivated = false;
		private List<PlayerConnectionData> m_playersConnections = new List<PlayerConnectionData>();
		private List<Dictionary<string, object>> m_displayMessages = new List<Dictionary<string, object>>();
		private int m_currentProcessedClient = 0;

		private List<RegisteredPrefabData> m_registeredPrefabs = new List<RegisteredPrefabData>();

		private ClientEventsDefinition m_clientEventDefinition;
		private ClientInstalledApps m_clientInstalledApps;

		public bool IsServer
		{
			get { return m_isServer; }
			set { m_isServer = value; }
		}
		public int NetworkID
		{
			get { return m_networkID; }
		}
		public bool VisualInterfaceActivated
		{
			get { return m_visualInterfaceActivated; }
		}

		// -------------------------------------------
		/* 
		 * Initialitzation
		 */
		void Start()
		{
			if (!m_isInited)
			{
				m_isInited = true;

				NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
				UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
			}
		}

		// -------------------------------------------
		/* 
		* Destroy all references
		*/
		public void Destroy()
		{
			if (_instance != null)
			{
				if (m_clientInstalledApps != null) m_clientInstalledApps.Destroy();
				NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
				UIEventController.Instance.UIEvent -= OnUIEvent;
				DestroyObject(_instance);
				_instance = null;
			}
		}

		// -------------------------------------------
		/* 
		 * Set the local instance of the player connection
		 */
		public void SetLocalInstance(int _networkID, PlayerConnectionController _playerInstance)
		{
			m_networkID = _networkID;
			LocalNetworkID = _networkID;
			if (!m_isServer)
			{
				string messageNetAddress = MessageIPAdress(m_networkID);
				NetworkEventController.Instance.DelayLocalEvent(NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_SEND_MESSAGE_CLIENT_TO_SERVER, 0.2f, messageNetAddress);
			}
			Debug.Log("CommunicationsController::SetLocalInstance::LOCAL PLAYER::NetworkID[" + m_networkID + "]");
		}

		// -------------------------------------------
		/* 
		 * Remove a player connection by id
		 */
		private bool RemoveConnection(int _idConnection)
		{
			for (int i = 0; i < m_playersConnections.Count; i++)
			{
				if (m_playersConnections[i].Id == _idConnection)
				{
					m_playersConnections[i].Destroy();
					m_playersConnections.RemoveAt(i);
					i--;
					return true;
				}
			}
			return false;
		}

		// -------------------------------------------
		/* 
		 * Get a player connection by id
		 */
		public PlayerConnectionData GetConnection(int _idConnection)
		{
			for (int i = 0; i < m_playersConnections.Count; i++)
			{
				if (m_playersConnections[i].Id == _idConnection)
				{
					return m_playersConnections[i];
				}
			}
			return null;
		}

		// -------------------------------------------
		/* 
		 * Creation of a JSON message from a dynamic list
		 */
		public static string CreateJSONMessage(int _idConnection, string _typeMessage, params string[] _list)
		{
			Dictionary<string, object> jsonData = new Dictionary<string, object>();
			jsonData.Add(DATAFIELD_UID, _idConnection);
			jsonData.Add(DATAFIELD_TYPE, _typeMessage);
			for (int i = 0; i < _list.Length; i += 2)
			{
				if (i + 1 < _list.Length)
				{
					jsonData.Add(_list[i], _list[i + 1]);
				}
				else
				{
					Debug.Log("ConnectionController::CreateJSONMessage::BAD NUMBER OF PARAMETERS CREATING JSON::_list[" + i + "]=" + _list[i]);
				}
			}
			return Json.Serialize(jsonData);
		}

		// -------------------------------------------
		/* 
		 * Creation of an information message
		 */
		public static string MessageEvent(int _idConnection, string _eventName, int _idOriginConnection, int _idTargetConnection, params object[] _list)
		{
			string eventData = "";
			for (int i = 0; i < _list.Length; i++)
			{
				if (_list[i] is string)
				{
					eventData += (string)_list[i];
					if (i + 1 < _list.Length)
					{
						eventData += TOKEN_PARAMETER_SEPARATOR;
					}
				}
				else
				{
					Debug.LogError("CommunicationsController::MessageEvent::ERROR::THE NETWORK EVENTS SHOULD ALWAYS HAVE STRING DATA::CONFLICTE IN[" + i + "]=" + _list[i] + "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
					return null;
				}
			}

			return CreateJSONMessage(_idConnection, MESSAGE_TYPE_EVENT,
									DATAFIELD_EVENT, _eventName,
									DATAFIELD_ORIGIN_ID, _idOriginConnection.ToString(),
									DATAFIELD_TARGET_ID, _idTargetConnection.ToString(),
									DATAFIELD_DATA, eventData);
		}

		// -------------------------------------------
		/* 
		 * Creation of an information message
		 */
		public static string MessageInformation(int _idConnection, string _data)
		{
			return CreateJSONMessage(_idConnection, MESSAGE_TYPE_INFORMATION,
									DATAFIELD_DATA, _data);
		}

		// -------------------------------------------
		/* 
		 * Creation of an register message
		 */
		public static string MessageRegisterPrefab(int _idConnection, string _classNameResources, int _indexPrefab)
		{
			return CreateJSONMessage(_idConnection, MESSAGE_TYPE_REGISTER_PREFAB,
									DATAFIELD_DATA, _classNameResources + TOKEN_PARAMETER_SEPARATOR + _indexPrefab);
		}

		// -------------------------------------------
		/* 
		 * Creation of an create object message
		 */
		public static string MessageCreateObject(int _idConnection, string _classNameResources, string _typeObject, string _namePrefab, Vector3 _position, string _assignedName, bool _allowServerChange, bool _allowClientChange)
		{
			return CreateJSONMessage(_idConnection, MESSAGE_TYPE_CREATE_OBJECT,
									DATAFIELD_DATA, _classNameResources + TOKEN_PARAMETER_SEPARATOR + _typeObject + TOKEN_PARAMETER_SEPARATOR + _namePrefab + TOKEN_PARAMETER_SEPARATOR + _assignedName + TOKEN_PARAMETER_SEPARATOR + _allowServerChange + TOKEN_PARAMETER_SEPARATOR + _allowClientChange,
									DATAFIELD_POSITION, "" + _position.x + TOKEN_PARAMETER_SEPARATOR + _position.y + TOKEN_PARAMETER_SEPARATOR + _position.z);
		}

		// -------------------------------------------
		/* 
		 * Creation of an message with the IP address
		 */
		public static string MessageIPAdress(int _idConnection)
		{
			string ip4 = NetworkManager.singleton.networkAddress;
			return CreateJSONMessage(_idConnection, MESSAGE_TYPE_IP_ADDRESS,
									DATAFIELD_DATA, ip4.Substring(ip4.LastIndexOf(':') + 1, ip4.Length - ip4.LastIndexOf(':') - 1));
		}


		// -------------------------------------------
		/* 
		 * Creation of a JSON message from a dynamic list
		 */
		public static Dictionary<string, object> DeserializeJSON(string _jsonString)
		{
			return Json.Deserialize(_jsonString) as Dictionary<string, object>;
		}

		// -------------------------------------------
		/* 
		* New client has been connected
		*/
		public void ClientNewConnection(int _idConnection, GameObject _reference)
		{
			PlayerConnectionData newPlayerConnectionData = new PlayerConnectionData(_idConnection, _reference);
			if (!m_playersConnections.Contains(newPlayerConnectionData))
			{
				m_playersConnections.Add(newPlayerConnectionData);
				m_currentProcessedClient = 0;
				string eventConnected = CreateJSONMessage(_idConnection, MESSAGE_TYPE_NEW_CONNECTION);
				m_displayMessages.Add(DeserializeJSON(eventConnected));
				Debug.Log(eventConnected);
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_LIST_USERS, m_playersConnections);
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_REGISTER_LOG, eventConnected);
			}
		}

		// -------------------------------------------
		/* 
		 * A client has been disconnected
		 */
		public void ClientDisconnected(int _idConnection)
		{
			if (RemoveConnection(_idConnection))
			{
				string eventDisconnected = CreateJSONMessage(_idConnection, MESSAGE_TYPE_DISCONNECTION);
				m_displayMessages.Add(DeserializeJSON(eventDisconnected));
				Debug.Log(eventDisconnected);
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_LIST_USERS, m_playersConnections);
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_REGISTER_LOG, eventDisconnected);
				NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_PLAYERCONNECTIONDATA_USER_DISCONNECTED, _idConnection);
			}
		}

		// -------------------------------------------
		/* 
		 * A message from the clients has been received
		 */
		public void MessageFromClientsToServer(int _idConnection, string _messageData)
		{
			PlayerConnectionData playerConnection = GetConnection(_idConnection);
			if (playerConnection != null)
			{
				Debug.Log("PlayersConnectionController::MessageFromClients::MESSAGE FROM CLIENT[" + _idConnection + "]=" + _messageData);
				Dictionary<string, object> message = DeserializeJSON(_messageData);
				playerConnection.PushMessage(message);
				m_displayMessages.Add(message);
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_REGISTER_LOG, _messageData);
			}
		}

		// -------------------------------------------
		/* 
		 * Process the messages from the server
		 */
		public void MessageFromServerToClients(int _idConnectionDestination, int _idConnectionOrigin, string _messageData)
		{
			if (((_idConnectionDestination == -1) || (_idConnectionDestination == m_networkID)) && (_idConnectionOrigin != m_networkID))
			{
				Debug.Log("PlayersConnectionController::MessageFromServer::MESSAGE FROM FROM SERVER TO CLIENT[" + _idConnectionDestination + "]=" + _messageData.ToString());
				Dictionary<string, object> message = DeserializeJSON(_messageData);
				ProcessMessage(message);
				m_displayMessages.Add(message);
			}
		}

		// -------------------------------------------
		/* 
		 * Set a binary data from a client
		 */
		public void SetBinaryDataFromClientsToServer(int _idConnection, byte[] _binaryData)
		{
			PlayerConnectionData playerConnection = GetConnection((int)_idConnection);
			if (playerConnection != null)
			{
				playerConnection.SetBinaryData(_binaryData);
			}
		}

		// -------------------------------------------
		/* 
		 * Clears all the messages
		 */
		public void ClearMessages()
		{
			m_displayMessages.Clear();
		}

		// -------------------------------------------
		/* 
		 * Check if the prefab has been registered
		 */
		public bool ExistRegisterPrefab(GameObject _newPrefab)
		{
			for (int i = 0; i < m_registeredPrefabs.Count; i++)
			{
				if (m_registeredPrefabs[i].Prefab == _newPrefab)
				{
					return true;
				}
			}
			return false;
		}

		// -------------------------------------------
		/* 
		 * Register new prefab
		 */
		public void RegisterNewPrefab(RegisteredPrefabData _newPrefab)
		{
			m_registeredPrefabs.Add(_newPrefab);
		}

		// -------------------------------------------
		/* 
		 * Register a new prefab in the list of spawnable network object
		 */
		public void RegisterPrefab(GameObject _newPrefab, string _classNetworkResources, string _typeObjects, string _prefabName)
		{
			if (!ExistRegisterPrefab(_newPrefab))
			{
				RegisterNewPrefab(new RegisteredPrefabData(_newPrefab, _classNetworkResources, _typeObjects, _prefabName));
				ClientScene.RegisterPrefab(_newPrefab);
			}
			else
			{
				Debug.Log("PlayerConnectionController::It was not possible to register");
			}
		}

		// -------------------------------------------
		/* 
		 * Register a new prefab from a class name reference
		 */
		public void RegisterPrefabFromClass(string _classNetworkResources, string _typeObjects, string _prefabName)
		{
			GameObject prefab = GetPrefabFromClass(_classNetworkResources, _typeObjects, _prefabName);
			if (prefab != null)
			{
				RegisterPrefab(prefab, _classNetworkResources, _typeObjects, _prefabName);
			}
			else
			{
				Debug.Log("PlayerConnectionFromClass::RegisterPrefabFromClass::NOT FOUND PREFAB FOR CLASS[" + _classNetworkResources + "," + _prefabName + "]");
			}
		}

		// -------------------------------------------
		/* 
		 * Will get the prefab from the referenced class name
		 */
		public GameObject GetPrefabFromClass(string _classNetworkResources, string _typeObjects, string _prefabName)
		{
			GameObject prefab = null;
			GameObject resourcesClass = GameObject.Find(_classNetworkResources);
			if (resourcesClass != null)
			{
				GameObject[] prefabs = null;
				switch (_typeObjects)
				{
					case NetworkEventController.REGISTER_PREFABS_OBJECTS:
						prefabs = resourcesClass.GetComponent<INetworkResources>().AppWorldObjects;
						break;
				}

				if (prefabs == null) return null;
				if (prefabs.Length == 0) return null;

				for (int i = 0; i < prefabs.Length; i++)
				{
					if (prefabs[i].name == _prefabName)
					{
						prefab = prefabs[i];
					}
				}

				return prefab;
			}
			return prefab;
		}

		// -------------------------------------------
		/* 
		* Display messages
		*/
		void OnGUI()
		{
			if (MultiplayerConfiguration.DEBUG_MODE)
			{
				GUILayout.BeginVertical();
				if (m_networkID == -1)
				{
					GUILayout.Label(new GUIContent("--[UNET]--SERVER IS SETTING UP. WAIT..."));
				}
				else
				{
					GUILayout.Label(new GUIContent("++[UNET]++MACHINE CONNECTION[" + m_networkID + "][" + (IsServer ? "SERVER" : "CLIENT") + "]"));
				}
				GUILayout.EndVertical();
			}
		}

		// -------------------------------------------
		/* 
		 * Process the messages of the clients connected
		 */
		private void ProcessClientMessages()
		{
			if (m_playersConnections.Count > 0)
			{
				Dictionary<string, object> message = null;
				if (m_currentProcessedClient < m_playersConnections.Count)
				{
					message = m_playersConnections[m_currentProcessedClient].PopMessage();
				}
				if (message != null)
				{
					ProcessMessage(message);
				}
				m_currentProcessedClient++;
				if (m_currentProcessedClient >= m_playersConnections.Count) m_currentProcessedClient = 0;
			}
		}

		// -------------------------------------------
		/* 
		 * Process a single message
		 */
		private void ProcessMessage(Dictionary<string, object> _message)
		{
			long networkOriginId = (long)_message[DATAFIELD_UID];
			string typeMessage = (string)_message[DATAFIELD_TYPE];
			switch (typeMessage)
			{
				case MESSAGE_TYPE_INFORMATION:
					string dataInformationMessage = _message[DATAFIELD_DATA].ToString();
					Debug.Log("MESSAGE_TYPE_INFORMATION::FROM[" + networkOriginId + "];DATA=" + dataInformationMessage);
					break;

				case MESSAGE_TYPE_REGISTER_PREFAB:
					if (!m_isServer)
					{
						Debug.Log("MESSAGE_TYPE_REGISTER_PREFAB::FROM[" + networkOriginId + "];DATA=" + _message[DATAFIELD_DATA].ToString());
						string[] prefabData = _message[DATAFIELD_DATA].ToString().Split(new string[] { TOKEN_PARAMETER_SEPARATOR }, StringSplitOptions.None);
						NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_REGISTER_PREFAB, prefabData[0], int.Parse(prefabData[1]));
					}
					break;

				case MESSAGE_TYPE_CREATE_OBJECT:
					if (m_isServer)
					{
						string[] objectData = _message[DATAFIELD_DATA].ToString().Split(new string[] { TOKEN_PARAMETER_SEPARATOR }, StringSplitOptions.None);
						string[] positionData = _message[DATAFIELD_POSITION].ToString().Split(new string[] { TOKEN_PARAMETER_SEPARATOR }, StringSplitOptions.None);
						Vector3 position = new Vector3(float.Parse(positionData[0]), float.Parse(positionData[1]), float.Parse(positionData[2]));
						NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_REQUEST_TO_CREATE_NETWORK_OBJECT, objectData[0], objectData[1], objectData[2], position, objectData[3], bool.Parse(objectData[4]), bool.Parse(objectData[5]));
					}
					break;

				case MESSAGE_TYPE_IP_ADDRESS:
					if (m_isServer)
					{
						PlayerConnectionData client = GetConnection((int)networkOriginId);
						if (client != null)
						{
							/*
							for (int i = 0; i < NetworkServer.connections.Count; i ++)
							{
								Debug.Log("NetworkServer.connections["+i+"]=" + NetworkServer.connections[i].address);
							}
							*/
							string ip4 = NetworkServer.connections[NetworkServer.connections.Count - 1].address;
							client.NetworkAddress = ip4.Substring(ip4.LastIndexOf(':') + 1, ip4.Length - ip4.LastIndexOf(':') - 1);
							Debug.Log("NETWORK ADDRESS RECEIVED::client.NetworkAddress=" + client.NetworkAddress);
						}
					}
					break;

				case MESSAGE_TYPE_EVENT:
					string nameEvent = _message[DATAFIELD_EVENT].ToString();
					int idOriginConnection = int.Parse(_message[DATAFIELD_ORIGIN_ID].ToString());
					int idTargetConnection = int.Parse(_message[DATAFIELD_TARGET_ID].ToString());
					string[] dataEvent = _message[DATAFIELD_DATA].ToString().Split(new string[] { TOKEN_PARAMETER_SEPARATOR }, StringSplitOptions.None);

					// IF OTHER MACHINES, DISPATCH EVENT LOCALLY
					if ((m_networkID != idOriginConnection) &&  // NOT ORIGIN
						((idTargetConnection == -1) || ((idTargetConnection != -1) && (m_networkID == idTargetConnection)))) // IS TARGET OR IT'S FOR ALL
					{
						NetworkEventController.Instance.DispatchCustomNetworkEvent(nameEvent, true, idOriginConnection, idTargetConnection, dataEvent);
					}

					// IF SERVER, BROADCAST RECEIVED MESSAGE ON ALL THE CLIENTS
					if (m_isServer)
					{
						string messageNetworkEvent = MessageEvent(m_networkID, nameEvent, idOriginConnection, idTargetConnection, dataEvent);
						NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_RPC_MESSAGE, idTargetConnection, idOriginConnection, messageNetworkEvent);
					}
					break;
			}
		}

		// -------------------------------------------
		/* 
		 * Manager of network events
		 */
		private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS)
			{
				Destroy();
			}
			if (!IsServer)
			{
				// +++++ CLIENT SIDE EVENTS
				if (_nameEvent == NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_REGISTER_INITIAL_DATA_ON_SERVER)
				{
					// REGISTER THE EVENTS IN THE SERVER
					m_clientEventDefinition = (ClientEventsDefinition)_list[0];
					m_clientEventDefinition.SendEventDefinitionToServer();

					// GET THE INSTALLED APPS AND SEND THE NAME TO THE SERVER
					m_clientInstalledApps = new ClientInstalledApps();
					m_clientInstalledApps.SendInstalledAppsToServer();
				}
				if (_nameEvent == NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_SEND_MESSAGE_CLIENT_TO_SERVER)
				{
					string messageInformation = (string)_list[0];
					NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_MESSAGE, messageInformation);
				}
			}
			else
			{
				// +++++ SERVER SIDE EVENTS
				if (_nameEvent == NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_SEND_MESSAGE_FROM_SERVER_TO_CLIENTS)
				{
					int networkIdPlayerGoal = (int)_list[0];
					string messageInformation = (string)_list[1];
					NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_RPC_MESSAGE, networkIdPlayerGoal, m_networkID, messageInformation);
				}
			}
			if (_nameEvent == NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_REGISTER_PREFAB)
			{
				RegisterPrefabFromClass((string)_list[0], (string)_list[1], (string)_list[2]);
			}
			// IF IT'S A NETWORK MESSAGE THEN SEND IT BY BROADCAST
			if (!_isLocalEvent)
			{
				string messageNetworkEvent = MessageEvent(_networkOriginID, _nameEvent, _networkOriginID, _networkTargetID, _list);
				if (m_isServer)
				{
					NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_RPC_MESSAGE, -1, m_networkID, messageNetworkEvent);
				}
				else
				{
					NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_MESSAGE, messageNetworkEvent);
				}
			}
		}


		// -------------------------------------------
		/* 
		 * Manager of ui events
		 */
		private void OnUIEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == UIEventController.EVENT_SCREENMAINCOMMANDCENTER_ACTIVATION_VISUAL_INTERFACE)
			{
				m_visualInterfaceActivated = true;
			}
			if (_nameEvent == UIEventController.EVENT_SCREENMAINCOMMANDCENTER_REQUEST_LIST_USERS)
			{
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_LIST_USERS, m_playersConnections);
			}
		}


		// -------------------------------------------
		/* 
		* Process the messages sent by the clients
		*/
		void Update()
		{
			ProcessClientMessages();
		}
	}
}