using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YourCommonTools;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * ScreenFacebookMainView
	 * 
	 * Screen where we can invite a friend to a game or receive an invitation.
	 * 
	 * To do that we will connect to the TCP socket server with our Facebook ID.
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenFacebookMainView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_FACEBOOK_MAIN";

		// ----------------------------------------------
		// PUBLIC CONSTANTS
		// ----------------------------------------------	

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		private int m_connectedFacebook = -1;
		private GameObject m_createInvitation;
		private GameObject m_acceptInvitation;
		private Text m_textDescription;
		private Button m_buttonBack;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

			NetworkEventController.Instance.MenuController_SetNameRoomLobby("");

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.game.title");

			m_createInvitation = m_container.Find("Button_CreateInvitation").gameObject;
			m_createInvitation.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.facebook.create.invitation");
			m_createInvitation.GetComponent<Button>().onClick.AddListener(OnCreateInvitationPressed);

			m_acceptInvitation = m_container.Find("Button_AcceptInvitation").gameObject;
			m_acceptInvitation.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.facebook.invitations.list");
			m_acceptInvitation.transform.Find("Number").GetComponent<Text>().text = ClientTCPEventsController.Instance.RoomsInvited.Count.ToString();
			m_acceptInvitation.GetComponent<Button>().onClick.AddListener(OnAcceptInvitationPressed);

			m_buttonBack = m_container.Find("Button_Back").GetComponent<Button>();
			m_buttonBack.onClick.AddListener(BackPressed);

			UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);			
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
		public override bool Destroy()
		{
			if (base.Destroy()) return true;
			UIEventController.Instance.UIEvent -= OnMenuEvent;
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
			return false;
		}

		// -------------------------------------------
		/* 
		 * OnCreateInvitationPressed
		 */
		private void OnCreateInvitationPressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			MenuScreenController.Instance.CreateNewScreen(ScreenFacebookCreateInvitationView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/* 
		 * OnAcceptInvitationPressed
		 */
		private void OnAcceptInvitationPressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			MenuScreenController.Instance.CreateNewScreen(ScreenFacebookInvitationListView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/* 
		 * BackPressed
		 */
		private void BackPressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			ClientTCPEventsController.Instance.Destroy();
			MenuScreenController.Instance.CreateNewScreen(ScreenMenuMainView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/*
		* OnMenuBasicEvent
		*/
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			base.OnMenuEvent(_nameEvent, _list);

			if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				BackPressed();
			}
			if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_LIST_OF_GAME_ROOMS)
			{
				m_acceptInvitation.transform.Find("Number").GetComponent<Text>().text = ClientTCPEventsController.Instance.RoomsInvited.Count.ToString();
			}
		}
	}
}