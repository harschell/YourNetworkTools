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
	 * ScreenMenuMainView
	 * 
	 * Main Menu Screen with the option to play a local or a remote game
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenMenuMainView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_MAIN";

		// ----------------------------------------------
		// SUBEVENTS
		// ----------------------------------------------	
		public const string SUB_EVENT_SCREENMAIN_CONFIRMATION_EXIT_APP = "SUB_EVENT_SCREENMAIN_CONFIRMATION_EXIT_APP";

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

			SoundsController.Instance.StopAllSounds();

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.game.title");

			GameObject localPartyGame = m_container.Find("Button_LocalParty").gameObject;
			localPartyGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.main.menu.local.party");

			localPartyGame.GetComponent<Button>().onClick.AddListener(OnLocalPartyGame);

			GameObject remotePartyGame = m_container.Find("Button_RemoteParty").gameObject;
			remotePartyGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.main.menu.remote.party");
			remotePartyGame.GetComponent<Button>().onClick.AddListener(OnRemotePartyGame);

			GameObject instructionsGame = m_container.Find("Button_Instructions").gameObject;
			instructionsGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.main.menu.instructions.game");
			instructionsGame.GetComponent<Button>().onClick.AddListener(InstructionsGamePressed);

			m_container.Find("Button_Exit").GetComponent<Button>().onClick.AddListener(ExitPressed);

			SoundsController.Instance.PlayLoopSound(SoundsConfiguration.SOUND_MAIN_MENU);

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
			GameObject.Destroy(this.gameObject);
			return false;
		}

		// -------------------------------------------
		/* 
		 * CreateGamePressed
		 */
		private void OnLocalPartyGame()
		{
			NetworkEventController.Instance.MenuController_SetLocalGame(true);
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			MenuScreenController.Instance.CreateNewScreen(ScreenMenuLocalGameView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/* 
		 * JoinGamePressed
		 */
		private void OnRemotePartyGame()
		{
			NetworkEventController.Instance.MenuController_SetLocalGame(false);
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			MenuScreenController.Instance.CreateNewScreen(ScreenRemoteModeView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/* 
		 * InstructionsGamePressed
		 */
		private void InstructionsGamePressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			List<PageInformation> pages = new List<PageInformation>();
			pages.Add(new PageInformation(LanguageController.Instance.GetText("screen.instructions.title"), LanguageController.Instance.GetText("screen.instructions.page.1"), MenuScreenController.Instance.Instructions[0], ""));
			pages.Add(new PageInformation(LanguageController.Instance.GetText("screen.instructions.title"), LanguageController.Instance.GetText("screen.instructions.page.2"), MenuScreenController.Instance.Instructions[1], ""));
			pages.Add(new PageInformation(LanguageController.Instance.GetText("screen.instructions.title"), LanguageController.Instance.GetText("screen.instructions.page.3"), MenuScreenController.Instance.Instructions[2], ""));
			pages.Add(new PageInformation(LanguageController.Instance.GetText("screen.instructions.title"), LanguageController.Instance.GetText("screen.instructions.page.4"), MenuScreenController.Instance.Instructions[3], ""));
			MenuScreenController.Instance.CreateNewScreen(ScreenInformationView.SCREEN_INFORMATION_IMAGE, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, pages);
		}


		// -------------------------------------------
		/* 
		 * Exit button pressed
		 */
		private void ExitPressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			MenuScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_CONFIRMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("message.do.you.want.exit"), null, SUB_EVENT_SCREENMAIN_CONFIRMATION_EXIT_APP);
		}

		// -------------------------------------------
		/* 
		* OnMenuBasicEvent
		*/
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				ExitPressed();
			}
			if (_nameEvent == MenuScreenController.EVENT_CONFIRMATION_POPUP)
			{
				string subEvent = (string)_list[2];
				if (subEvent == SUB_EVENT_SCREENMAIN_CONFIRMATION_EXIT_APP)
				{
					if ((bool)_list[1])
					{
						Application.Quit();
					}
				}
			}
			if (_nameEvent == ClientTCPEventsController.EVENT_CLIENT_TCP_CONNECTED_ROOM)
			{
				NetworkEventController.Instance.MenuController_LoadGameScene(MenuScreenController.Instance.TargetGameScene);
			}
		}
	}
}