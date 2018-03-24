using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * ScreenListRoomsView
	 * 
	 * Display the list of rooms available to join
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenListRoomsView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_LIST_ROOMS";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		public GameObject RoomLobbyItemPrefab;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;
		private GameObject m_grid;

		private Button m_joinRoom;
		private Button m_buttonBack;
		private List<ItemLobbyRoomView> m_rooms = new List<ItemLobbyRoomView>();

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.game.title");

			m_joinRoom = m_container.Find("Button_Join").GetComponent<Button>();
			m_container.Find("Button_Join/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.lobby.join.the.selected.room");
			m_joinRoom.onClick.AddListener(OnJoinRoom);

			m_buttonBack = m_container.Find("Button_Back").GetComponent<Button>();
			m_buttonBack.onClick.AddListener(BackPressed);

			m_grid = m_container.Find("ScrollList/Grid").gameObject;

			// JOIN ROOM IN LOBBY
#if ENABLE_BALANCE_LOADER
			MenuEventController.Instance.DelayMenuEvent(MenuEventController.EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE, 0.1f);
			HTTPController.Instance.GetListRooms(true, "PLAYER_LOBBY");
#else
			LoadRooms(ClientTCPEventsController.Instance.RoomsLobby);
#endif

			MenuEventController.Instance.MenuEvent += new MenuEventHandler(OnMenuEvent);
		}

		// -------------------------------------------
		/* 
		 * GetGameObject
		 */
		public GameObject GetGameObject()
		{
			return this.gameObject;
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			MenuEventController.Instance.MenuEvent -= OnMenuEvent;
			GameObject.DestroyObject(this.gameObject);
		}

		// -------------------------------------------
		/* 
		 * LoadRooms
		 */
		private void LoadRooms(List<ItemMultiTextEntry> _rooms)
		{
			for (int i = 0; i < _rooms.Count; i++)
			{
				ItemMultiTextEntry room = _rooms[i];
				GameObject instance = UtilitiesNetwork.AddChild(m_grid.transform, RoomLobbyItemPrefab);
				// JOIN ROOM IN LOBBY
#if ENABLE_BALANCE_LOADER
				instance.GetComponent<ItemLobbyRoomView>().Initialization(int.Parse(room.Items[0]), room.Items[1], room.Items[2], int.Parse(room.Items[3]));
#else
				instance.GetComponent<ItemLobbyRoomView>().Initialization(int.Parse(room.Items[1]), room.Items[2], MultiplayerConfiguration.SOCKET_SERVER_ADDRESS, MultiplayerConfiguration.PORT_SERVER_ADDRESS);
#endif
				m_rooms.Add(instance.GetComponent<ItemLobbyRoomView>());
			}
		}

		// -------------------------------------------
		/* 
		 * BackPressed
		 */
		private void BackPressed()
		{
			SoundsController.Instance.PlayFxSelection();
			MenuScreenController.Instance.CreateNewScreen(ScreenMainLobbyView.SCREEN_NAME, ScreenTypePreviousActionEnum.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/* 
		 * OnJoinRoom
		 */
		private void OnJoinRoom()
		{
			ItemLobbyRoomView roomSelected = null;
			for (int i = 0; i < m_rooms.Count; i++)
			{
				if (m_rooms[i].Selected)
				{
					roomSelected = m_rooms[i];
				}
			}

			if (roomSelected != null)
			{
				MenuEventController.Instance.MenuController_SaveRoomNumberInServer(roomSelected.Room);
				// JOIN ROOM IN LOBBY
#if ENABLE_BALANCE_LOADER
				MenuEventController.Instance.MenuController_SaveIPAddressServer(roomSelected.IPAddress);
				MenuEventController.Instance.MenuController_SavePortServer(roomSelected.Port);
#endif
				JoinGamePressed();
			}
			else
			{
				MenuScreenController.Instance.CreateNewInformationScreen(ScreenMenuInformationView.SCREEN_INFORMATION, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("message.you.should.select.an.item"), null, "");
			}
		}

		// -------------------------------------------
		/* 
		 * JoinGamePressed
		 */
		private void JoinGamePressed()
		{
			SoundsController.Instance.PlayFxSelection();
			MenuEventController.Instance.MenuController_SaveNumberOfPlayers(MultiplayerConfiguration.VALUE_FOR_JOINING);
			MenuScreenController.Instance.CreateOrJoinRoomInServer(true);
		}

		// -------------------------------------------
		/*
		* OnMenuBasicEvent
		*/
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			base.OnMenuEvent(_nameEvent, _list);

			if (_nameEvent == ItemLobbyRoomView.EVENT_ITEM_ROOM_LOBBY_SELECTED)
			{
				ItemLobbyRoomView itemLobbyRoomView = (ItemLobbyRoomView)_list[0];
				for (int i = 0; i < m_rooms.Count; i++)
				{
					if (m_rooms[i] == itemLobbyRoomView)
					{
						m_rooms[i].Selected = true;
					}
					else
					{
						m_rooms[i].Selected = false;
					}
				}
			}
			if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_LIST_OF_GAME_ROOMS)
			{
				LoadRooms(ClientTCPEventsController.Instance.RoomsLobby);
			}
			if (_nameEvent == MenuEventController.EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE)
			{
				MenuScreenController.Instance.CreateNewScreen(ScreenMenuLoadingView.SCREEN_NAME, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, null);
			}
			if (_nameEvent == GetListRoomsHTTP.EVENT_CLIENT_HTTP_LIST_OF_GAME_ROOMS)
			{
				MenuEventController.Instance.DispatchMenuEvent(MenuScreenController.EVENT_MENU_FORCE_DESTRUCTION_POPUP);
				if (_list.Length == 1)
				{
					LoadRooms((List<ItemMultiTextEntry>)_list[0]);
				}
				else
				{
					MenuEventController.Instance.DispatchMenuEvent(MenuScreenController.EVENT_MENU_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
					MenuScreenController.Instance.CreateNewInformationScreen(ScreenMenuInformationView.SCREEN_INFORMATION, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.room.list.not.retrieved"), null, "");
				}
			}
		}
	}
}