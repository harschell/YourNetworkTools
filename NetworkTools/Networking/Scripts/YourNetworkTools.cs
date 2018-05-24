using System;
using System.Collections.Generic;
using UnityEngine;

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * YourNetworkTools
	 * 
	 * Interface to include the normal GameObject that the programmer
	 * wants to be Network managed and other configurations
	 *
	 * @author Esteban Gallardo
	 */
	public class YourNetworkTools : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_YOURNETWORKTOOLS_NETID_NEW = "EVENT_YOURNETWORKTOOLS_NETID_NEW";
		public const string EVENT_YOURNETWORKTOOLS_DESTROYED_GAMEOBJECT = "EVENT_YOURNETWORKTOOLS_DESTROYED_GAMEOBJECT";

		public const string COOCKIE_IS_LOCAL_GAME = "COOCKIE_IS_LOCAL_GAME";
		public const char TOKEN_SEPARATOR_NAME = '_';

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static YourNetworkTools instance;

		public static YourNetworkTools Instance
		{
			get
			{
				if (!instance)
				{
					instance = GameObject.FindObjectOfType(typeof(YourNetworkTools)) as YourNetworkTools;
				}
				return instance;
			}
		}
		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------
		public bool IsLocalGame = true;
		public float TimeToUpdateTransforms = 0.2f;
		public GameObject[] LocalNetworkPrefabManagers;
		public GameObject NetworkVariablesManager;
		public GameObject[] GameObjects;

		private List<NetworkWorldObject> m_unetNetworkObjects = new List<NetworkWorldObject>();
		private List<GameObject> m_tcpNetworkObjects = new List<GameObject>();
		private List<GameObject> m_tcpNetworkTypes = new List<GameObject>();
		private Dictionary<string, string> m_initialData = new Dictionary<string, string>();
		private float m_timeoutUpdateRemoteNetworkObject = 0;

		private bool m_activateTransformUpdate = false;
		private int m_networkIDReceived = -1;

		private int m_uidCounter = 0;

		public bool IsServer
		{
			get
			{
				if (IsLocalGame)
				{
					return CommunicationsController.Instance.IsServer;
				}
				else
				{
					return ClientTCPEventsController.Instance.IsServer();
				}
			}
		}
		public bool ActivateTransformUpdate
		{
			get { return m_activateTransformUpdate; }
			set { m_activateTransformUpdate = value; }
		}

		// -------------------------------------------
		/* 
		 * Stores in the coockie if it's a local game
		 */
		public static void SetLocalGame(bool _isLocalGame)
		{
			PlayerPrefs.SetInt(COOCKIE_IS_LOCAL_GAME, (_isLocalGame ? -1 : 1));
		}


		// -------------------------------------------
		/* 
		 * Stores in the coockie if it's a local game
		 */
		public static bool GetIsLocalGame()
		{
			return (PlayerPrefs.GetInt(COOCKIE_IS_LOCAL_GAME, -1000) == -1);
		}

		// -------------------------------------------
		/* 
		 * Converts the normal GameObjects in Network GameObjects
		 */
		void Start()
		{
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

			NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);

			int isLocalGame = PlayerPrefs.GetInt(COOCKIE_IS_LOCAL_GAME, -1);
			if (isLocalGame == -1)
			{
				IsLocalGame = true;
			}
			else
			{
				IsLocalGame = false;
			}

			if (IsLocalGame)
			{
				// INSTANTIATE LOCAL NETWORK PREFAB MANAGERS
				for (int j = 0; j < LocalNetworkPrefabManagers.Length; j++)
				{
					UtilitiesNetwork.AddChild(transform, LocalNetworkPrefabManagers[j]);
				}

				// NETWORK VARIABLES MANAGER
				UtilitiesNetwork.AddChild(transform, NetworkVariablesManager);

				// ASSIGN THE GAME OBJECTS TO THE CONTROLLER
				WorldObjectController worldObjectController = GameObject.FindObjectOfType<WorldObjectController>();
				if (worldObjectController != null)
				{
					worldObjectController.AppWorldObjects = new GameObject[GameObjects.Length];
					for (int i = 0; i < GameObjects.Length; i++)
					{
						GameObject prefabToNetwork = GameObjects[i];
						if (prefabToNetwork.GetComponent<NetworkWorldObjectData>() == null)
						{
							prefabToNetwork.AddComponent<NetworkWorldObjectData>();
						}
						else
						{
							prefabToNetwork.GetComponent<NetworkWorldObjectData>().enabled = true;
						}
						if (prefabToNetwork.GetComponent<NetworkID>() != null)
						{
							prefabToNetwork.GetComponent<NetworkID>().enabled = false;
						}
						if (prefabToNetwork.GetComponent<ActorNetwork>() == null)
						{
							prefabToNetwork.AddComponent<ActorNetwork>();
						}
						worldObjectController.AppWorldObjects[i] = prefabToNetwork;
					}
				}
			}
			else
			{
				// CONNECT TO THE SERVER
				ClientTCPEventsController.Instance.Initialitzation(MultiplayerConfiguration.LoadIPAddressServer(), MultiplayerConfiguration.LoadPortServer(), MultiplayerConfiguration.LoadRoomNumberInServer(0), MultiplayerConfiguration.LoadMachineIDServer(0));

				// NETWORK VARIABLES MANAGER
				UtilitiesNetwork.AddChild(transform, NetworkVariablesManager);

				// ADD NETWORK IDENTIFICATION TO THE GAME OBJECTS
				for (int i = 0; i < GameObjects.Length; i++)
				{
					GameObject prefabToNetwork = GameObjects[i];
					if (prefabToNetwork.GetComponent<NetworkID>() == null)
					{
						prefabToNetwork.AddComponent<NetworkID>();
					}
					else
					{
						prefabToNetwork.GetComponent<NetworkID>().enabled = true;
					}
					if (prefabToNetwork.GetComponent<NetworkWorldObjectData>() != null)
					{
						prefabToNetwork.GetComponent<NetworkWorldObjectData>().enabled = false;
					}
					if (prefabToNetwork.GetComponent<ActorNetwork>() == null)
					{
						prefabToNetwork.AddComponent<ActorNetwork>();
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Release resources
		 */
		public void Destroy()
		{
			NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
		}

		// -------------------------------------------
		/* 
		 * Returns the network identificator independently if it's a local or a remote game
		 */
		public int GetUniversalNetworkID()
		{
			if (IsLocalGame)
			{
				return CommunicationsController.Instance.NetworkID;
			}
			else
			{
				return ClientTCPEventsController.Instance.UniqueNetworkID;
			}
		}

		// -------------------------------------------
		/* 
		 * Get the prefab by name
		 */
		private GameObject GetPrefabByName(string _prefabName)
		{
			for (int i = 0; i < GameObjects.Length; i++)
			{
				if (GameObjects[i].name == _prefabName)
				{
					return GameObjects[i];
				}
			}
			return null;
		}

		// -------------------------------------------
		/* 
		 * Get the prefab index by name
		 */
		private int GetPrefabIndexOfName(string _prefabName)
		{
			for (int i = 0; i < GameObjects.Length; i++)
			{
				if (GameObjects[i].name == _prefabName)
				{
					return i;
				}
			}
			return -1;
		}
		// -------------------------------------------
		/* 
		 * Get the network object by id
		 */
		private object GetNetworkObjectByID(int _netID, int _uid)
		{
			if (IsLocalGame)
			{
				for (int i = 0; i < m_unetNetworkObjects.Count; i++)
				{
					if (m_unetNetworkObjects[i] != null)
					{
						if (m_unetNetworkObjects[i].GetNetworkObjectData().CheckID(_netID, _uid))
						{
							return m_unetNetworkObjects[i];
						}
					}
					else
					{
						m_unetNetworkObjects.RemoveAt(i);
						i--;
					}
				}
			}
			else
			{
				for (int i = 0; i < m_tcpNetworkObjects.Count; i++)
				{
					if (m_tcpNetworkObjects[i] != null)
					{
						if (m_tcpNetworkObjects[i].GetComponent<NetworkID>().CheckID(_netID, _uid))
						{
							return m_tcpNetworkObjects[i];
						}
					}
					else
					{
						m_tcpNetworkObjects.RemoveAt(i);
						i--;
					}
				}
			}
			return null;
		}

		// -------------------------------------------
		/* 
		* Create a NetworkObject
		*/
		public void CreateLocalNetworkObject(string _prefabName, object _initialData, bool _createInServer)
		{
			if (IsLocalGame)
			{
				string assignedNetworkName = _prefabName + TOKEN_SEPARATOR_NAME + GetUniversalNetworkID() + TOKEN_SEPARATOR_NAME + m_uidCounter;
				m_uidCounter++;
				NetworkWorldObject networkWorldObject = new NetworkWorldObject(assignedNetworkName, _prefabName, Vector3.zero, Vector3.zero, Vector3.one, _initialData, true, true, _createInServer);
				m_unetNetworkObjects.Add(networkWorldObject);
			}
			else
			{
				GameObject networkGameObject = UtilitiesNetwork.AddChild(this.gameObject.transform, GetPrefabByName(_prefabName));
				networkGameObject.GetComponent<NetworkID>().NetID = GetUniversalNetworkID();
				networkGameObject.GetComponent<NetworkID>().UID = m_uidCounter;
				m_uidCounter++;
				networkGameObject.GetComponent<NetworkID>().IndexPrefab = GetPrefabIndexOfName(_prefabName);
				m_tcpNetworkObjects.Add(networkGameObject);
				if (networkGameObject.GetComponent<IGameNetworkActor>() != null)
				{
					networkGameObject.GetComponent<IGameNetworkActor>().Initialize(_initialData);
				}
				ClientTCPEventsController.Instance.SendTranform(networkGameObject.GetComponent<NetworkID>().NetID, networkGameObject.GetComponent<NetworkID>().UID, networkGameObject.GetComponent<NetworkID>().IndexPrefab, networkGameObject.transform.position, networkGameObject.transform.forward, networkGameObject.transform.localScale);
			}
		}

		// -------------------------------------------
		/* 
		 * Will get the prefab from the referenced class name
		 */
		public GameObject GetPrefabFromClassName(GameObject[] _prefabs, string _prefabName)
		{
			GameObject prefab = null;
			if (_prefabs == null) return null;
			if (_prefabs.Length == 0) return null;

			for (int i = 0; i < _prefabs.Length; i++)
			{
				if (_prefabs[i].name == _prefabName)
				{
					prefab = _prefabs[i];
				}
			}

			return prefab;
		}

		// -------------------------------------------
		/* 
		* Will check if there are objects to be initialized with the data received
		*/
		private void CheckInitializationObjects()
		{
			if (IsLocalGame)
			{
				for (int i = 0; i < m_unetNetworkObjects.Count; i++)
				{
					if (m_unetNetworkObjects[i] != null)
					{
						if (m_unetNetworkObjects[i].GetNetworkObjectData() != null)
						{
							CheckExistingInitialDataForObject(m_unetNetworkObjects[i].GetNetworkObjectData().GetID(), m_unetNetworkObjects[i].GetNetworkObjectData().gameObject);
						}
					}
				}
			}
			else
			{
				for (int i = 0; i < m_tcpNetworkObjects.Count; i++)
				{
					if (m_tcpNetworkObjects[i] != null)
					{
						CheckExistingInitialDataForObject(m_tcpNetworkObjects[i].GetComponent<NetworkID>().GetID(), m_tcpNetworkObjects[i]);
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		* CheckExistingInitialDataForObject
		*/
		private bool CheckExistingInitialDataForObject(string _keyID, GameObject _objectToInit)
		{
			if (m_initialData.ContainsKey(_keyID))
			{
				string initialData = "";
				if (m_initialData.TryGetValue(_keyID, out initialData))
				{
					_objectToInit.GetComponent<IGameNetworkActor>().Initialize(initialData);
					if (m_initialData.Remove(_keyID))
					{
						return true;
					}
				}
			}
			return false;
		}

		// -------------------------------------------
		/* 
		* Will destroy a network object by NetID and UID
		*/
		private void DestroyNetworkObject(int _netID, int _uid)
		{
			if (IsLocalGame)
			{
				for (int i = 0; i < m_unetNetworkObjects.Count; i++)
				{
					if (m_unetNetworkObjects[i] != null)
					{
						if (m_unetNetworkObjects[i].GetNetworkObjectData() != null)
						{
							if ((m_unetNetworkObjects[i].GetNetworkObjectData().NetID == _netID)
								&& (m_unetNetworkObjects[i].GetNetworkObjectData().UID == _uid))
							{
#if DEBUG_MODE_DISPLAY_LOG
								Debug.LogError("[UNET] REMOVED FROM LIST");
#endif
								m_unetNetworkObjects.RemoveAt(i);
								return;
							}
						}
						else
						{
							m_unetNetworkObjects.RemoveAt(i);
							i--;
						}
					}
					else
					{
						m_unetNetworkObjects.RemoveAt(i);
						i--;
					}
				}
			}
			else
			{
				for (int i = 0; i < m_tcpNetworkObjects.Count; i++)
				{
					if (m_tcpNetworkObjects[i] != null)
					{
						if ((m_tcpNetworkObjects[i].GetComponent<NetworkID>().NetID == _netID)
							&& (m_tcpNetworkObjects[i].GetComponent<NetworkID>().UID == _uid))
						{
#if DEBUG_MODE_DISPLAY_LOG
							Debug.LogError("[SOCKET] REMOVED FROM LIST");
#endif
							m_tcpNetworkObjects.RemoveAt(i);
							return;
						}
					}
					else
					{
						m_tcpNetworkObjects.RemoveAt(i);
						i--;
					}
				}
			}
		}



		// -------------------------------------------
		/* 
		* Manager of global events
		*/
		private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID)
			{
#if ENABLE_BALANCE_LOADER
				int totalPlayersConfigurated = MultiplayerConfiguration.LoadNumberOfPlayers();
				if (totalPlayersConfigurated != MultiplayerConfiguration.VALUE_FOR_JOINING)
				{
					string friends = MultiplayerConfiguration.LoadFriendsGame();
					if (friends.Length > 0)
					{
						string[] friendIDs = friends.Split(',');
						int idRoomLobby = MultiplayerConfiguration.LoadRoomNumberInServer(-1);
						ClientTCPEventsController.Instance.CreateRoomForFriends(idRoomLobby, friendIDs, "extraData");
					}
					else
					{
						string nameRoomLobby = MultiplayerConfiguration.LoadNameRoomLobby();
						if (nameRoomLobby.Length > 0)
						{
							int idRoomLobby = MultiplayerConfiguration.LoadRoomNumberInServer(-1);
							ClientTCPEventsController.Instance.CreateRoomForLobby(idRoomLobby, nameRoomLobby, totalPlayersConfigurated, "extraData");
						}
						else
						{
							throw new Exception("THERE IS NO NAME OF LOBBY TO CREATE A TCP CONNECTION");
						}
					}
				}
				else
				{
					int idRoomLobby = MultiplayerConfiguration.LoadRoomNumberInServer(-1);
					if (idRoomLobby != -1)
					{
						if (MultiplayerConfiguration.LoadIsRoomLobby())
						{
							ClientTCPEventsController.Instance.JoinRoomOfLobby(idRoomLobby, "null", "extraData");
						}
						else
						{
							ClientTCPEventsController.Instance.JoinRoomForFriends(idRoomLobby, "null", "extraData");
						}
					}
					else
					{
						throw new Exception("NO GOOD");
					}
				}
#endif
			}
			if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_CONNECTED_ROOM)
			{
				// Debug.LogError("EVENT_CLIENT_TCP_CONNECTED_ROOM::UniversalUniqueID[" + GetUniversalNetworkID() + "]");
			}
			if (_nameEvent == NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_REMOTE_CREATION_CONFIRMATION)
			{
				ActorNetwork actorNetwork = ((GameObject)_list[0]).GetComponent<ActorNetwork>();
				CheckInitializationObjects();
			}
			if (_nameEvent == NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_INITIAL_DATA)
			{
				if (!m_initialData.ContainsKey((string)_list[0]))
				{
					m_initialData.Add((string)_list[0], (string)_list[1]);
					CheckInitializationObjects();
				}
			}
			if (_nameEvent == NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_DESTROY_REQUEST)
			{
				DestroyNetworkObject(int.Parse((string)_list[0]), int.Parse((string)_list[1]));
			}
			if (_nameEvent == NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_CREATION_CONFIRMATION_NETWORK_OBJECT)
			{
				m_unetNetworkObjects.Add(new NetworkWorldObject((GameObject)_list[0]));
			}
			if (_nameEvent == NetworkEventController.EVENT_PLAYERCONNECTIONDATA_USER_DISCONNECTED)
			{
				Debug.Log("----------------------DISCONNECTED PLAYER[" + (int)_list[0] + "]");
			}
			if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_TRANSFORM_DATA)
			{
				int NetID = (int)_list[0];
				int UID = (int)_list[1];
				int prefabIndex = (int)_list[2];
				Vector3 position = (Vector3)_list[3];
				Vector3 forward = (Vector3)_list[4];
				Vector3 scale = (Vector3)_list[5];
				object networkObject = GetNetworkObjectByID(NetID, UID);
				GameObject networkGameObject = null;
				if (networkObject == null)
				{
					m_networkIDReceived = NetID;
					networkGameObject = UtilitiesNetwork.AddChild(this.gameObject.transform, GetPrefabByName(GameObjects[prefabIndex].name));
					networkGameObject.GetComponent<NetworkID>().IndexPrefab = GetPrefabIndexOfName(GameObjects[prefabIndex].name);
					networkGameObject.GetComponent<NetworkID>().NetID = NetID;
					networkGameObject.GetComponent<NetworkID>().UID = UID;
					m_tcpNetworkObjects.Add(networkGameObject);
					networkGameObject.transform.position = position;
					networkGameObject.transform.forward = forward;
					networkGameObject.transform.localScale = scale;
					NetworkEventController.Instance.DispatchLocalEvent(EVENT_YOURNETWORKTOOLS_NETID_NEW, NetID);
				}
				else
				{
					networkGameObject = (GameObject)networkObject;
					InterpolatorController.Instance.Interpolate(networkGameObject, position, TimeToUpdateTransforms * 1.01f);
					networkGameObject.transform.forward = forward;
					networkGameObject.transform.localScale = scale;
				}
			}
			if (_nameEvent == EVENT_YOURNETWORKTOOLS_DESTROYED_GAMEOBJECT)
			{
				int NetID = (int)_list[1];
				int UID = (int)_list[2];

				if (IsLocalGame)
				{
					for (int i = 0; i < m_unetNetworkObjects.Count; i++)
					{
						bool removeObject = false;
						if (m_unetNetworkObjects[i] == null)
						{
							removeObject = true;
						}
						else
						{
							if (m_unetNetworkObjects[i].GetNetworkObjectData() == null)
							{
								removeObject = true;
							}
						}
						if (removeObject)
						{
							m_unetNetworkObjects.RemoveAt(i);
						}
						else
						{
							if ((m_unetNetworkObjects[i].GetNetworkObjectData().NetID == NetID)
							&& (m_unetNetworkObjects[i].GetNetworkObjectData().UID == UID))
							{
								m_unetNetworkObjects[i].Destroy();
								m_unetNetworkObjects.RemoveAt(i);
								return;
							}
						}
					}
				}
				else
				{
					for (int i = 0; i < m_tcpNetworkObjects.Count; i++)
					{
						bool removeObject = false;
						if (m_tcpNetworkObjects[i] == null)
						{
							removeObject = true;
						}
						if (removeObject)
						{
							m_tcpNetworkObjects.RemoveAt(i);
						}
						else
						{
							if ((m_tcpNetworkObjects[i].GetComponent<NetworkID>().NetID == NetID)
								&& (m_tcpNetworkObjects[i].GetComponent<NetworkID>().UID == UID))
							{
								m_tcpNetworkObjects.RemoveAt(i);
								return;
							}
						}
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		* Will update the transforms in the remotes players
		*/
		public void UpdateRemoteTransforms(bool _force)
		{
			if (m_activateTransformUpdate || _force)
			{
				if (!IsLocalGame)
				{
					m_timeoutUpdateRemoteNetworkObject += Time.deltaTime;
					if ((m_timeoutUpdateRemoteNetworkObject >= TimeToUpdateTransforms) || _force)
					{
						m_timeoutUpdateRemoteNetworkObject = 0;
						for (int i = 0; i < m_tcpNetworkObjects.Count; i++)
						{
							GameObject networkGameObject = m_tcpNetworkObjects[i];
							if (networkGameObject != null)
							{
								if (networkGameObject.GetComponent<NetworkID>() != null)
								{
									if ((networkGameObject.GetComponent<NetworkID>().NetID == GetUniversalNetworkID()) || _force)
									{
										ClientTCPEventsController.Instance.SendTranform(networkGameObject.GetComponent<NetworkID>().NetID, networkGameObject.GetComponent<NetworkID>().UID, networkGameObject.GetComponent<NetworkID>().IndexPrefab, networkGameObject.transform.position, networkGameObject.transform.forward, networkGameObject.transform.localScale);
									}
								}
							}
						}
					}
				}
				else
				{
					for (int i = 0; i < m_unetNetworkObjects.Count; i++)
					{
						NetworkWorldObject unetNetworkObject = m_unetNetworkObjects[i];
						bool destroyNetworkObject = false;
						if (unetNetworkObject != null)
						{
							try
							{
								if ((unetNetworkObject.GetNetworkObjectData().NetID == CommunicationsController.Instance.NetworkID) || _force)
								{
									unetNetworkObject.SetPosition(unetNetworkObject.GetNetworkObjectData().gameObject.transform.position);
									unetNetworkObject.SetScale(unetNetworkObject.GetNetworkObjectData().gameObject.transform.localScale);
									unetNetworkObject.SetForward(unetNetworkObject.GetNetworkObjectData().gameObject.transform.forward);
								}
							}
							catch (Exception err)
							{
								destroyNetworkObject = true;
							}
						}
						if (destroyNetworkObject)
						{
							m_unetNetworkObjects.RemoveAt(i);
							i--;
						}
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		* Send the information about the local transforms
		*/
		void Update()
		{
			UpdateRemoteTransforms(false);
		}
	}
}
