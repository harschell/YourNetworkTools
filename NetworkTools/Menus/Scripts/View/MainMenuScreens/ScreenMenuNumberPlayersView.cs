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
	 * ScreenMenuNumberPlayersView
	 * 
	 * Screen where we define the initial number of players that will have the game
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenMenuNumberPlayersView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_PLAYER_NUMBER";

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        public int MaximumNumberOfPlayers = 6;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private GameObject m_root;
		private Transform m_container;

		private int m_finalNumberOfPlayers;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public int FinalNumberOfPlayers
        {
            get
            {
                string numberOfPlayers = m_container.Find("PlayerValue").GetComponent<InputField>().text;

                // NUMBER OF PLAYERS
                m_finalNumberOfPlayers = -1;
                if (!int.TryParse(numberOfPlayers, out m_finalNumberOfPlayers))
                {
                    m_finalNumberOfPlayers = -1;
                }

                return m_finalNumberOfPlayers;
            }
            set
            {
                if ((value > 0) && (value <= MaximumNumberOfPlayers))
                {
                    m_finalNumberOfPlayers = value;
                    m_container.Find("PlayerValue").GetComponent<InputField>().text = m_finalNumberOfPlayers.ToString();
                }
            }
        }

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.player.number.create.new.game");

			m_container.Find("Button_Ok").GetComponent<Button>().onClick.AddListener(ConfirmNumberPlayers);
			m_container.Find("Button_Ok/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.player.number.create.new.game");

			m_container.Find("Description").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.player.number.description");

            if (m_container.Find("Button_Plus") != null)
            {
                m_container.Find("Button_Plus").GetComponent<Button>().onClick.AddListener(IncreasePlayerNumber);
            }

            if (m_container.Find("Button_Minus") != null)
            {
                m_container.Find("Button_Minus").GetComponent<Button>().onClick.AddListener(DecreasePlayerNumber);
            }

            m_container.Find("PlayerValue").GetComponent<InputField>().text = "2";
#if ENABLE_OCULUS || ENABLE_WORLDSENSE
            m_container.Find("PlayerValue").GetComponent<InputField>().text = "1";
#endif
        }

        // -------------------------------------------
        /* 
		 * DecreasePlayerNumber
		 */
        private void DecreasePlayerNumber()
        {
            FinalNumberOfPlayers = FinalNumberOfPlayers - 1;
        }

        // -------------------------------------------
        /* 
		 * IncreasePlayerNumber
		 */
        private void IncreasePlayerNumber()
        {
            FinalNumberOfPlayers = FinalNumberOfPlayers + 1;
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
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
			return false;
		}

		// -------------------------------------------
		/* 
		 * ConfirmNumberPlayers
		 */
		private void ConfirmNumberPlayers()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);

			string numberOfPlayers = m_container.Find("PlayerValue").GetComponent<InputField>().text;

			// NUMBER OF PLAYERS
			m_finalNumberOfPlayers = -1;
			if (!int.TryParse(numberOfPlayers, out m_finalNumberOfPlayers))
			{
				m_finalNumberOfPlayers = -1;
			}
			MenuScreenController.Instance.LoadCustomGameScreenOrCreateGame(false, m_finalNumberOfPlayers, "", null);
		}
	}
}