using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Facebook.Unity;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * FacebookController
	 * 
	 * It manages all the Facebook operations, login and payments.
	 * 
	 * @author Esteban Gallardo
	 */
	public class FacebookController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_FACEBOOK_REQUEST_INITIALITZATION = "EVENT_FACEBOOK_REQUEST_INITIALITZATION";
		public const string EVENT_FACEBOOK_MY_INFO_LOADED = "EVENT_FACEBOOK_MY_INFO_LOADED";
		public const string EVENT_FACEBOOK_FRIENDS_LOADED = "EVENT_FACEBOOK_FRIENDS_LOADED";
		public const string EVENT_FACEBOOK_COMPLETE_INITIALITZATION = "EVENT_FACEBOOK_COMPLETE_INITIALITZATION";
		public const string EVENT_REGISTER_IAP_COMPLETED = "EVENT_REGISTER_IAP_COMPLETED";

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static FacebookController _instance;

		public static FacebookController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(FacebookController)) as FacebookController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						container.name = "FacebookController";
						_instance = container.AddComponent(typeof(FacebookController)) as FacebookController;
						DontDestroyOnLoad(_instance);
					}
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// MEMBERS
		// ----------------------------------------------
		private string m_id = null;
		private string m_nameHuman;
		private string m_email;
		private bool m_isInited = false;
		private bool m_invitationAccepted = false;

		private List<ItemMultiTextEntry> m_friends = new List<ItemMultiTextEntry>();

		public string Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public string NameHuman
		{
			get { return m_nameHuman; }
			set { m_nameHuman = value; }
		}
		public string Email
		{
			get { return m_email; }
		}
		public List<ItemMultiTextEntry> Friends
		{
			get { return m_friends; }
		}
		public bool IsInited
		{
			get { return m_isInited; }
		}
		public bool InvitationAccepted
		{
			get { return m_invitationAccepted; }
		}

		// ----------------------------------------------
		// CONSTRUCTOR
		// ----------------------------------------------	
		// -------------------------------------------
		/* 
		 * Constructor
		 */
		private FacebookController()
		{
		}

		// -------------------------------------------
		/* 
		 * InitListener
		 */
		public void InitListener()
		{
			MenuEventController.Instance.MenuEvent += new MenuEventHandler(OnMenuEvent);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			MenuEventController.Instance.MenuEvent -= OnMenuEvent;
			DestroyObject(_instance.gameObject);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		 * Initialitzation
		 */
		public void Initialitzation()
		{
			if (!FB.IsInitialized)
			{
				if (!m_isInited)
				{
					// ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_WAIT, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
					InitListener();
					FB.Init(this.OnInitComplete, this.OnHideUnity);
				}
				else
				{
					InitListener();
					RegisterConnectionFacebookID(true);
				}
			}
			else
			{
				// Already initialized, signal an app activation App Event
				InitListener();
				FB.ActivateApp();
				OnInitComplete();
			}
		}

		// -------------------------------------------
		/* 
		 * OnInitComplete
		 */
		private void OnInitComplete()
		{
			MenuEventController.Instance.DispatchMenuEvent(EVENT_FACEBOOK_REQUEST_INITIALITZATION);
			Debug.Log("Success - Check log for details");
			Debug.Log("Success Response: OnInitComplete Called");
			Debug.Log("OnInitCompleteCalled IsLoggedIn='{" + FB.IsLoggedIn + "}' IsInitialized='{" + FB.IsInitialized + "}'");
			if (AccessToken.CurrentAccessToken != null)
			{
				Debug.Log(AccessToken.CurrentAccessToken.ToString());
			}
			LogInWithPermissions();
		}

		// -------------------------------------------
		/* 
		 * OnHideUnity
		 */
		private void OnHideUnity(bool _isGameShown)
		{
			Debug.Log("Success - Check log for details");
			Debug.Log("Success Response: OnHideUnity Called {" + _isGameShown + "}");
			Debug.Log("Is game shown: " + _isGameShown);
		}

		// -------------------------------------------
		/* 
		 * LogInWithPermissions
		 */
		private void LogInWithPermissions()
		{
			FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" }, LoggedWithPermissions);
		}

		// -------------------------------------------
		/* 
		 * LoggedWithPermissions
		 */
		private void LoggedWithPermissions(IResult _result)
		{
			if (_result == null)
			{
				// if (ScreenController.Instance.DebugMode) Debug.Log("Null Response");
				return;
			}

			// if (ScreenController.Instance.DebugMode) Debug.Log("FacebookController::LoggedWithPermissions::result.RawResult=" + _result.RawResult);
			FB.API("/me?fields=id,name,email", HttpMethod.GET, HandleMyInformation);
		}

		// -------------------------------------------
		/* 
		 * HandleMyInformation
		 */
		private void HandleMyInformation(IResult _result)
		{
			if (_result == null)
			{
				// if (ScreenController.Instance.DebugMode) Debug.Log("Null Response");
				return;
			}

			JSONNode jsonResponse = JSONNode.Parse(_result.RawResult);

			m_id = jsonResponse["id"];
			m_nameHuman = jsonResponse["name"];
			m_email = jsonResponse["email"];

			// if (ScreenController.Instance.DebugMode) Debug.Log("CURRENT PLAYER NAME=" + m_nameHuman + ";ID=" + m_id);

			MenuEventController.Instance.DispatchMenuEvent(EVENT_FACEBOOK_MY_INFO_LOADED);

			// if (ScreenController.Instance.DebugMode) Debug.Log("FacebookController::HandleMyInformation::result.RawResult=" + _result.RawResult);
			FB.API("/me/friends", HttpMethod.GET, HandleListOfFriends);
		}

		// -------------------------------------------
		/* 
		 * HandleListOfFriends
		 */
		private void HandleListOfFriends(IResult _result)
		{
			if (_result == null)
			{
				Debug.Log("HandleListOfFriends::Null Response");
				return;
			}

			Debug.Log("FacebookController::HandleListOfFriends::result.RawResult=" + _result.RawResult);
			JSONNode jsonResponse = JSONNode.Parse(_result.RawResult);

			JSONNode friends = jsonResponse["data"];
			Debug.Log("FacebookController::HandleListOfFriends::friends.Count=" + friends.Count);
			for (int i = 0; i < friends.Count; i++)
			{
				string nameFriend = friends[i]["name"];
				string idFriend = friends[i]["id"];
				m_friends.Add(new ItemMultiTextEntry(idFriend, nameFriend));
				Debug.Log("   NAME=" + nameFriend + ";ID=" + idFriend);
			}

			MenuEventController.Instance.DispatchMenuEvent(EVENT_FACEBOOK_FRIENDS_LOADED);

			// INIT PAYMENT METHOD
			RegisterConnectionFacebookID(true);
		}

		// -------------------------------------------
		/* 
		* RegisterConnectionFacebookID
		*/
		public void RegisterConnectionFacebookID(bool _dispatchCompletedFacebookInit)
		{
			// START BASIC CONNECTION
			if (m_id != null)
			{
				m_isInited = true;
			}
			else
			{
				m_isInited = false;
			}
			if (_dispatchCompletedFacebookInit)
			{
				MenuEventController.Instance.DispatchMenuEvent(EVENT_FACEBOOK_COMPLETE_INITIALITZATION, m_id, m_nameHuman, m_email);
			}
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EVENT_REGISTER_IAP_COMPLETED)
			{
			}
		}

		// -------------------------------------------
		/* 
		 * GetNameOfFriendID
		 */
		public string GetNameOfFriendID(string _facebookID)
		{
			for (int i = 0; i < m_friends.Count; i++)
			{
				if (m_friends[i].Items[0] == _facebookID)
				{
					return m_friends[i].Items[1];
				}
			}

			return null;
		}

		// -------------------------------------------
		/* 
		* GetPackageFriends
		*/
		public string GetPackageFriends()
		{
			string output = "";
			for (int i = 0; i < m_friends.Count; i++)
			{
				output += m_friends[i].Items[0] + "," + m_friends[i].Items[1];
				if (i < m_friends.Count - 1)
				{
					output += ";";
				}
			}
			return output;
		}

		// -------------------------------------------
		/* 
		 * SetFriends
		 */
		public void SetFriends(string _data)
		{
			string[] friendsList = _data.Split(';');
			m_friends.Clear();
			for (int i = 0; i < friendsList.Length; i++)
			{
				string[] sFriendEntry = friendsList[i].Split(',');
				if (sFriendEntry.Length == 2)
				{
					m_friends.Add(new ItemMultiTextEntry(sFriendEntry[0], sFriendEntry[1]));
					// if (ScreenController.Instance.DebugMode) Debug.Log("FacebookController::SetFriends::FRIEND[" + sFriendEntry[0] + "][" + sFriendEntry[1] + "]");
				}
			}
		}

	}
}

