using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YourCommonTools;
using YourNetworkingTools;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * ScreenEnableVR
	 * 
	 * Enable VR or gyroscope mode
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenEnableVR : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_ENABLE_VR";

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

			GameObject playInVRGame = m_container.Find("Button_EnableVR").gameObject;
			playInVRGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.play.as.vr.game");
			playInVRGame.GetComponent<Button>().onClick.AddListener(PlayInVRPressed);

			GameObject playWithGyroscopeGame = m_container.Find("Button_EnableGyroscope").gameObject;
			playWithGyroscopeGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.play.with.gyroscope");
			playWithGyroscopeGame.GetComponent<Button>().onClick.AddListener(PlayWithGyroscopePressed);

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
		* PlayInVRPressed
		*/
		private void PlayInVRPressed()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
            CardboardLoaderVR.SaveEnableCardboard(true);
#if ENABLE_GOOGLE_ARCORE
            if (!MenuScreenController.Instance.AskToEnableBackgroundARCore || (MultiplayerConfiguration.LoadGoogleARCore(-1) != MultiplayerConfiguration.GOOGLE_ARCORE_ENABLED))
            {
                MenuScreenController.Instance.CreateOrJoinRoomInServer(false);
                Destroy();
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
            }
            else
            {
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenEnableBackground.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
            }
#else
            MenuScreenController.Instance.CreateOrJoinRoomInServer(false);
            Destroy();
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
#endif
        }

        // -------------------------------------------
        /* 
		* JoinGamePressed
		*/
        private void PlayWithGyroscopePressed()
		{
            SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
            CardboardLoaderVR.SaveEnableCardboard(false);
#if ENABLE_GOOGLE_ARCORE
            if (!MenuScreenController.Instance.AskToEnableBackgroundARCore || (MultiplayerConfiguration.LoadGoogleARCore(-1) != MultiplayerConfiguration.GOOGLE_ARCORE_ENABLED))
            {
                MenuScreenController.Instance.CreateOrJoinRoomInServer(false);
                Destroy();
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
            }
            else
            {
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenEnableBackground.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
            }
#else
            MenuScreenController.Instance.CreateOrJoinRoomInServer(false);
            Destroy();
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, null);
#endif
        }

        // -------------------------------------------
        /* 
		* OnMenuBasicEvent
		*/
        protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			base.OnMenuEvent(_nameEvent, _list);
		}
	}
}