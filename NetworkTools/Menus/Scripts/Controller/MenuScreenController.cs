using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YourCommonTools;

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

			Screen.orientation = ScreenOrientation.Portrait;

			LanguageController.Instance.Initialize();
			SoundsController.Instance.Initialize();

			UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);

#if UNITY_EDITOR
			CreateNewScreen(ScreenMenuMainView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true);
			// CreateNewScreen(ScreenSplashView.SCREEN_NAME, ScreenTypePreviousActionEnum.DESTROY_ALL_SCREENS, true);
#else
		CreateNewScreen(ScreenSplashView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true);        
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
			base.OnUIEvent(_nameEvent, _list);

			if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_CONNECTED_ROOM)
			{
				NetworkEventController.Instance.MenuController_SaveNumberOfPlayers((int)_list[0]);
				CreateNewScreen(ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
				NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
			}
			if (_nameEvent == EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE)
			{
				CreateNewScreen(ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
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
					CreateNewScreen(ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
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
						CreateNewScreen(ScreenGameOptions, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
					}
					else
					{
						CreateNewScreen(ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
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

			NetworkEventController.Instance.MenuController_SaveNumberOfPlayers(m_numberOfPlayers);
			if (ScreenGameOptions.Length > 0)
			{
				CreateNewScreen(ScreenGameOptions, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
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
			if (NetworkEventController.Instance.MenuController_LoadNumberOfPlayers() == MultiplayerConfiguration.VALUE_FOR_JOINING)
			{
				if (_checkScreenGameOptions && (MenuScreenController.Instance.ScreenGameOptions.Length > 0))
				{
					MenuScreenController.Instance.CreateNewScreen(MenuScreenController.Instance.ScreenGameOptions, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
				}
				else
				{
					MenuScreenController.Instance.CreateNewScreen(ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
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
					CreateNewScreen(ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
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
				CreateNewScreen(ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
				NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
#else
				NetworkEventController.Instance.MenuController_JoinRoomOfLobby(MultiplayerConfiguration.LoadRoomNumberInServer(-1), "null", "extraData");
#endif
			}
			else
			{
				// JOIN ROOM IN FACEBOOK
#if ENABLE_BALANCE_LOADER
				CreateNewScreen(ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
				NetworkEventController.Instance.MenuController_LoadGameScene(TargetGameScene);
#else
				NetworkEventController.Instance.MenuController_JoinRoomForFriends(MultiplayerConfiguration.LoadRoomNumberInServer(-1), "null", "extraData");
#endif
			}
		}
	}
}