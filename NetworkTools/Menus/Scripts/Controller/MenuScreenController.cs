using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YourCommonTools;
#if ENABLE_YOURVRUI
using YourVRUI;
#endif

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * MenuScreenController
	 * 
	 * ScreenManager controller that handles all the screens's creation and disposal
	 * 
	 * @author Esteban Gallardo
	 */
	public class MenuScreenController : ScreenController
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE = "EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE";

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

        [Tooltip("The IP address of the server")]
        public string ServerIPAdress = "";

        [Tooltip("The IP address of the server")]
        public int ServerPortNumber = -1;

        [Tooltip("Image used by ARCore to set the anchor")]
        public Sprite ScanImageARCore;

        [Tooltip("Main camera used to display the menus in 2D")]
        public GameObject MainCamera2D;

        [Tooltip("Components to display the menus in VR")]
        public GameObject VRComponents;

        [Tooltip("Allow the option to enable AR or VR gaming")]
        public bool AskToEnableBackgroundARCore = false;

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------
        public Sprite IconApp;
		public Sprite LogoApp;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private bool m_isFriendsRoom = false;
		private int m_numberOfPlayers = -1;
		private string m_friends;
		private List<string> m_friendsIDs;
		private string m_extraData = "extraData";

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public string ExtraData
		{
			get { return m_extraData; }
			set { m_extraData = value; }
		}

		// -------------------------------------------
		/* 
		 * Initialitzation listener
		 */
		public override void Start()
		{
			base.Start();

			if (DebugMode)
			{
				Debug.Log("YourVRUIScreenController::Start::First class to initialize for the whole system to work");
			}

#if !ENABLE_OCULUS
            Screen.orientation = ScreenOrientation.Portrait;
#endif

			LanguageController.Instance.Initialize();
			SoundsController.Instance.Initialize();

            if (ServerIPAdress.Length > 0) MultiplayerConfiguration.SaveIPAddressServer(ServerIPAdress);
            if (ServerPortNumber != -1) MultiplayerConfiguration.SavePortServer(ServerPortNumber);

            UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);

#if ENABLE_WORLDSENSE || ENABLE_OCULUS
            KeysEventInputController.Instance.EnableActionOnMouseDown = false;

            MenuScreenController.Instance.MainCamera2D.SetActive(false);
            MenuScreenController.Instance.VRComponents.SetActive(true);
            Invoke("StartSplashScreen", 0.2f);
#else
            StartSplashScreen();
#endif
        }

        // -------------------------------------------
        /* 
		 * StartSplashScreen
		 */
        public void StartSplashScreen()
        {
#if UNITY_EDITOR
            // UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenMenuMainView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true);
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenSplashView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true);
#else
		UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenSplashView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true);        
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
		public override void Destroy()
		{
			base.Destroy();

			UIEventController.Instance.UIEvent -= OnUIEvent;
		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		protected override void OnUIEvent(string _nameEvent, params object[] _list)
		{
#if ENABLE_YOURVRUI
            ProcessConnectionEvents(_nameEvent, _list);

            ProcessVRUIScreens(_nameEvent, _list);
#else
            base.OnUIEvent(_nameEvent, _list);

            ProcessConnectionEvents(_nameEvent, _list);
#endif

            if (_nameEvent == EVENT_APP_LOST_FOCUS)
            {
#if ENABLE_WORLDSENSE || ENABLE_OCULUS
                if ((bool)_list[0])
                {
                    Application.Quit();
                }
#endif
            }
        }

        // -------------------------------------------
        /* 
		 * Process connection events
		 */
        protected void ProcessConnectionEvents(string _nameEvent, params object[] _list)
        {
            if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_CONNECTED_ROOM)
            {
                NetworkEventController.Instance.MenuController_SaveNumberOfPlayers((int)_list[0]);
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
                NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
            }
            if (_nameEvent == EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE)
            {
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
            }
            if (_nameEvent == CreateNewRoomHTTP.EVENT_CLIENT_HTTP_NEW_ROOM_CREATED)
            {
                // CREATE ROOM IN LOBBY
                UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_POPUP);
                if (_list.Length == 4)
                {
                    NetworkEventController.Instance.MenuController_SaveRoomNumberInServer((int)_list[0]);
                    NetworkEventController.Instance.MenuController_SaveIPAddressServer((string)_list[1]);
                    NetworkEventController.Instance.MenuController_SavePortServer((int)_list[2]);
                    NetworkEventController.Instance.MenuController_SaveMachineIDServer((int)_list[3]);
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
                    NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
                }
                else
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
                    CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.room.not.created.right"), null, "");
                }
            }
        }

        // -------------------------------------------
        /* 
		 * Process optional VR menus
		 */
        protected void ProcessVRUIScreens(string _nameEvent, params object[] _list)
        {
#if ENABLE_YOURVRUI
            if (YourVRUIScreenController.Instance == null)
            {
                ProcessScreenEvents(_nameEvent, _list);
            }
            else
            {
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN)
                {
                    if (_list.Length > 2)
                    {
                        if ((bool)_list[2])
                        {
                            YourVRUIScreenController.Instance.DestroyScreens();
                        }
                        else
                        {
                            YourVRUIScreenController.Instance.EnableScreens = true;
                        }
                    }
                    List<PageInformation> pages = null;
                    if (_list.Length > 3)
                    {
                        pages = (List<PageInformation>)_list[3];
                    }
                    YourVRUIScreenController.Instance.CreateScreenLinkedToCamera(GetScreenPrefabByName((string)_list[0]), pages, 1.5f, -1, false, -1, (UIScreenTypePreviousAction)_list[1]);
                    if ((string)_list[0] == ScreenCreateRoomView.SCREEN_NAME)
                    {
                        UIEventController.Instance.DispatchUIEvent(ScreenCreateRoomView.EVENT_SCREENCREATEROOM_CREATE_RANDOM_NAME);
                    }
                }
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN)
                {
                    string nameScreen = (string)_list[0];
                    UIScreenTypePreviousAction previousAction = (UIScreenTypePreviousAction)_list[1];
                    string title = (string)_list[2];
                    string description = (string)_list[3];
                    Sprite image = (Sprite)_list[4];
                    string eventData = (string)_list[5];
                    List<PageInformation> pages = new List<PageInformation>();
                    pages.Add(new PageInformation(title, description, image, eventData, "", ""));
                    YourVRUIScreenController.Instance.CreateScreenLinkedToCamera(GetScreenPrefabByName((string)_list[0]), pages, 1.5f, -1, false, -1, previousAction);
                }
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_LOAD_NEW_SCENE)
                {
                    if (YourVRUIScreenController.Instance != null)
                    {
                        YourVRUIScreenController.Instance.Destroy();
                    }
                }
            }
