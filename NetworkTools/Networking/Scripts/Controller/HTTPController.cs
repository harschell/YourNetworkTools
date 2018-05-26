using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using YourCommonTools;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * HTTPController
	 * 
	 * It manages all HTTP communications with the server
	 * 
	 * @author Esteban Gallardo
	 */
	public class HTTPController : MonoBehaviour
	{
		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const char TOKEN_SEPARATOR_COMA = ',';
		public const string TOKEN_SEPARATOR_EVENTS = "<par>";
		public const string TOKEN_SEPARATOR_LINES = "<line>";

		// ----------------------------------------------
		// COMM EVENTS
		// ----------------------------------------------	
		public const string EVENT_COMM_CREATE_NEW_ROOM = "EVENT_COMM_CREATE_NEW_ROOM";
		public const string EVENT_COMM_GET_LIST_ROOMS = "EVENT_COMM_GET_LIST_ROOMS";

		public const int STATE_IDLE = 0;
		public const int STATE_COMMUNICATION = 1;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	

		private static HTTPController _instance;

		public static HTTPController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(HTTPController)) as HTTPController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						container.name = "HTTPController";
						_instance = container.AddComponent(typeof(HTTPController)) as HTTPController;
					}
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// MEMBERS
		// ----------------------------------------------
		private int m_state = STATE_IDLE;
		private string m_event;
		private IHTTPComms m_commRequest;
		private List<TimedEventData> m_listTimedEvents = new List<TimedEventData>();
		private List<TimedEventData> m_listQueuedEvents = new List<TimedEventData>();
		private List<TimedEventData> m_priorityQueuedEvents = new List<TimedEventData>();

		private string m_inGameLog = "";

		public bool ReloadXML = false;

		// -------------------------------------------
		/* 
		 * Will delete from the text introduced by the user any special token that can break the comunication
		 */
		public static string FilterSpecialTokens(string _text)
		{
			string output = _text;

			string[] arrayEvents = output.Split(new string[] { TOKEN_SEPARATOR_EVENTS }, StringSplitOptions.None);
			output = "";
			for (int i = 0; i < arrayEvents.Length; i++)
			{
				output += arrayEvents[i];
				if (i + 1 < arrayEvents.Length)
				{
					output += " ";
				}
			}

			string[] arrayLines = output.Split(new string[] { TOKEN_SEPARATOR_LINES }, StringSplitOptions.None);
			output = "";
			for (int i = 0; i < arrayLines.Length; i++)
			{
				output += arrayLines[i];
				if (i + 1 < arrayLines.Length)
				{
					output += " ";
				}
			}

			return output;
		}

		// ----------------------------------------------
		// CONSTRUCTOR
		// ----------------------------------------------	
		// -------------------------------------------
		/* 
		 * Constructor
		 */
		private HTTPController()
		{
			m_state = STATE_IDLE;
		}

		// -------------------------------------------
		/* 
		 * Start
		 */
		void Start()
		{
			m_state = STATE_IDLE;
		}

		// -------------------------------------------
		/* 
		 * Init
		 */
		public void Init()
		{
			m_state = STATE_IDLE;
		}

		// -------------------------------------------
		/* 
		 * CreateNewRoom
		 */
		public void CreateNewRoom(bool _isLobby, string _nameRoom, string _players, string _extraData)
		{
			Request(EVENT_COMM_CREATE_NEW_ROOM, (_isLobby ? "1" : "0"), _nameRoom, _players, _extraData);
		}

		// -------------------------------------------
		/* 
		 * RequestListRooms
		 */
		public void GetListRooms(bool _isLobby, string _idUser)
		{
			Request(EVENT_COMM_GET_LIST_ROOMS, (_isLobby ? "1" : "0"), _idUser);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			Destroy(_instance.gameObject);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		 * Request
		 */
		public void Request(string _event, params object[] _list)
		{
			if (m_state != STATE_IDLE)
			{
				QueuedRequest(_event, _list);
				return;
			}

			RequestReal(_event, _list);
		}

		// -------------------------------------------
		/* 
		 * RequestPriority
		 */
		public void RequestPriority(string _event, params object[] _list)
		{
			if (m_state != STATE_IDLE)
			{
				InsertRequest(_event, _list);
				return;
			}

			RequestReal(_event, _list);
		}

		// -------------------------------------------
		/* 
		 * RequestNoQueue
		 */
		public void RequestNoQueue(string _event, params object[] _list)
		{
			if (m_state != STATE_IDLE)
			{
				return;
			}

			RequestReal(_event, _list);
		}

		// -------------------------------------------
		/* 
		 * RequestReal
		 */
		private void RequestReal(string _event, params object[] _list)
		{
			m_event = _event;
			bool isBinaryResponse = true;

			switch (m_event)
			{
				case EVENT_COMM_CREATE_NEW_ROOM:
					isBinaryResponse = false;
					m_commRequest = new CreateNewRoomHTTP();
					break;


				case EVENT_COMM_GET_LIST_ROOMS:
					isBinaryResponse = false;
					m_commRequest = new GetListRoomsHTTP();
					break;
			}

			m_state = STATE_COMMUNICATION;
			string data = m_commRequest.Build(_list);
			if (m_commRequest.Method == BaseDataHTTP.METHOD_GET)
			{
				WWW www = new WWW(m_commRequest.UrlRequest + data);
				if (isBinaryResponse)
				{
					StartCoroutine(WaitForRequest(www));
				}
				else
				{
					StartCoroutine(WaitForStringRequest(www));
				}
			}
			else
			{
				WWW www = new WWW(m_commRequest.UrlRequest, m_commRequest.FormPost.data, m_commRequest.FormPost.headers);
				if (isBinaryResponse)
				{
					StartCoroutine(WaitForRequest(www));
				}
				else
				{
					StartCoroutine(WaitForStringRequest(www));
				}
			}
		}

		// -------------------------------------------
		/* 
		 * DelayRequest
		 */
		public void DelayRequest(string _nameEvent, float _time, params object[] _list)
		{
			m_listTimedEvents.Add(new TimedEventData(_nameEvent, _time, true, _list));
		}

		// -------------------------------------------
		/* 
		 * QueuedRequest
		 */
		public void QueuedRequest(string _nameEvent, params object[] _list)
		{
			m_listQueuedEvents.Add(new TimedEventData(_nameEvent, 0, _list));
		}

		// -------------------------------------------
		/* 
		 * InsertRequest
		 */
		public void InsertRequest(string _nameEvent, params object[] _list)
		{
			m_priorityQueuedEvents.Insert(0, new TimedEventData(_nameEvent, 0, _list));
		}

		// -------------------------------------------
		/* 
		* WaitForRequest
		*/
		IEnumerator WaitForRequest(WWW www)
		{
			yield return www;

			// check for errors
			if (www.error == null)
			{
				Debug.Log("WWW Ok!: " + www.text);
				m_commRequest.Response(www.bytes);
			}
			else
			{
				Debug.LogError("WWW Error: " + www.error);
				m_commRequest.Response(Encoding.ASCII.GetBytes(www.error));

			}

			m_state = STATE_IDLE;
			ProcesQueuedComms();
		}

		// -------------------------------------------
		/* 
		* WaitForRequest
		*/
		IEnumerator WaitForStringRequest(WWW www)
		{
			yield return www;

			// check for errors
			if (www.error == null)
			{
				Debug.Log("WWW Ok!: " + www.text);
				m_commRequest.Response(www.text);
			}
			else
			{
				Debug.LogError("WWW Error: " + www.error);
				m_commRequest.Response(Encoding.ASCII.GetBytes(www.error));

			}

			m_state = STATE_IDLE;
			ProcesQueuedComms();
		}

		// -------------------------------------------
		/* 
		 * DisplayLog
		 */
		public void DisplayLog(string _data)
		{
			m_inGameLog = _data + "\n";
#if DEBUG_MODE_DISPLAY_LOG
			Debug.Log("CommController::DisplayLog::DATA=" + _data);
#endif
		}

		// -------------------------------------------
		/* 
		 * ClearLog
		 */
		public void ClearLog()
		{
			m_inGameLog = "";
		}

		private bool m_enableLog = true;

		// -------------------------------------------
		/* 
		 * OnGUI
		 */
		void OnGUI()
		{
#if DEBUG_MODE_DISPLAY_LOG
			if (!m_enableLog)
			{
				if (m_inGameLog.Length > 0)
				{
					ClearLog();
				}
			}

			if (m_enableLog)
			{
				if (m_inGameLog.Length > 0)
				{
					GUILayout.BeginScrollView(Vector2.zero);
					if (GUILayout.Button(m_inGameLog))
					{
						ClearLog();
					}
					GUILayout.EndScrollView();
				}
				else
				{
					switch (m_state)
					{
						case STATE_IDLE:
							break;

						case STATE_COMMUNICATION:
							GUILayout.BeginScrollView(Vector2.zero);
							GUILayout.Label("COMMUNICATION::Event=" + m_event);
							GUILayout.EndScrollView();
							break;
					}
				}
			}
#endif
		}

		// -------------------------------------------
		/* 
		 * ProcessTimedEvents
		 */
		private void ProcessTimedEvents()
		{
			switch (m_state)
			{
				case STATE_IDLE:
					for (int i = 0; i < m_listTimedEvents.Count; i++)
					{
						TimedEventData eventData = m_listTimedEvents[i];
						eventData.Time -= Time.deltaTime;
						if (eventData.Time <= 0)
						{
							m_listTimedEvents.RemoveAt(i);
							Request(eventData.NameEvent, eventData.List);
							eventData.Destroy();
							break;
						}
					}
					break;
			}
		}

		// -------------------------------------------
		/* 
		 * ProcesQueuedComms
		 */
		private void ProcesQueuedComms()
		{
			// PRIORITY QUEUE
			if (m_priorityQueuedEvents.Count > 0)
			{
				int i = 0;
				TimedEventData eventData = m_priorityQueuedEvents[i];
				m_priorityQueuedEvents.RemoveAt(i);
				Request(eventData.NameEvent, eventData.List);
				eventData.Destroy();
				return;
			}
			// NORMAL QUEUE
			if (m_listQueuedEvents.Count > 0)
			{
				int i = 0;
				TimedEventData eventData = m_listQueuedEvents[i];
				m_listQueuedEvents.RemoveAt(i);
				Request(eventData.NameEvent, eventData.List);
				eventData.Destroy();
				return;
			}
		}

		// -------------------------------------------
		/* 
		 * ProcessQueueEvents
		 */
		private void ProcessQueueEvents()
		{
			switch (m_state)
			{
				case STATE_IDLE:
					break;

				case STATE_COMMUNICATION:
					break;
			}
		}

		// -------------------------------------------
		/* 
		 * Update
		 */
		public void Update()
		{
			ProcessTimedEvents();
			ProcessQueueEvents();
		}
	}
}