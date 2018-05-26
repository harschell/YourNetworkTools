using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YourCommonTools;

namespace YourNetworkingTools
{

	public enum ScreenTypePreviousActionEnum
	{
		DESTROY_ALL_SCREENS = 0x00,
		DESTROY_CURRENT_SCREEN = 0x01,
		KEEP_CURRENT_SCREEN = 0x02,
		HIDE_CURRENT_SCREEN = 0x03
	}

	/******************************************
	 * 
	 * MenuScreenController
	 * 
	 * ScreenManager controller that handles all the screens's creation and disposal
	 * 
	 * @author Esteban Gallardo
	 */
	public class MenuScreenController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_MENU_SCREENMANAGER_OPEN_GENERIC_SCREEN = "EVENT_MENU_SCREENMANAGER_OPEN_GENERIC_SCREEN";
		public const string EVENT_MENU_SCREENMANAGER_OPEN_INFORMATION_SCREEN = "EVENT_MENU_SCREENMANAGER_OPEN_INFORMATION_SCREEN";
		public const string EVENT_MENU_SCREENMANAGER_DESTROY_SCREEN = "EVENT_MENU_SCREENMANAGER_DESTROY_SCREEN";
		public const string EVENT_MENU_SCREENMANAGER_DESTROY_ALL_SCREEN = "EVENT_MENU_SCREENMANAGER_DESTROY_ALL_SCREEN";
		public const string EVENT_MENU_GENERIC_MESSAGE_INFO_OK_BUTTON = "EVENT_MENU_GENERIC_MESSAGE_INFO_OK_BUTTON";

		public const string EVENT_MENU_CHANGED_PAGE_POPUP = "EVENT_MENU_CHANGED_PAGE_POPUP";
		public const string EVENT_MENU_CONFIRMATION_POPUP = "EVENT_MENU_CONFIRMATION_POPUP";
		public const string EVENT_MENU_FORCE_DESTRUCTION_POPUP = "EVENT_MENU_FORCE_DESTRUCTION_POPUP";
		public const string EVENT_MENU_FORCE_TRIGGER_OK_BUTTON = "EVENT_MENU_FORCE_TRIGGER_OK_BUTTON";

		public const string EVENT_MENU_SCREENMANAGER_REPORT_DESTROYED = "EVENT_MENU_SCREENMANAGER_REPORT_DESTROYED";

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static MenuScreenController instance;

		public static MenuScreenController Instance
		{
			get
			{
				if (!instance)
				{
					instance = GameObject.FindObjectOfType(typeof(MenuScreenController)) as MenuScreenController;
				}
				return instance;
			}
		}


		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		[Tooltip("Target scene where the real application is")]
		public string TargetGameScene;

		[Tooltip("It allows the debug of most common messages")]
		public bool DebugMode = true;

		[Tooltip("All the screens used by the application")]
		public GameObject[] ScreensPrefabs;

		[Tooltip("Application instruction images")]
		public Sprite[] Instructions;

		[Tooltip("Custom selector menu buttons graphic")]
		public Sprite SelectorGraphic;

		[Tooltip("Maximum number of players allowed")]
		public int MaxPlayers = 5;

		[Tooltip("Fix the number of players of a game")]
		public int ForceFixedPlayers = -1;

		[Tooltip("Name of the game options screen")]
		public string ScreenGameOptions = "";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------
		public Sprite IconApp;
		public Sprite LogoApp;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private List<GameObject> m_screensPool = new List<GameObject>();
		private List<GameObject> m_screensOverlay = new List<GameObject>();
		private bool m_enableScreens = true;
		private bool m_enableDebugTestingCode = false;

		private bool m_isFriendsRoom = false;
		private int m_numberOfPlayers = -1;
		private string m_friends;
		private List<string> m_friendsIDs;
		private string m_extraData = "extraData";

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public bool EnableDebugTestingCode
		{
			get { return m_enableDebugTestingCode; }
			set { m_enableDebugTestingCode = value; }
		}
		public string ExtraData
		{
			get { return m_extraData; }
			set { m_extraData = value; }
		}

