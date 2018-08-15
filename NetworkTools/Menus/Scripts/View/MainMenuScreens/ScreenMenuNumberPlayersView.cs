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
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		private int m_finalNumberOfPlayers;

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

			m_container.Find("PlayerValue").GetComponent<InputField>().text = "2";
#if ENABLE_OCULUS
            m_container.Find("PlayerValue").GetComponent<InputField>().text = "1";
#endif
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