#endif
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
				NetworkEventController.Instance.MenuController_SaveNumberOfPlayers(finalNumberOfPlayers);
				if (NetworkEventController.Instance.IsLobbyMode)
				{
					if (NetworkEventController.Instance.NameRoomLobby.Length > 0)
					{
						NetworkEventController.Instance.MenuController_CreateNewLobbyRoom(NetworkEventController.Instance.NameRoomLobby, finalNumberOfPlayers, _extraData);
					}
					else
					{
						UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
						CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.there.is.no.name.for.room"), null, "");
					}
				}
				else
				{
                    if (ScreenGameOptions.Length > 0)
					{
						UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenGameOptions, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
					}
					else
					{
						UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
						NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
					}
				}
			}
			else
			{
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
				CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.player.number.not.right.number"), null, "");
			}
		}

		// -------------------------------------------
		/* 
		 * Will create the game or it will load a custom game screen
		 */
		public void LoadCustomGameScreenOrCreateGame(bool _isFriendsRoom, int _numberOfPlayers, string _friends, List<string> _friendsIDs, bool _loadNextScreen = true)
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

			NetworkEventController.Instance.MenuController_SaveNumberOfPlayers(m_numberOfPlayers);
            if (_loadNextScreen)
            {
                if (ScreenGameOptions.Length > 0)
                {
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenGameOptions, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
                }
                else
                {
                    CreateOrJoinRoomInServer(false);
                }
            }
        }

		// -------------------------------------------
		/* 
		 * Create for real the room in server
		 */
		public void CreateOrJoinRoomInServer(bool _checkScreenGameOptions)
		{
			if (NetworkEventController.Instance.MenuController_LoadNumberOfPlayers() == MultiplayerConfiguration.VALUE_FOR_JOINING)
			{
				if (_checkScreenGameOptions && (MenuScreenController.Instance.ScreenGameOptions.Length > 0))
				{
					UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,MenuScreenController.Instance.ScreenGameOptions, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
				}
				else
				{
					UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
					MultiplayerConfiguration.SaveExtraData(m_extraData);
					if (!YourNetworkTools.GetIsLocalGame())
					{
                        JoinARoomInServer();
					}
					else
					{
						NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
					}
				}
			}
			else
			{
				if (!YourNetworkTools.GetIsLocalGame())
				{
					if (m_isFriendsRoom)
					{
						NetworkEventController.Instance.MenuController_CreateNewFacebookRoom(m_friends, m_friendsIDs, m_extraData);
					}
					else
					{
						CreateRoomInServer(m_numberOfPlayers, m_extraData);
					}
				}
				else
				{
					MultiplayerConfiguration.SaveExtraData(m_extraData);
					UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
					NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
				}
			}
		}


		// -------------------------------------------
		/* 
		* Client has selected a room to join
		*/
		public void JoinARoomInServer()
		{
			if (NetworkEventController.Instance.IsLobbyMode)
			{
				// JOIN ROOM IN LOBBY
#if ENABLE_BALANCE_LOADER
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
				NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
#else
				NetworkEventController.Instance.MenuController_JoinRoomOfLobby(MultiplayerConfiguration.LoadRoomNumberInServer(-1), "null", "extraData");
#endif
			}
			else
			{
				// JOIN ROOM IN FACEBOOK
#if ENABLE_BALANCE_LOADER
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN,ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
				NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
#else
				NetworkEventController.Instance.MenuController_JoinRoomForFriends(MultiplayerConfiguration.LoadRoomNumberInServer(-1), "null", "extraData");
#endif
			}
		}
	}
}