		// -------------------------------------------
		/* 
		 * Initialitzation listener
		 */
		public virtual void Start()
		{
			if (DebugMode)
			{
				Debug.Log("YourVRUIScreenController::Start::First class to initialize for the whole system to work");
			}

			MenuEventController.Instance.MenuEvent += new MenuEventHandler(OnMenuEvent);

			Screen.orientation = ScreenOrientation.Portrait;

			LanguageController.Instance.Initialize();
			SoundsController.Instance.Initialize();

#if UNITY_EDITOR
			CreateNewScreen(ScreenMenuMainView.SCREEN_NAME, ScreenTypePreviousActionEnum.DESTROY_ALL_SCREENS, true);
			// CreateNewScreen(ScreenSplashView.SCREEN_NAME, ScreenTypePreviousActionEnum.DESTROY_ALL_SCREENS, true);
#else
		CreateNewScreen(ScreenSplashView.SCREEN_NAME, ScreenTypePreviousActionEnum.DESTROY_ALL_SCREENS, true);        
#endif
		}

		// -------------------------------------------
		/* 
		 * Destroy all references
		 */
		void OnDestroy()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		* Destroy all references
		*/
		public void Destroy()
		{
			DestroyScreensPool();
			DestroyScreensOverlay();
			MenuEventController.Instance.MenuEvent -= OnMenuEvent;
			instance = null;
		}

		// -------------------------------------------
		/* 
		 * Create a new screen
		 */
		public void CreateNewInformationScreen(string _nameScreen, ScreenTypePreviousActionEnum _previousAction, string _title, string _description, Sprite _image, string _eventData)
		{
			List<PageInformationData> pages = new List<PageInformationData>();
			pages.Add(new PageInformationData(_title, _description, _image, _eventData));

			CreateNewScreen(_nameScreen, _previousAction, false, pages);
		}

		// -------------------------------------------
		/* 
		* Create a new screen
		*/
		public void CreateNewScreen(string _nameScreen, ScreenTypePreviousActionEnum _previousAction, bool _hidePreviousScreens, params object[] _list)
		{
			if (!m_enableScreens) return;

			if (DebugMode)
			{
				Debug.Log("EVENT_SCREENMANAGER_OPEN_SCREEN::Creating the screen[" + _nameScreen + "]");
			}
			if (_hidePreviousScreens)
			{
				EnableAllScreens(false);
			}

			// PREVIOUS ACTION
			switch (_previousAction)
			{
				case ScreenTypePreviousActionEnum.HIDE_CURRENT_SCREEN:
					if (m_screensPool.Count > 0)
					{
						m_screensPool[m_screensPool.Count - 1].GetComponent<IBasicView>().SetActivation(false);
					}
					break;

				case ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN:
					if (m_screensPool.Count > 0)
					{
						m_screensPool[m_screensPool.Count - 1].GetComponent<IBasicView>().SetActivation(false);
					}
					break;

				case ScreenTypePreviousActionEnum.DESTROY_CURRENT_SCREEN:
					if (m_screensPool.Count > 0)
					{
						GameObject sCurrentScreen = m_screensPool[m_screensPool.Count - 1];
						if (sCurrentScreen.GetComponent<IBasicView>() != null)
						{
							sCurrentScreen.GetComponent<IBasicView>().Destroy();
						}
						GameObject.Destroy(sCurrentScreen);
						m_screensPool.RemoveAt(m_screensPool.Count - 1);
					}
					break;

				case ScreenTypePreviousActionEnum.DESTROY_ALL_SCREENS:
					DestroyScreensPool();
					DestroyScreensOverlay();
					break;
			}

			// CREATE SCREEN
			GameObject currentScreen = null;
			for (int i = 0; i < ScreensPrefabs.Length; i++)
			{
				if (ScreensPrefabs[i].name == _nameScreen)
				{
					currentScreen = (GameObject)Instantiate(ScreensPrefabs[i]);
					currentScreen.GetComponent<IBasicView>().Initialize(_list);
					break;
				}
			}

			if (_hidePreviousScreens)
			{
				m_screensPool.Add(currentScreen);
			}
			else
			{
				m_screensOverlay.Add(currentScreen);
			}
		}

