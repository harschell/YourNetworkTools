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
	 * ScreenFacebookCreateInvitationView
	 * 
	 * Display the selectable list of friends and send the invitations
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenFacebookCreateInvitationView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_FACEBOOK_FRIENDS";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		public GameObject FacebookFriendItemPrefab;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;
		private GameObject m_grid;

		private Button m_sendInvitations;
		private Button m_buttonBack;
		private List<ItemFriendView> m_friends = new List<ItemFriendView>();

		private int m_currentSelectedPlayers = 0;
		private int m_counterTotalPlayers = 0;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.facebook.invite.friends");

			m_sendInvitations = m_container.Find("Button_Invite").GetComponent<Button>();
			m_container.Find("Button_Invite/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.facebook.send.invitations");
			m_sendInvitations.onClick.AddListener(OnSendInvitationsPressed);

			m_buttonBack = m_container.Find("Button_Back").GetComponent<Button>();
			m_buttonBack.onClick.AddListener(BackPressed);

			m_grid = m_container.Find("ScrollList/Grid").gameObject;

			for (int i = 0; i < FacebookController.Instance.Friends.Count; i++)
			{
				ItemMultiTextEntry sfriend = FacebookController.Instance.Friends[i];
				GameObject instance = UtilitiesNetwork.AddChild(m_grid.transform, FacebookFriendItemPrefab);
				instance.GetComponent<ItemFriendView>().Initialization(sfriend.Items[0], sfriend.Items[1]);
				m_friends.Add(instance.GetComponent<ItemFriendView>());
			}

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
		private void OnSendInvitationsPressed()
		{
			List<string> friendsIDs = new List<string>();
			friendsIDs.Add(FacebookController.Instance.Id);
			m_counterTotalPlayers = 1;
			string friends = "";
			for (int i = 0; i < m_friends.Count; i++)
			{
				if (m_friends[i].Selected)
				{
					m_counterTotalPlayers++;
					friendsIDs.Add(m_friends[i].FacebookID);

					if (friends.Length > 0)
					{
						friends += ",";
					}
					friends += m_friends[i].FacebookID;
				}
			}

			if (friendsIDs.Count > 0)
			{
				friends = FacebookController.Instance.Id + "," + friends;
				MenuScreenController.Instance.LoadCustomGameScreenOrCreateGame(true, friendsIDs.Count, friends, friendsIDs);
			}
			else
			{
				MenuScreenController.Instance.CreateNewInformationScreen(ScreenMenuInformationView.SCREEN_INFORMATION, ScreenTypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.warning"), LanguageController.Instance.GetText("message.you.should.select.an.item"), null, "");
			}
		}


		// -------------------------------------------
		/*
		* OnMenuBasicEvent
		*/
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			base.OnMenuEvent(_nameEvent, _list);

			if (_nameEvent == ItemFriendView.EVENT_ITEM_FRIEND_SELECTED)
			{
				ItemFriendView itemFriendView = (ItemFriendView)_list[0];
				for (int i = 0; i < m_friends.Count; i++)
				{
					if (m_friends[i] == itemFriendView)
					{
						if (!m_friends[i].Selected)
						{
							if (m_currentSelectedPlayers < MenuScreenController.Instance.MaxPlayers - 1)
							{
								m_friends[i].Selected = true;
								m_currentSelectedPlayers++;
							}
						}
						else
						{
							m_friends[i].Selected = false;
							m_currentSelectedPlayers--;
						}
					}
				}
			}
		}
	}
}