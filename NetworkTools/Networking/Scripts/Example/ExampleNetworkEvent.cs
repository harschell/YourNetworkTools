using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine.SceneManagement;
using YourNetworkingTools;

/******************************************
* 
* ExampleNetworkEvent
*    
* Simple example of the use of network event to communicate between clients
*
* @author Esteban Gallardo
*/
public class ExampleNetworkEvent : MonoBehaviour
{
	// ----------------------------------------------
	// EVENTS
	// ----------------------------------------------	
	public const string EVENT_HELLOWORLD_MESSAGE = "EVENT_HELLOWORLD_MESSAGE";

	// ----------------------------------------------
	// PRIVATE MEMBERS
	// ----------------------------------------------	
	private List<string> m_messagesOnScreen = new List<string>();
	private float m_timeOutToClean = 0;

	// -------------------------------------------
	/* 
	* Initialitzation
	*/
	void Start()
	{
		NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
	}

	// -------------------------------------------
	/* 
	* Remove references and release memory
	*/
	public void Destroy()
	{
		NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
	}

	// -------------------------------------------
	/* 
	* Destroy
	*/
	void OnDestroy()
	{
		Destroy();
	}

	// -------------------------------------------
	/* 
	* DisplayLogMessage
	*/
	private void DisplayLogMessage(string _message)
	{
		Debug.Log(_message);
		m_messagesOnScreen.Add(_message);
		m_timeOutToClean = 3;
	}

	// -------------------------------------------
	/* 
	* Manager of global events
	*/
	private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
	{
		if (_nameEvent == NetworkEventController.EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED)
		{
			DisplayLogMessage("OnNetworkEvent::NEW CONNECTION[" + (int)_list[0] + "]");
			m_timeOutToClean = 6;
		}
		if (_nameEvent == EVENT_HELLOWORLD_MESSAGE)
		{
			if (_networkOriginID != YourNetworkTools.Instance.GetUniversalNetworkID())
			{
				DisplayLogMessage("OnNetworkEvent::EVENT_HELLOWORLD_MESSAGE::RECEIVED A MESSAGE FROM[" + _networkOriginID + "] WITH THE DATA[" + (string)_list[0] + "]");
			}			
		}
	}

	// -------------------------------------------
	/* 
    * Display information about the operation mode
    */
	void OnGUI()
	{
		GUILayout.BeginVertical();
		if (YourNetworkTools.Instance.GetUniversalNetworkID() == -1)
		{
			GUILayout.Label(new GUIContent(""));
			GUILayout.Label(new GUIContent("SERVER IS SETTING UP. WAIT..."));
		}
		else
		{
			GUILayout.Label(new GUIContent(""));
			GUILayout.Label(new GUIContent("MOUSE CLICK TO INTERACT"));
		}
		for (int i = 0; i < m_messagesOnScreen.Count; i++)
		{
			GUILayout.Label(new GUIContent(m_messagesOnScreen[i]));
		}
		GUILayout.EndVertical();
	}

	// -------------------------------------------
	/* 
	* Will send network events
	*/
	void Update()
	{
		if (YourNetworkTools.Instance.GetUniversalNetworkID() != -1)
		{
			if (Input.GetMouseButtonDown(0))
			{
				if  (YourNetworkTools.Instance.IsServer)
				{
					NetworkEventController.Instance.DispatchNetworkEvent(EVENT_HELLOWORLD_MESSAGE, "Hi, I'm the server.");
				}
				else
				{
					NetworkEventController.Instance.DispatchNetworkEvent(EVENT_HELLOWORLD_MESSAGE, "Hello, I'm the client.");
				}				
			}

			if (m_timeOutToClean > 0)
			{
				m_timeOutToClean -= Time.deltaTime;
				if (m_timeOutToClean <= 0)
				{
					m_messagesOnScreen.Clear();
				}
			}			
		}
	}
}