		// -------------------------------------------
		/* 
		 * Destroy all the screens in memory
		 */
		public void DestroyScreensPool()
		{
			for (int i = 0; i < m_screensPool.Count; i++)
			{
				if (m_screensPool[i] != null)
				{
					if (m_screensPool[i].GetComponent<IBasicView>() != null)
					{
						m_screensPool[i].GetComponent<IBasicView>().Destroy();
					}
					GameObject.Destroy(m_screensPool[i]);
					m_screensPool[i] = null;
				}
			}
			m_screensPool.Clear();
		}

		// -------------------------------------------
		/* 
		 * Destroy all the screens in memory
		 */
		public void DestroyScreensOverlay()
		{
			for (int i = 0; i < m_screensOverlay.Count; i++)
			{
				if (m_screensOverlay[i] != null)
				{
					if (m_screensOverlay[i].GetComponent<IBasicView>() != null)
					{
						m_screensOverlay[i].GetComponent<IBasicView>().Destroy();
					}
					GameObject.Destroy(m_screensOverlay[i]);
					m_screensOverlay[i] = null;
				}
			}
			m_screensOverlay.Clear();
		}


		// -------------------------------------------
		/* 
		 * Remove the screen from the list of screens
		 */
		private void DestroyGameObjectSingleScreen(GameObject _screen, bool _runDestroy)
		{
			if (_screen == null) return;

			for (int i = 0; i < m_screensPool.Count; i++)
			{
				GameObject screen = (GameObject)m_screensPool[i];
				if (_screen == screen)
				{
					if (_runDestroy)
					{
						screen.GetComponent<IBasicView>().Destroy();
					}
					m_screensPool.RemoveAt(i);
					return;
				}
			}

			for (int i = 0; i < m_screensOverlay.Count; i++)
			{
				GameObject screen = (GameObject)m_screensOverlay[i];
				if (_screen == screen)
				{
					if (_runDestroy)
					{
						screen.GetComponent<IBasicView>().Destroy();
					}
					m_screensOverlay.RemoveAt(i);
					return;
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Changes the enable of the screens
		 */
		private void EnableAllScreens(bool _activation)
		{
			for (int i = 0; i < m_screensPool.Count; i++)
			{
				if (m_screensPool[i] != null)
				{
					if (m_screensPool[i].GetComponent<IBasicView>() != null)
					{
						m_screensPool[i].GetComponent<IBasicView>().SetActivation(_activation);
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		protected virtual void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EVENT_MENU_SCREENMANAGER_OPEN_GENERIC_SCREEN)
			{
				string nameScreen = (string)_list[0];
				ScreenTypePreviousActionEnum previousAction = (ScreenTypePreviousActionEnum)_list[1];
				bool hidePreviousScreens = (bool)_list[2];
				CreateNewScreen(nameScreen, previousAction, hidePreviousScreens);
			}
			if (_nameEvent == EVENT_MENU_SCREENMANAGER_OPEN_INFORMATION_SCREEN)
			{
				string nameScreen = (string)_list[0];
				ScreenTypePreviousActionEnum previousAction = (ScreenTypePreviousActionEnum)_list[1];
				bool hidePreviousScreens = (bool)_list[2];
				string title = (string)_list[3];
				string description = (string)_list[4];
				Sprite image = (Sprite)_list[5];
				string eventData = (string)_list[6];
				List<PageInformationData> pages = new List<PageInformationData>();
				pages.Add(new PageInformationData(title, description, image, eventData));
				CreateNewScreen(nameScreen, previousAction, false, pages);
			}
			if (_nameEvent == EVENT_MENU_SCREENMANAGER_DESTROY_SCREEN)
			{
				m_enableScreens = true;
				GameObject screen = (GameObject)_list[0];
				DestroyGameObjectSingleScreen(screen, true);
				if (m_screensPool.Count > 0)
				{
					m_screensPool[m_screensPool.Count - 1].GetComponent<IBasicView>().SetActivation(true);
				}
			}
			if (_nameEvent == EVENT_MENU_SCREENMANAGER_DESTROY_ALL_SCREEN)
			{
				DestroyScreensOverlay();
				DestroyScreensPool();
			}
			if (_nameEvent == EVENT_MENU_CONFIRMATION_POPUP)
			{
				GameObject screen = (GameObject)_list[0];
				bool accepted = (bool)_list[1];
				string subnameEvent = (string)_list[2];
			}

			OnMenuEventRoomConnected(_nameEvent, _list);
		}

		// -------------------------------------------
		/* 
		 * Create the room in server
		 */
		public void CreateRoomInServer(int _finalNumberOfPlayers, string _extraData)
		{
			// NUMBER OF PLAYERS
			int finalNumberOfPlayers = _finalNumberOfPlayers;
			if ((finalNumberOfPlayers > 0) && (finalNumberOfPlayers <= MaxPlayers))
			{
				MenuEventController.Instance.MenuController_SaveNumberOfPlayers(finalNumberOfPlayers);
				if (MenuEventController.Instance.IsLobbyMode)
				{
					if (MenuEventController.Instance.NameRoomLobby.Length > 0)
					{
						MenuEventController.Instance.MenuController_CreateNewLobbyRoom(MenuEventController.Instance.NameRoomLobby, finalNumberOfPlayers, _extraData);
					}
					else
					{
						MenuEventController.Instance.DispatchMenuEvent(MenuScreenController.EVENT_MENU_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
						MenuScreenController.Instance.CreateNewInformationScreen(ScreenMenuInformationView.SCREEN_INFORMATION, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.there.is.no.name.for.room"), null, "");
					}
				}
				else
				{
					if (MenuScreenController.Instance.ScreenGameOptions.Length > 0)
					{
						MenuScreenController.Instance.CreateNewScreen(MenuScreenController.Instance.ScreenGameOptions, ScreenTypePreviousActionEnum.DESTROY_ALL_SCREENS, false, null);
					}
					else
					{
						MenuScreenController.Instance.CreateNewScreen(ScreenMenuLoadingView.SCREEN_NAME, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, null);
						MenuEventController.Instance.MenuController_LoadGameScene();
					}
				}
			}
			else
			{
				MenuEventController.Instance.DispatchMenuEvent(MenuScreenController.EVENT_MENU_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
				MenuScreenController.Instance.CreateNewInformationScreen(ScreenMenuInformationView.SCREEN_INFORMATION, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.player.number.not.right.number"), null, "");
			}
		}

		// -------------------------------------------
		/* 
		 * Will create the game or it will load a custom game screen
		 */
		public void LoadCustomGameScreenOrCreateGame(bool _isFriendsRoom, int _numberOfPlayers, string _friends, List<string> _friendsIDs)
		{
			m_isFriendsRoom = _isFriendsRoom;
			m_numberOfPlayers = _numberOfPlayers;
			m_friends = _friends;
			if (_friendsIDs != null)
			{
				m_friendsIDs = new List<string>();
				for (int i = 0; i < _friendsIDs.Count; i++)
				{
					m_friendsIDs.Add(_friendsIDs[i]);
				}
			}

			if (m_isFriendsRoom)
			{
				m_numberOfPlayers = m_friendsIDs.Count;
			}

			MenuEventController.Instance.MenuController_SaveNumberOfPlayers(m_numberOfPlayers);
			if (MenuScreenController.Instance.ScreenGameOptions.Length > 0)
			{
				MenuScreenController.Instance.CreateNewScreen(MenuScreenController.Instance.ScreenGameOptions, ScreenTypePreviousActionEnum.DESTROY_ALL_SCREENS, false, null);
			}
			else
			{
				CreateOrJoinRoomInServer(false);
			}
		}

		// -------------------------------------------
		/* 
		 * Create for real the room in server
		 */
		public void CreateOrJoinRoomInServer(bool _checkScreenGameOptions)
		{
			if (MenuEventController.Instance.MenuController_LoadNumberOfPlayers() == MultiplayerConfiguration.VALUE_FOR_JOINING)
			{
				if (_checkScreenGameOptions && (MenuScreenController.Instance.ScreenGameOptions.Length > 0))
				{
					MenuScreenController.Instance.CreateNewScreen(MenuScreenController.Instance.ScreenGameOptions, ScreenTypePreviousActionEnum.DESTROY_ALL_SCREENS, false, null);
				}
				else
				{
					MenuScreenController.Instance.CreateNewScreen(ScreenMenuLoadingView.SCREEN_NAME, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, null);
					MultiplayerConfiguration.SaveExtraData(m_extraData);
					if (!YourNetworkTools.GetIsLocalGame())
					{						
						JoinARoomInServer();
					}
					else
					{
						MenuEventController.Instance.MenuController_LoadGameScene();
					}
				}
			}
			else
			{
				if (!YourNetworkTools.GetIsLocalGame())
				{
					if (m_isFriendsRoom)
					{
						MenuEventController.Instance.MenuController_CreateNewFacebookRoom(m_friends, m_friendsIDs, m_extraData);
					}
					else
					{
						MenuScreenController.Instance.CreateRoomInServer(m_numberOfPlayers, m_extraData);
					}
				}
				else
				{
					MultiplayerConfiguration.SaveExtraData(m_extraData);
					MenuScreenController.Instance.CreateNewScreen(ScreenMenuLoadingView.SCREEN_NAME, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, null);
					MenuEventController.Instance.MenuController_LoadGameScene();
				}
			}
		}


		// -------------------------------------------
		/* 
		* Client has selected a room to join
		*/
		public void JoinARoomInServer()
		{
			if (MenuEventController.Instance.IsLobbyMode)
			{
				// JOIN ROOM IN LOBBY
#if ENABLE_BALANCE_LOADER
				MenuScreenController.Instance.CreateNewScreen(ScreenMenuLoadingView.SCREEN_NAME, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, null);
				MenuEventController.Instance.MenuController_LoadGameScene();
#else
				MenuEventController.Instance.MenuController_JoinRoomOfLobby(MultiplayerConfiguration.LoadRoomNumberInServer(-1), "null", "extraData");
#endif
			}
			else
			{
				// JOIN ROOM IN FACEBOOK
#if ENABLE_BALANCE_LOADER
				MenuScreenController.Instance.CreateNewScreen(ScreenMenuLoadingView.SCREEN_NAME, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, null);
				MenuEventController.Instance.MenuController_LoadGameScene();
#else
				MenuEventController.Instance.MenuController_JoinRoomForFriends(MultiplayerConfiguration.LoadRoomNumberInServer(-1), "null", "extraData");
#endif
			}
		}

		// -------------------------------------------
		/* 
		* Process the room connection
		*/
		private void OnMenuEventRoomConnected(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_CONNECTED_ROOM)
			{
				MenuEventController.Instance.MenuController_SaveNumberOfPlayers((int)_list[0]);
				MenuScreenController.Instance.CreateNewScreen(ScreenMenuLoadingView.SCREEN_NAME, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, null);
				MenuEventController.Instance.MenuController_LoadGameScene();
			}
			if (_nameEvent == MenuEventController.EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE)
			{
				MenuScreenController.Instance.CreateNewScreen(ScreenMenuLoadingView.SCREEN_NAME, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, null);
			}
			if (_nameEvent == CreateNewRoomHTTP.EVENT_CLIENT_HTTP_NEW_ROOM_CREATED)
			{
				// CREATE ROOM IN LOBBY
				MenuEventController.Instance.DispatchMenuEvent(MenuScreenController.EVENT_MENU_FORCE_DESTRUCTION_POPUP);
				if (_list.Length == 4)
				{
					MenuEventController.Instance.MenuController_SaveRoomNumberInServer((int)_list[0]);
					MenuEventController.Instance.MenuController_SaveIPAddressServer((string)_list[1]);
					MenuEventController.Instance.MenuController_SavePortServer((int)_list[2]);
					MenuEventController.Instance.MenuController_SaveMachineIDServer((int)_list[3]);
					MenuScreenController.Instance.CreateNewScreen(ScreenMenuLoadingView.SCREEN_NAME, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, null);
					MenuEventController.Instance.MenuController_LoadGameScene();
				}
				else
				{
					MenuEventController.Instance.DispatchMenuEvent(MenuScreenController.EVENT_MENU_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
					MenuScreenController.Instance.CreateNewInformationScreen(ScreenMenuInformationView.SCREEN_INFORMATION, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.room.not.created.right"), null, "");
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Update
		 */
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				MenuEventController.Instance.DispatchMenuEvent(MenuEventController.EVENT_SYSTEM_ANDROID_BACK_BUTTON);
			}
		}
	}
}