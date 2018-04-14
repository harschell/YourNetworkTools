using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine.SceneManagement;

namespace YourNetworkingTools
{
	public delegate void MenuEventHandler(string _nameEvent, params object[] _list);

	/******************************************
	* 
	* MenuBasicEventController
	* 
	* Class used to dispatch events through all the system
	* 
	* @author Esteban Gallardo
	*/
	public class MenuEventController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------
		public const string EVENT_MENUEVENTCONTROLLER_LOAD_GAME_SCENE = "EVENT_MENUEVENTCONTROLLER_LOAD_GAME_SCENE";
		public const string EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE = "EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE";
		public const string EVENT_SYSTEM_ANDROID_BACK_BUTTON = "EVENT_SYSTEM_ANDROID_BACK_BUTTON";

		public const string ACTION_KEY_UP = "ACTION_KEY_UP";
		public const string ACTION_KEY_DOWN = "ACTION_KEY_DOWN";
		public const string ACTION_KEY_LEFT = "ACTION_KEY_LEFT";
		public const string ACTION_KEY_RIGHT = "ACTION_KEY_RIGHT";
		public const string ACTION_BUTTON_DOWN = "ACTION_BUTTON_DOWN";

		// ----------------------------------------------
		// HANDLER
		// ----------------------------------------------
		public event MenuEventHandler MenuEvent;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static MenuEventController instance;

		public static MenuEventController Instance
		{
			get
			{
				if (!instance)
				{
					instance = GameObject.FindObjectOfType(typeof(MenuEventController)) as MenuEventController;
					if (!instance)
					{
						GameObject container = new GameObject();
						container.name = "MenuEventController";
						instance = container.AddComponent(typeof(MenuEventController)) as MenuEventController;
					}
				}
				return instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private List<TimedEventData> m_listEvents = new List<TimedEventData>();
		private bool m_keyInputActivated = false;

		private string m_nameRoomLobby = "";
		private bool m_isLobbyMode = false;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------
		public bool KeyInputActivated
		{
			get { return m_keyInputActivated; }
		}
		public string NameRoomLobby
		{
			get { return m_nameRoomLobby; }
		}
		public bool IsLobbyMode
		{
			get { return m_isLobbyMode; }
		}

		// -------------------------------------------
		/* 
		* Constructor
		*/
		private MenuEventController()
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
		* Will dispatch a menu event
		*/
		public void DispatchMenuEvent(string _nameEvent, params object[] _list)
		{
			// Debug.Log("[MENU EVENT]_nameEvent=" + _nameEvent);
			if (MenuEvent != null) MenuEvent(_nameEvent, _list);
		}

		// -------------------------------------------
		/* 
		 * Will add a new delayed event to the queue
		 */
		public void DelayMenuEvent(string _nameEvent, float _time, params object[] _list)
		{
			m_listEvents.Add(new TimedEventData(_nameEvent, _time, _list));
		}

		// -------------------------------------------
		/* 
		 * Gets the input keys
		 */
		private void GetKeyCodeInput()
		{
			// ARROWS KEYPAD
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				DispatchMenuEvent(ACTION_KEY_LEFT);
				m_keyInputActivated = true;
			}
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				DispatchMenuEvent(ACTION_KEY_RIGHT);
				m_keyInputActivated = true;
			}
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				DispatchMenuEvent(ACTION_KEY_UP);
				m_keyInputActivated = true;
			}
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				DispatchMenuEvent(ACTION_KEY_DOWN);
				m_keyInputActivated = true;
			}

#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.LeftControl))
			{
				DispatchMenuEvent(ACTION_BUTTON_DOWN);
			}
#else
	try {
			if (Input.GetMouseButton(0))
			{
				DispatchMenuEvent(ACTION_BUTTON_DOWN);
			}
			else
			{
				if (Input.GetKeyDown("Fire1"))
				{
					DispatchMenuEvent(ACTION_BUTTON_DOWN);
				}
			}
	} catch (Exception err) {}
