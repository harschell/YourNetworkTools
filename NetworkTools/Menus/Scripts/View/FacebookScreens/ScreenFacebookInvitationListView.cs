﻿using System;
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
	 * ScreenFacebookInvitationListView
	 * 
	 * Display the list of received invitations
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenFacebookInvitationListView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_INVITATIONS";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		public GameObject RoomFriendItemPrefab;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;
		private GameObject m_grid;

		private Button m_acceptInvitations;
		private Button m_buttonBack;
		private List<ItemRoomView> m_rooms = new List<ItemRoomView>();

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.facebook.select.invitation.to.accept");

			m_acceptInvitations = m_container.Find("Button_Accept").GetComponent<Button>();
			m_container.Find("Button_Accept/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.facebook.accept.invitation");
			m_acceptInvitations.onClick.AddListener(OnAcceptInvitationsPressed);

			m_buttonBack = m_container.Find("Button_Back").GetComponent<Button>();
			m_buttonBack.onClick.AddListener(BackPressed);

			m_grid = m_container.Find("ScrollList/Grid").gameObject;

			// JOIN ROOM IN FACEBOOK
#if ENABLE_BALANCE_LOADER
			MenuEventController.Instance.DelayMenuEvent(MenuEventController.EVENT_MENUEVENTCONTROLLER_SHOW_LOADING_MESSAGE, 0.1f);
			HTTPController.Instance.GetListRooms(false, FacebookController.Instance.Id);
#else
			LoadInvitations(ClientTCPEventsController.Instance.RoomsInvited);
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
		 * LoadInvitations
		 */
		private void LoadInvitations(List<ItemMultiTextEntry> _rooms)
		{
			for (int i = 0; i < _rooms.Count; i++)
			{
				ItemMultiTextEntry friends = _rooms[i];
				GameObject instance = UtilitiesNetwork.AddChild(m_grid.transform, RoomFriendItemPrefab);
				// JOIN ROOM IN FACEBOOK
#if ENABLE_BALANCE_LOADER
				instance.GetComponent<ItemRoomView>().Initialization(int.Parse(friends.Items[0]), friends.Items[1], friends.Items[2], int.Parse(friends.Items[3]));
#else
				instance.GetComponent<ItemRoomView>().Initialization(int.Parse(friends.Items[1]), friends.Items[2], MultiplayerConfiguration.SOCKET_SERVER_ADDRESS, MultiplayerConfiguration.PORT_SERVER_ADDRESS);
#endif

				m_rooms.Add(instance.GetComponent<ItemRoomView>());
			}
		}

		// -------------------------------------------
		/* 
		 * BackPressed
		 */
		private void BackPressed()
		{
			SoundsController.Instance.PlayFxSelection();
			MenuScreenController.Instance.CreateNewScreen(ScreenFacebookMainView.SCREEN_NAME, ScreenTypePreviousActionEnum.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/* 
		 * OnSendInvitationsPressed
		 */
		private void OnAcceptInvitationsPressed()
		{
			ItemRoomView roomSelected = null;
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

				// JOIN ROOM IN FACEBOOK
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

			if (_nameEvent == ItemRoomView.EVENT_ITEM_ROOM_SELECTED)
			{
				ItemRoomView itemRoomView = (ItemRoomView)_list[0];
				for (int i = 0; i < m_rooms.Count; i++)
				{
					if (m_rooms[i] == itemRoomView)
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
				LoadInvitations(ClientTCPEventsController.Instance.RoomsInvited);
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
					LoadInvitations((List<ItemMultiTextEntry>)_list[0]);
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