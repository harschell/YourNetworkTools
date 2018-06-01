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
	 * ScreenRemoteModeView
	 * 
	 * Screen where we select between playing the game with friends or going to the lobby
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenRemoteModeView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_REMOTE_MODE";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

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

			GameObject socialFriendsMode = m_container.Find("Button_Friends").gameObject;
			socialFriendsMode.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.remote.mode.friends.mode");
			socialFriendsMode.GetComponent<Button>().onClick.AddListener(PlayWithFriends);

			GameObject lobbyMode = m_container.Find("Button_Lobby").gameObject;
			lobbyMode.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.remote.mode.lobby.mode");
			lobbyMode.GetComponent<Button>().onClick.AddListener(GoToLobby);

#if !ENABLE_FACEBOOK
			socialFriendsMode.gameObject.SetActive(false);
#endif

			m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(BackPressed);

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
		 * PlayWithFriends
		 */
		private void PlayWithFriends()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			MenuScreenController.Instance.CreateNewScreen(ScreenFacebookConnectView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/* 
		 * GoToLobby
		 */
		private void GoToLobby()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			NetworkEventController.Instance.MenuController_SetLobbyMode(true);
			// NO CONNECT TCP, GO TO LOBBY
#if ENABLE_BALANCE_LOADER
			MenuScreenController.Instance.CreateNewScreen(ScreenMainLobbyView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
#else
			NetworkEventController.Instance.MenuController_InitialitzationSocket(-1, 0);
#endif
		}

		// -------------------------------------------
		/* 
		 * Exit button pressed
		 */
		private void BackPressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			MenuScreenController.Instance.CreateNewScreen(ScreenMenuMainView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/* 
		* OnMenuBasicEvent
		*/
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			base.OnMenuEvent(_nameEvent, _list);

			if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID)
			{
				MenuScreenController.Instance.CreateNewScreen(ScreenMainLobbyView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
			}
			if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				BackPressed();
			}
		}
	}
}
