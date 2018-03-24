using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourNetworkingTools
{
	/******************************************
	* 
	* ActorNetwork
	* 
	* Base class of the common properties of a game's actor
	* 
	* @author Esteban Gallardo
	*/
	public class ActorNetwork : MonoBehaviour
	{
		// ----------------------------------------------
		// PROTECTED MEMBERS
		// ----------------------------------------------	
		private NetworkID m_networkID;
		private string m_eventNameObjectCreated = "";

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public NetworkID NetworkID
		{
			get
			{
				if (YourNetworkTools.Instance.IsLocalGame)
				{
					Initialize();
				}
				if (m_networkID == null)
				{
					m_networkID = this.gameObject.GetComponent<NetworkID>();
				}
				return m_networkID;
			}
		}
		public string EventNameObjectCreated
		{
			set { m_eventNameObjectCreated = value; }
		}

		// -------------------------------------------
		/* 
		 * Report the event in the system when a new player has been created.
		 * 
		 * The player could have been created by a remote client so we should throw an event
		 * so that the controller will be listening to it.
		 */
		void Start()
		{
			if (m_eventNameObjectCreated == "")
			{
				Debug.LogError("ReportCreationObject::YOU SHOULD DEFINE IN THE CONSTRUCTOR THE EVENT TO REPORT THE CREATION OF THE GAME OBJECT IN THE SYSTEM");
			}
			NetworkEventController.Instance.DispatchLocalEvent(m_eventNameObjectCreated, this.gameObject);
			if (IsMine())
			{
				NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_LOCAL_CREATION_CONFIRMATION, this.gameObject);
			}
			else
			{
				NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_REMOTE_CREATION_CONFIRMATION, this.gameObject);
			}
			NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
		}

		// -------------------------------------------
		/* 
		* Initialize the identification of the network object
		*/
		public void Initialize()
		{
			if (m_networkID == null)
			{
				if (this.gameObject.GetComponent<NetworkID>() == null)
				{
					this.gameObject.AddComponent<NetworkID>();
				}
				m_networkID = this.gameObject.GetComponent<NetworkID>();
				m_networkID.NetID = this.gameObject.GetComponent<NetworkWorldObjectData>().NetID;
				m_networkID.UID = this.gameObject.GetComponent<NetworkWorldObjectData>().UID;
			}
		}

		// -------------------------------------------
		/* 
		 * Check it the actor belongs to the current player
		 */
		public bool IsMine()
		{
			return (YourNetworkTools.Instance.GetUniversalNetworkID() == NetworkID.NetID);
		}

		// -------------------------------------------
		/* 
		 * Will dispatch an event that will destroy the object in all the network
		 */
		void OnDestroy()
		{
#if DEBUG_MODE_DISPLAY_LOG
			Debug.LogError("[ActorNetwork] ++SEND++ SIGNAL FOR AUTODESTRUCTION");
#endif
			NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
			NetworkEventController.Instance.DispatchNetworkEvent(NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_DESTROY_REQUEST, NetworkID.NetID.ToString(), NetworkID.UID.ToString());
		}

		// -------------------------------------------
		/* 
		 * OnNetworkEvent
		 */
		private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (_nameEvent == NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_DESTROY_REQUEST)
			{
				if ((NetworkID.NetID == int.Parse((string)_list[0]))
					&& (NetworkID.UID == int.Parse((string)_list[1])))
				{
#if DEBUG_MODE_DISPLAY_LOG
			Debug.LogError("[ActorNetwork] --RECEIVE-- SIGNAL FOR AUTODESTRUCTION");
#endif
					GameObject.Destroy(this.gameObject);
				}
			}
		}
	}
}