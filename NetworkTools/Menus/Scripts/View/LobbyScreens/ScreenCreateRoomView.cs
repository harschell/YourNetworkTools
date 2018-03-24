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
	 * ScreenCreateRoomView
	 * 
	 * Screen where we can create a room
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenCreateRoomView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_CREATE_ROOM";

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

			m_container.Find("Description").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.lobby.create.with.description.room");
			m_container.Find("RoomName").GetComponent<InputField>().text = "";

			GameObject createGame = m_container.Find("Button_CreateRoom").gameObject;
			createGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.lobby.create.with.name.room");
			createGame.GetComponent<Button>().onClick.AddListener(CreateRoom);

			m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(BackPressed);

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
		 * CreateRoom
		 */
		private void CreateRoom()
		{
			SoundsController.Instance.PlayFxSelection();
			string roomName = m_container.Find("RoomName").GetComponent<InputField>().text;
			if (roomName.Length < 5)
			{
				MenuScreenController.Instance.CreateNewInformationScreen(ScreenMenuInformationView.SCREEN_INFORMATION, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.lobby.no.name.in.create.room"), null, "");
			}
			else
			{
				MenuEventController.Instance.MenuController_SetNameRoomLobby(roomName);
				if (MenuScreenController.Instance.ForceFixedPlayers != -1)
				{
					MenuScreenController.Instance.LoadCustomGameScreenOrCreateGame(false, MenuScreenController.Instance.ForceFixedPlayers, "", null);
				}
				else
				{
					MenuScreenController.Instance.CreateNewScreen(ScreenMenuNumberPlayersView.SCREEN_NAME, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, null);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Exit button pressed
		 */
		private void BackPressed()
		{
			SoundsController.Instance.PlayFxSelection();
			MenuScreenController.Instance.CreateNewScreen(ScreenMainLobbyView.SCREEN_NAME, ScreenTypePreviousActionEnum.DESTROY_ALL_SCREENS, false, null);
		}

		// -------------------------------------------
		/* 
		* OnMenuBasicEvent
		*/
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			base.OnMenuEvent(_nameEvent, _list);

			if (_nameEvent == MenuEventController.EVENT_SYSTEM_ANDROID_BACK_BUTTON)
			{
				BackPressed();
			}
		}
	}
}
