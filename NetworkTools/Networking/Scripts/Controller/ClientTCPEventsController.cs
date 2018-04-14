using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Text;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * ClientGameTCPEvents
	 * 
	 * Singleton that handles the sending and receiving of the communications
	 * 
	 * @author Esteban Gallardo
	 */
	public class ClientTCPEventsController : MonoBehaviour
	{
		public const int MESSAGE_EVENT = 0;
		public const int MESSAGE_TRANSFORM = 1;

		// EVENTS
		public const string EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID = "EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID";
		public const string EVENT_CLIENT_TCP_LIST_OF_GAME_ROOMS = "EVENT_CLIENT_TCP_LIST_OF_GAME_ROOMS";
		public const string EVENT_CLIENT_TCP_ROOM_ID = "EVENT_CLIENT_TCP_ROOM_ID";
		public const string EVENT_CLIENT_TCP_CONNECTED_ROOM = "EVENT_CLIENT_TCP_CONNECTED_ROOM";
		public const string EVENT_SYSTEM_REQUEST_DATA_ROOM = "EVENT_SYSTEM_REQUEST_DATA_ROOM";

		public const string EVENT_CLIENT_TCP_PING_ALIVE = "EVENT_CLIENT_TCP_PING_ALIVE";
		public const string EVENT_CLIENT_TCP_REPONSE_ALIVE = "EVENT_CLIENT_TCP_REPONSE_ALIVE";

		public const string EVENT_CLIENT_TCP_TRANSFORM_DATA = "EVENT_CLIENT_TCP_TRANSFORM_DATA";

		public const string EVENT_CLIENT_TCP_PLAYER_UID = "EVENT_CLIENT_TCP_PLAYER_UID";

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------
		public const char TOKEN_SEPARATOR_EVENTS = '%';
		public const char TOKEN_SEPARATOR_PARTY = '@';
		public const char TOKEN_SEPARATOR_PLAYERS_IDS = ',';

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static ClientTCPEventsController _instance;

		public static ClientTCPEventsController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(ClientTCPEventsController)) as ClientTCPEventsController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						DontDestroyOnLoad(container);
						container.name = "ClientTCPEventsController";
						_instance = container.AddComponent(typeof(ClientTCPEventsController)) as ClientTCPEventsController;
					}
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// MEMBER VARIABLES
		// ----------------------------------------------	

		internal bool m_socketConnected = false;

		private TcpClient m_mySocket;
		private int m_uniqueNetworkID = -1;
		private int m_idNetworkServer = -1;

		private string m_uidPlayer = "null";

		private NetworkStream m_theStream;
		private StreamWriter m_theWriter;
		private StreamReader m_theReader;
		private BinaryWriter m_binWriter;
		private BinaryReader m_binReader;
		private string m_host;
		private Int32 m_port;

		private int m_room = -1;
		private int m_hostRoomID = -1;
		private List<string> m_events = new List<string>();
		private List<byte[]> m_transforms = new List<byte[]>();
		private List<ItemMultiTextEntry> m_roomsInvited = new List<ItemMultiTextEntry>();
		private List<ItemMultiTextEntry> m_roomsLobby = new List<ItemMultiTextEntry>();

		private List<PlayerConnectionData> m_playersConnections = new List<PlayerConnectionData>();

		public bool SocketConnected
		{
			get { return m_socketConnected; }
		}
		public int UniqueNetworkID
		{
			get { return m_uniqueNetworkID; }
		}
		public int ServerNetworkID
		{
			get { return m_idNetworkServer; }
		}
		public List<ItemMultiTextEntry> RoomsInvited
		{
			get { return m_roomsInvited; }
		}
		public List<ItemMultiTextEntry> RoomsLobby
		{
			get { return m_roomsLobby; }
		}

		// -------------------------------------------
		/* 
		 * Set up the connection with the server
		 */
		public void Initialitzation(string _host, int _port, int _room, int _hostRoomID)
		{
			if (m_socketConnected)
			{
				NetworkEventController.Instance.DelayLocalEvent(NetworkEventController.EVENT_SYSTEM_INITIALITZATION_LOCAL_COMPLETED, 1f, m_uniqueNetworkID);
				return;
			}

			m_room = _room;
			m_hostRoomID = _hostRoomID;

			try
			{
				m_host = _host;
				m_port = _port;
				m_mySocket = new TcpClient(m_host, m_port);
				m_theStream = m_mySocket.GetStream();
				m_theWriter = new StreamWriter(m_theStream);
				m_theReader = new StreamReader(m_theStream);
				m_binWriter = new BinaryWriter(m_theStream);
				m_binReader = new BinaryReader(m_theStream);

				NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
				UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);

				m_socketConnected = true;
			}
			catch (Exception e)
			{
#if DEBUG_MODE_DISPLAY_LOG
				Debug.LogError("ClientTCPEventsController::Init::CONNECTION ERROR WITH SERVER[" + m_host + "]::Socket error: " + e);
#endif
			}
		}

		private bool m_hasBeenDestroyed = false;

		// -------------------------------------------
		/* 
		 * Release resources
		 */
		public void Destroy()
		{
			if (m_hasBeenDestroyed) return;
			m_hasBeenDestroyed = true;

			NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS);
			NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
			UIEventController.Instance.UIEvent -= OnUIEvent;
			CloseSocket(true);
			DestroyObject(_instance.gameObject);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		 * Returns if the current machine acts as a server machine
		 */
		public bool IsServer()
		{
			return m_uniqueNetworkID == m_idNetworkServer;
		}

		// -------------------------------------------
		/* 
		 * Close the socket
		 */
		private void CloseSocket(bool _sendMessageDisconnect)
		{
			if (!m_socketConnected)
			{
				return;
			}

			try { if (m_theWriter != null) m_theWriter.Close(); } catch (Exception errw) { }
			try { if (m_theReader != null) m_theReader.Close(); } catch (Exception errR) { }
			try { if (m_mySocket != null) m_mySocket.Close(); } catch (Exception errS) { }

			m_socketConnected = false;
		}

		// -------------------------------------------
		/* 
		 * Write a string message
		 */
		private bool WriteSocket(string _message)
		{
			if (!m_socketConnected)
			{
				return false;
			}

			MemoryStream ms = new MemoryStream();
			BinaryWriter bw = new BinaryWriter(ms);
			bw.Write((byte)MESSAGE_EVENT);
			byte[] bytesMessage = Encoding.UTF8.GetBytes(_message);
			bw.Write(bytesMessage.Length);
			bw.Write(bytesMessage);

			byte[] bytesEvent = ms.ToArray();
			m_binWriter.Write(bytesEvent, 0, bytesEvent.Length);
			m_binWriter.Flush();

			return true;
		}

		// -------------------------------------------
		/* 
		 * Write a byte[] message
		 */
		private bool WriteSocket(byte[] _message)
		{
			if (!m_socketConnected)
			{
				return false;
			}

			// PROCESS TRANSFORMS
			try
			{
				m_binWriter.Write(_message, 0, _message.Length);
				m_binWriter.Flush();
			}
			catch (Exception err)
			{
				Destroy();
			}

			return true;
		}

		// -------------------------------------------
		/* 
		 * Read a string message if there is any available
		 */
		private bool ReadSocket()
		{
			if (!m_socketConnected)
			{
				return false;
			}
			if (m_theStream.DataAvailable)
			{
				int firstByte = (int)m_binReader.ReadByte();
				int sizeData = (int)m_binReader.ReadInt32();
				if (firstByte == MESSAGE_EVENT)
				{
					byte[] eventData = m_binReader.ReadBytes(sizeData);
					string message = System.Text.Encoding.UTF8.GetString(eventData);
					UnPackEventAndDispatch(message);
				}
				else
				{
					ReadTransformAndDispatch();
				}
			}
			if (m_events.Count > 0)
			{
				for (int i = 0; i < m_events.Count; i++)
				{
					WriteSocket(m_events[i]);
				}
				m_events.Clear();
			}
			else
			{
				for (int i = 0; i < m_transforms.Count; i++)
				{
					WriteSocket(m_transforms[i]);
				}
				m_transforms.Clear();
			}
			return true;
		}

		// -------------------------------------------
		/* 
		 * Makes the package string of the message that must be sent
		 */
		private string Pack(string _nameEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			string output = _nameEvent + TOKEN_SEPARATOR_EVENTS;
			output += m_uniqueNetworkID.ToString() + TOKEN_SEPARATOR_EVENTS;
			output += _networkOriginID.ToString() + TOKEN_SEPARATOR_EVENTS;
			output += _networkTargetID.ToString() + TOKEN_SEPARATOR_EVENTS;
			for (int i = 0; i < _list.Length; i++)
			{
				if (i < _list.Length - 1)
				{
					output += (string)_list[i] + TOKEN_SEPARATOR_EVENTS;
				}
				else
				{
					output += _list[i];
				}
			}
			return output;
		}

		// -------------------------------------------
		/* 
		* Manager of global events
		*/
		private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (m_uniqueNetworkID == -1) return;

			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS)
			{
				Destroy();
			}
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED)
			{
				int networkIDPlayer = int.Parse((string)_list[0]);
				if (networkIDPlayer != m_uniqueNetworkID)
				{
					if (ClientNewConnection(networkIDPlayer))
					{
						NetworkEventController.Instance.DispatchNetworkEvent(NetworkEventController.EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED, m_uniqueNetworkID.ToString());
					}
				}
			}
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_PLAYER_HAS_BEEN_DESTROYED)
			{
				int networkIDPlayer = int.Parse((string)_list[0]);
				ClientDisconnected(networkIDPlayer);
			}
			if (_nameEvent == NetworkEventController.EVENT_STREAMSERVER_REPORT_CLOSED_STREAM)
			{
				int networkIDPlayer = int.Parse((string)_list[0]);
				ClientDisconnected(networkIDPlayer);
			}
			if (!_isLocalEvent)
			{
				m_events.Add(Pack(_nameEvent, _networkOriginID, _networkTargetID, _list));
			}
			else
			{
				if (_nameEvent == EVENT_CLIENT_TCP_CONNECTED_ROOM)
				{
					int otherNetworkID = int.Parse((string)_list[0]);
#if DEBUG_MODE_DISPLAY_LOG
					Debug.LogError("EVENT_CLIENT_TCP_CONNECTED_ROOM::ASSIGNED OTHER CLIENT NUMBER[" + otherNetworkID + "] IN THE ROOM[" + m_room + "] WHERE THE SERVER IS[" + m_idNetworkServer + "]------------");
#endif
					NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED, otherNetworkID.ToString());
				}
			}
		}


		// -------------------------------------------
		/* 
		 * Manager of ui events
		 */
		private void OnUIEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == UIEventController.EVENT_SCREENMAINCOMMANDCENTER_REQUEST_LIST_USERS)
			{
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_LIST_USERS, m_playersConnections);
			}
		}

		// -------------------------------------------
		/* 
		 * UnPack a communication received and dispatch it to the system
		 */
		public void UnPackEventAndDispatch(string _package)
		{
#if DEBUG_MODE_DISPLAY_LOG
			Debug.LogError("****UnPackEventAndDispatch::_package=" + _package);
#endif

			// PROCESS ALL THE OTHER EVENTS
			string[] parameters = _package.Split(TOKEN_SEPARATOR_EVENTS);
			string nameEvent = parameters[0];

			// PROCESS PACKAGE
			if (m_uniqueNetworkID != -1)
			{
				if (parameters.Length > 3)
				{
					int uniqueNetworkID = int.Parse(parameters[1]);
					if (uniqueNetworkID == m_uniqueNetworkID)
					{
						// Debug.LogError("ClientTCPEventsController::EVENT[" + nameEvent + "] IGNORED BECAUSE IT CAME FROM THIS ORIGIN");
						return;
					}

					int originNetworkID = int.Parse(parameters[2]);
					int targetNetworkID = int.Parse(parameters[3]);
					string[] list = new string[parameters.Length - 4];
					for (int i = 4; i < parameters.Length; i++)
					{
						list[i - 4] = parameters[i];
					}

					NetworkEventController.Instance.DispatchCustomNetworkEvent(nameEvent, true, originNetworkID, targetNetworkID, list);
				}
			}
			else
			{
				// RETRIEVE THE UNIQUE NETWORK IDENTIFICATOR
				if (nameEvent == EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID)
				{
					m_room = int.Parse(parameters[4]);
					m_uidPlayer = parameters[5];
#if DEBUG_MODE_DISPLAY_LOG
					Debug.LogError("EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID::HAS BEEN ASSIGNED AVAILABLE ROOM[" + m_room + "]++++++++++");
#endif

					m_events.Add(Pack(EVENT_CLIENT_TCP_PLAYER_UID, -1, -1, (MultiplayerConfiguration.LoadIsRoomLobby() ? m_uidPlayer : FacebookController.Instance.Id)));

					if (GameObject.FindObjectOfType<YourNetworkTools>() == null)
					{
						MenuEventController.Instance.DispatchMenuEvent(EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID, m_room);
					}
					else
					{
						NetworkEventController.Instance.DelayLocalEvent(EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID, 1f, m_room);
					}
				}
				else
				{
					if (nameEvent == EVENT_CLIENT_TCP_LIST_OF_GAME_ROOMS)
					{
						string[] roomsInvited = parameters[4].Split(TOKEN_SEPARATOR_PARTY);
						m_roomsInvited.Clear();
						m_roomsLobby.Clear();
						for (int i = 0; i < roomsInvited.Length; i++)
						{
							string[] dataParty = roomsInvited[i].Split(TOKEN_SEPARATOR_PLAYERS_IDS);
							if (dataParty.Length > 1)
							{
								if (int.Parse(dataParty[0]) == 0)
								{
									m_roomsInvited.Add(new ItemMultiTextEntry(dataParty));
								}
								else
								{
									m_roomsLobby.Add(new ItemMultiTextEntry(dataParty));
								}
							}
						}
						MenuEventController.Instance.DispatchMenuEvent(EVENT_CLIENT_TCP_LIST_OF_GAME_ROOMS);
					}
					else
					{
						if (nameEvent == EVENT_CLIENT_TCP_CONNECTED_ROOM)
						{
							m_uniqueNetworkID = int.Parse(parameters[4]);
							m_idNetworkServer = int.Parse(parameters[5]);
							int totalNumberPlayers = int.Parse(parameters[6]);
							NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_SYSTEM_INITIALITZATION_LOCAL_COMPLETED, m_uniqueNetworkID);
							MenuEventController.Instance.DispatchMenuEvent(EVENT_CLIENT_TCP_CONNECTED_ROOM, totalNumberPlayers);
#if DEBUG_MODE_DISPLAY_LOG
							Debug.LogError("EVENT_CLIENT_TCP_CONNECTED_ROOM::ASSIGNED LOCAL CLIENT NUMBER[" + m_uniqueNetworkID + "] IN THE ROOM[" + m_room + "] WHERE THE SERVER IS[" + m_idNetworkServer + "]++++++++++");
#endif
						}
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * SendTranform
		 */
		public void SendTranform(int _netID, int _uID, int _indexPrefab, Vector3 _position, Vector3 _forward, Vector3 _scale)
		{
			int counter = 0;
			int totalSizePacket = 4 + 4 + 4 + (8 * 9);
			byte[] message = new byte[1 + 4 + totalSizePacket];
			message[0] = (byte)MESSAGE_TRANSFORM;
			counter++;
			Array.Copy(BitConverter.GetBytes(totalSizePacket), 0, message, counter, 4);
			counter += 4;
			Array.Copy(BitConverter.GetBytes(_netID), 0, message, counter, 4);
			counter += 4;
			Array.Copy(BitConverter.GetBytes(_uID), 0, message, counter, 4);
			counter += 4;
			Array.Copy(BitConverter.GetBytes(_indexPrefab), 0, message, counter, 4);
			counter += 4;
			// POSITION
			Array.Copy(BitConverter.GetBytes((double)_position.x), 0, message, counter, 8);
			counter += 8;
			Array.Copy(BitConverter.GetBytes((double)_position.y), 0, message, counter, 8);
			counter += 8;
			Array.Copy(BitConverter.GetBytes((double)_position.z), 0, message, counter, 8);
			counter += 8;
			// FORWARD
			Array.Copy(BitConverter.GetBytes((double)_forward.x), 0, message, counter, 8);
			counter += 8;
			Array.Copy(BitConverter.GetBytes((double)_forward.y), 0, message, counter, 8);
			counter += 8;
			Array.Copy(BitConverter.GetBytes((double)_forward.z), 0, message, counter, 8);
			counter += 8;
			// SCALE
			Array.Copy(BitConverter.GetBytes((double)_scale.x), 0, message, counter, 8);
			counter += 8;
			Array.Copy(BitConverter.GetBytes((double)_scale.y), 0, message, counter, 8);
			counter += 8;
			Array.Copy(BitConverter.GetBytes((double)_scale.z), 0, message, counter, 8);

			m_transforms.Add(message);
		}

		// -------------------------------------------
		/* 
		 * ReadTransformAndDispatch
		 */
		private void ReadTransformAndDispatch()
		{
			int netID = m_binReader.ReadInt32();
			int uID = m_binReader.ReadInt32();
			int indexPrefab = m_binReader.ReadInt32();

			// POSITION
			float posX = (float)m_binReader.ReadDouble();
			float posY = (float)m_binReader.ReadDouble();
			float posZ = (float)m_binReader.ReadDouble();
			// FORWARD
			float fwdX = (float)m_binReader.ReadDouble();
			float fwdY = (float)m_binReader.ReadDouble();
			float fwdZ = (float)m_binReader.ReadDouble();
			// SCALE
			float scaleX = (float)m_binReader.ReadDouble();
			float scaleY = (float)m_binReader.ReadDouble();
			float scaleZ = (float)m_binReader.ReadDouble();

			NetworkEventController.Instance.DispatchLocalEvent(EVENT_CLIENT_TCP_TRANSFORM_DATA, netID, uID, indexPrefab, new Vector3(posX, posY, posZ), new Vector3(fwdX, fwdY, fwdZ), new Vector3(scaleX, scaleY, scaleZ));
		}

		// -------------------------------------------
		/* 
		 * Send the creation of a new room with the friends' Facebook IDs
		 */
		public void CreateRoomForFriends(string[] _friendsIDs, string _extraData)
		{
			string friends = "";
			for (int i = 0; i < _friendsIDs.Length; i++)
			{
				if (friends.Length > 0)
				{
					friends += TOKEN_SEPARATOR_PLAYERS_IDS;
				}
				friends += _friendsIDs[i];
			}

			m_events.Add(Pack(EVENT_CLIENT_TCP_ROOM_ID, -1, -1, m_room.ToString(), "0", FacebookController.Instance.NameHuman, m_hostRoomID.ToString(), friends, _extraData));
		}

		// -------------------------------------------
		/* 
		 * Send the creation of a new room with the friends' Facebook IDs
		 */
		public void CreateRoomForFriends(int _room, string[] _friendsIDs, string _extraData)
		{
			m_room = _room;
			CreateRoomForFriends(_friendsIDs, _extraData);
		}

		// -------------------------------------------
		/* 
		 * Accept the invitation and join a room
		 */
		public void JoinRoomForFriends(int _room, string _players, string _extraData)
		{
			m_room = _room;
			m_events.Add(Pack(EVENT_CLIENT_TCP_ROOM_ID, -1, -1, m_room.ToString(), "0", FacebookController.Instance.NameHuman, m_hostRoomID.ToString(), _players, _extraData));
		}

		// -------------------------------------------
		/* 
		 * Returns a well formed list of players
		 */
		public static string GetPlayersString(int _playerNumber)
		{
			string players = "";
			for (int i = 0; i < _playerNumber; i++)
			{
				if (players.Length > 0)
				{
					players += TOKEN_SEPARATOR_PLAYERS_IDS;
				}
				players += "PLAYER_LOBBY_" + i;
			}

			return players;
		}

		// -------------------------------------------
		/* 
		 * Send the creation of a new room for the lobby
		 */
		public void CreateRoomForLobby(string _nameRoom, int _playerNumber, string _extraData)
		{
			m_events.Add(Pack(EVENT_CLIENT_TCP_ROOM_ID, -1, -1, m_room.ToString(), "1", _nameRoom, m_hostRoomID.ToString(), GetPlayersString(_playerNumber), _extraData));
		}

		// -------------------------------------------
		/* 
		 * Send the creation of a new room for the lobby
		 */
		public void CreateRoomForLobby(int _room, string _nameRoom, int _playerNumber, string _extraData)
		{
			m_room = _room;
			CreateRoomForLobby(_nameRoom, _playerNumber, _extraData);
		}

		// -------------------------------------------
		/* 
		 * Join an existing room
		 */
		public void JoinRoomOfLobby(int _room, string _players, string _extraData)
		{
			m_room = _room;
			m_events.Add(Pack(EVENT_CLIENT_TCP_ROOM_ID, -1, -1, m_room.ToString(), "1", m_uidPlayer, m_hostRoomID.ToString(), _players, _extraData));
		}

		private float m_timeoutForPing = 0;

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
		* New client has been connected
		*/
		public bool ClientNewConnection(int _idConnection)
		{
			PlayerConnectionData newPlayerConnection = new PlayerConnectionData(_idConnection, null);
			if (!m_playersConnections.Contains(newPlayerConnection))
			{
				m_playersConnections.Add(newPlayerConnection);
				string eventConnected = CommunicationsController.CreateJSONMessage(_idConnection, CommunicationsController.MESSAGE_TYPE_NEW_CONNECTION);
				Debug.Log(eventConnected);
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_LIST_USERS, m_playersConnections);
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_REGISTER_LOG, eventConnected);
				return true;
			}
			else
			{
				return false;
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
				string eventDisconnected = CommunicationsController.CreateJSONMessage(_idConnection, CommunicationsController.MESSAGE_TYPE_DISCONNECTION);
				Debug.Log(eventDisconnected);
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_LIST_USERS, m_playersConnections);
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_REGISTER_LOG, eventDisconnected);
				NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_PLAYERCONNECTIONDATA_USER_DISCONNECTED, _idConnection);
			}
		}

		// -------------------------------------------
		/* 
		* Display information about the operation mode
		*/
		void OnGUI()
		{
			if (MultiplayerConfiguration.DEBUG_MODE)
			{
				GUILayout.BeginVertical();
				if (m_uniqueNetworkID == -1)
				{
					GUILayout.Label(new GUIContent("--[SOCKET]--SERVER IS SETTING UP. WAIT..."));
				}
				else
				{
					GUILayout.Label(new GUIContent("++[SOCKET]++MACHINE CONNECTION[" + m_uniqueNetworkID + "][" + (IsServer() ? "SERVER" : "CLIENT") + "]"));
				}
				GUILayout.EndVertical();
			}
		}

		// -------------------------------------------
		/* 
		 * In the main loop it will keep listening to the received communications
		 */
		public void Update()
		{
			ReadSocket();

			// PING
			m_timeoutForPing += Time.deltaTime;
			if (m_timeoutForPing > 2)
			{
				m_timeoutForPing = 0;
				m_events.Add(EVENT_CLIENT_TCP_REPONSE_ALIVE);
			}
		}

	}
}