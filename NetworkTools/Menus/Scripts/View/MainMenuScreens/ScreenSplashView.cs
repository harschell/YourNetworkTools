using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * ScreenSplashView
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenSplashView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_SPLASH";

        private Transform m_container;

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

            bool isThereButtons = false;

            m_container = this.gameObject.transform.Find("Content");

            if (m_container.Find("Text") != null)
            {
                m_container.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.splash.presentation.text");
            }

            if (m_container.Find("Button_Play") != null)
            {
                isThereButtons = true;
                m_container.Find("Button_Play/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.splash.play.game");
                m_container.Find("Button_Play").GetComponent<Button>().onClick.AddListener(PlayGame);
            }

            if (m_container.Find("Button_Store") != null)
            {
                isThereButtons = true;
                m_container.Find("Button_Store/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.splash.go.to.store");
                m_container.Find("Button_Store").GetComponent<Button>().onClick.AddListener(GoToStore);
            }

            if (!isThereButtons)
            {
                StartCoroutine(ShowSplashDelay());
            }            
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
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenMenuMainView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false);
            return false;
        }

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        IEnumerator ShowSplashDelay()
        {
            yield return new WaitForSeconds(5);
            Destroy();
        }

        // -------------------------------------------
        /* 
		 * PlayGame
		 */
        private void PlayGame()
        {
            Destroy();
        }

        // -------------------------------------------
        /* 
		 * GoToStore
		 */
        private void GoToStore()
        {
            Application.OpenURL("https://assetstore.unity.com/publishers/28662");
        }

		// -------------------------------------------
		/* 
		 * GetGameObject
		 */
		public GameObject GetGameObject()
		{
			return this.gameObject;
		}
	}
}