#endif
		}

		// -------------------------------------------
		/* 
		 * Set if we are going to go to the lobby
		 */
		public void MenuController_SetLobbyMode(bool _value)
		{
			m_isLobbyMode = _value;
			MultiplayerConfiguration.SaveIsRoomLobby(m_isLobbyMode);
		}

		// -------------------------------------------
		/* 
		 * Set the name of the room lobby
		 */
		public void MenuController_SetNameRoomLobby(string _value)
		{
			m_nameRoomLobby = _value;
		}

		// -------------------------------------------
		/* 
		 * Set if the connection is going to be local(UNET) or global(Sockets)
		 */
		public void MenuController_SetLocalGame(bool _value)
		{
			YourNetworkTools.SetLocalGame(_value);
		}

		// -------------------------------------------
		/* 
		 * Will save the number of players
		 */
		public void MenuController_SaveNumberOfPlayers(int _value)
		{
			MultiplayerConfiguration.SaveNumberOfPlayers(_value);
		}

		// -------------------------------------------
		/* 
		 * Will load the number of players
		 */
		public int MenuController_LoadNumberOfPlayers()
		{
			return MultiplayerConfiguration.LoadNumberOfPlayers();
		}

		// -------------------------------------------
		/* 
		 * Will create a new room for lobby
		 */
		public void MenuController_CreateNewLobbyRoom(string _nameLobby, int _finalNumberOfPlayers, string _extraData)
		{
			// CREATE ROOM IN LOBBY
			MultiplayerConfiguration.SaveNameRoomLobby(_nameLobby);
#if ENABLE_BALANCE_LOADER
			MenuEventController.Instance.DispatchMenuEvent(EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE);
			HTTPController.Instance.CreateNewRoom(true, _nameLobby, ClientTCPEventsController.GetPlayersString(_finalNumberOfPlayers), _extraData);
#else
			MenuController_CreateRoomForLobby(_nameLobby, _finalNumberOfPlayers, _extraData);
#endif
		}

		// -------------------------------------------
		/* 
		 * Will create a new room for friends
		 */
		public void MenuController_CreateNewFacebookRoom(string _friends, List<string> _friendsIDs, string _extraData)
		{
			// CREATE ROOM IN FACEBOOK
#if ENABLE_BALANCE_LOADER
			MenuEventController.instance.DispatchMenuEvent(EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE);
			MultiplayerConfiguration.SaveFriendsGame(_friends);
			MultiplayerConfiguration.SaveNumberOfPlayers(_friends.Split(',').Length);
			HTTPController.Instance.CreateNewRoom(false, FacebookController.Instance.NameHuman, _friends, _extraData);
#else
			ClientTCPEventsController.Instance.CreateRoomForFriends(_friendsIDs.ToArray(), _extraData);
#endif
		}

		// -------------------------------------------
		/* 
		 * Will create the socket connection
		 */
		public void MenuController_InitialitzationSocket(int _numberRoom, int _idMachineHost)
		{
#if !ENABLE_BALANCE_LOADER
			MultiplayerConfiguration.SaveIPAddressServer(MultiplayerConfiguration.SOCKET_SERVER_ADDRESS);
			MultiplayerConfiguration.SavePortServer(MultiplayerConfiguration.PORT_SERVER_ADDRESS);
#endif
			ClientTCPEventsController.Instance.Initialitzation(MultiplayerConfiguration.LoadIPAddressServer(), MultiplayerConfiguration.LoadPortServer(), MultiplayerConfiguration.LoadRoomNumberInServer(_numberRoom), MultiplayerConfiguration.LoadMachineIDServer(_idMachineHost));
		}

		// -------------------------------------------
		/* 
		 * Will connect the socket to create the lobby
		 */
		public void MenuController_CreateRoomForLobby(string _nameLobby, int _finalNumberOfPlayers, string _extraData)
		{
			ClientTCPEventsController.Instance.CreateRoomForLobby(_nameLobby, _finalNumberOfPlayers, _extraData);
		}

		// -------------------------------------------
		/* 
		 * Will join to an existing room of friends
		 */
		public void MenuController_JoinRoomForFriends(int _room, string _players, string _extraData)
		{
			ClientTCPEventsController.Instance.JoinRoomForFriends(_room, _players, _extraData);
		}

		// -------------------------------------------
		/* 
		 * Will join to an existing room of the lobby
		 */
		public void MenuController_JoinRoomOfLobby(int _room, string _players, string _extraData)
		{
			ClientTCPEventsController.Instance.JoinRoomOfLobby(_room, _players, _extraData);
		}

		// -------------------------------------------
		/* 
		 * Will save the room number we should connect
		 */
		public void MenuController_SaveRoomNumberInServer(int _value)
		{
			MultiplayerConfiguration.SaveRoomNumberInServer(_value);
		}

		// -------------------------------------------
		/* 
		 * We save the IP address we should connect
		 */
		public void MenuController_SaveIPAddressServer(string _value)
		{
			MultiplayerConfiguration.SaveIPAddressServer(_value);
		}

		// -------------------------------------------
		/* 
		 * Will save the port number of the host
		 */
		public void MenuController_SavePortServer(int _value)
		{
			MultiplayerConfiguration.SavePortServer(_value);
		}

		// -------------------------------------------
		/* 
		 * Will save the ID of the machine which is hosting the room
		 */
		public void MenuController_SaveMachineIDServer(int _value)
		{
			MultiplayerConfiguration.SaveMachineIDServer(_value);
		}

		// -------------------------------------------
		/* 
		 * Will load the game scene after 1 second delay
		 */
		public void MenuController_LoadGameScene()
		{
			StartCoroutine(LoadScene());
		}

		// -------------------------------------------
		/* 
		 * LoadGameScene
		 */
		IEnumerator LoadScene()
		{
			yield return new WaitForSeconds(0.1f);
			SceneManager.LoadScene(MenuScreenController.Instance.TargetGameScene);
		}


		// -------------------------------------------
		/* 
		 * Will process the queue of delayed events 
		 */
		void Update()
		{
			// DELAYED EVENTS
			for (int i = 0; i < m_listEvents.Count; i++)
			{
				TimedEventData eventData = m_listEvents[i];
				eventData.Time -= Time.deltaTime;
				if (eventData.Time <= 0)
				{
					MenuEvent(eventData.NameEvent, eventData.List);
					eventData.Destroy();
					m_listEvents.RemoveAt(i);
					break;
				}
			}

			GetKeyCodeInput();
		}
	}
}
