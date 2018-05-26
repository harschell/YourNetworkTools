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
	 * ScreenFacebookConnectView
	 * 
	 * Screen where we will connect with Facebook
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenFacebookConnectView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_FACEBOOK_CONNECT";

		// ----------------------------------------------
		// PUBLIC CONSTANTS
		// ----------------------------------------------	
		public const string USER_FACEBOOK_CONNECTED_COOCKIE = "USER_FACEBOOK_CONNECTED_COOCKIE";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		private int m_connectedFacebook = -1;
		private GameObject m_connectionFacebookButton;
		private Text m_textDescription;
		private Button m_buttonBack;

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

			m_textDescription = m_container.Find("Description").GetComponent<Text>();
			m_textDescription.text = LanguageController.Instance.GetText("message.facebook.description.connect.for.friends");

			m_connectionFacebookButton = m_container.Find("Button_ConnectFacebook").gameObject;
			m_connectionFacebookButton.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.facebook.connect");
			m_connectionFacebookButton.GetComponent<Button>().onClick.AddListener(OnFacebookButtonPressed);

			m_buttonBack = m_container.Find("Button_Back").GetComponent<Button>();
			m_buttonBack.onClick.AddListener(BackPressed);

			MenuEventController.Instance.MenuEvent += new MenuEventHandler(OnMenuEvent);

			AutoLogin();
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
		 * AutoLogin
		 */
		private void AutoLogin()
		{
			// CHECK COOKIES CONNECTION
			m_connectedFacebook = PlayerPrefs.GetInt(USER_FACEBOOK_CONNECTED_COOCKIE, -1);
			if (m_connectedFacebook != -1)
			{
				OnFacebookButtonPressed();
			}
		}

		// -------------------------------------------
		/* 
		 * OnFacebookButtonPressed
		 */
		private void OnFacebookButtonPressed()
		{
			m_buttonBack.gameObject.SetActive(false);
			m_connectionFacebookButton.SetActive(false);
			m_textDescription.text = LanguageController.Instance.GetText("message.facebook.connecting.wait");
			MenuEventController.Instance.MenuController_SetLobbyMode(false);
			FacebookController.Instance.Initialitzation();
		}

		// -------------------------------------------
		/* 
		 * BackPressed
		 */
		private void BackPressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			MenuScreenController.Instance.CreateNewScreen(ScreenMenuMainView.SCREEN_NAME, ScreenTypePreviousActionEnum.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/*
		* OnMenuBasicEvent
		*/
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			base.OnMenuEvent(_nameEvent, _list);

			if (_nameEvent == FacebookController.EVENT_FACEBOOK_COMPLETE_INITIALITZATION)
			{
				if ((string)_list[0] != null)
				{
					PlayerPrefs.SetInt(USER_FACEBOOK_CONNECTED_COOCKIE, 1);
					// NO CONNECT TCP, GO TO MAIN FACEBOOK
#if ENABLE_BALANCE_LOADER
					MenuScreenController.Instance.CreateNewScreen(ScreenFacebookMainView.SCREEN_NAME, ScreenTypePreviousActionEnum.DESTROY_ALL_SCREENS, false, null);
#else
					m_textDescription.text = LanguageController.Instance.GetText("message.facebook.connecting.to.server");
					MenuEventController.Instance.MenuController_InitialitzationSocket(-1, 0);
#endif

				}
				else
				{
					m_textDescription.text = LanguageController.Instance.GetText("message.facebook.description.connect.for.friends");
					m_connectionFacebookButton.SetActive(true);
					m_buttonBack.gameObject.SetActive(true);
					MenuScreenController.Instance.CreateNewInformationScreen(ScreenMenuInformationView.SCREEN_INFORMATION, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.facebook.connection.error"), null, "");
				}
			}
			if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID)
			{
				MenuScreenController.Instance.CreateNewScreen(ScreenFacebookMainView.SCREEN_NAME, ScreenTypePreviousActionEnum.DESTROY_ALL_SCREENS, false, null);
			}
			if (_nameEvent == MenuEventController.EVENT_SYSTEM_ANDROID_BACK_BUTTON)
			{
				BackPressed();
			}
		}
	}
}