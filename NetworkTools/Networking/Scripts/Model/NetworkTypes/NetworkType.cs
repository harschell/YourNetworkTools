using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourNetworkingTools
{
	public delegate void NetworkTypeEventHandler(string _nameEvent, INetworkType _networkObject, params object[] _list);

	/******************************************
	 * 
	 * NetworkType
	 * 
	 * Base class of the network type
	 * 
	 * @author Esteban Gallardo
	 */
	public class NetworkType : INetworkType
	{
		public event NetworkTypeEventHandler NetworkTypeEvent;

		// ----------------------------------------------	
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_NETWORKTYPE_CHANGED_VALUE = "EVENT_NETWORKTYPE_CHANGED_VALUE";

		// ----------------------------------------------
		// PROTECTED MEMBERS
		// ----------------------------------------------	
		protected bool m_allowServerChange = false;
		protected bool m_allowClientChange = false;
		protected string m_assignedName;

		protected GameObject m_networkObject;
		protected bool m_hasBeenDestroyed = false;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public string AssignedName
		{
			get { return m_assignedName; }
		}
		public GameObject NetworkObject
		{
			get { return m_networkObject; }
		}
		public int UID
		{
			get { return m_networkObject.GetComponent<NetworkObjectData>().UID; }
		}
		public int NetID
		{
			get { return m_networkObject.GetComponent<NetworkObjectData>().NetID; }
		}
		public bool HasBeenDestroyed
		{
			get { return m_hasBeenDestroyed; }
		}

		// -------------------------------------------
		/* 
		 * Set the value
		 */
		public NetworkObjectData GetNetworkObjectData()
		{
			if (m_networkObject == null)
			{
				return null;
			}
			else
			{
				return m_networkObject.GetComponent<NetworkObjectData>();
			}			
		}

		// -------------------------------------------
		/* 
		 * Check the unique identification
		 */
		public bool CheckID(int _netID, int _uid)
		{
			return m_networkObject.GetComponent<NetworkObjectData>().CheckID(_netID, _uid);
		}

		// -------------------------------------------
		/* 
		 * Get the unique identification
		 */
		public string GetID()
		{
			return m_networkObject.GetComponent<NetworkObjectData>().GetID();
		}

		// -------------------------------------------
		/* 
		 * Set the value
		 */
		public virtual void SetObjectValue(object _value)
		{

		}

		// -------------------------------------------
		/* 
		 * Set the value
		 */
		public virtual object GetObjectValue()
		{
			return null;
		}

		// -------------------------------------------
		/* 
		 * Increase the value
		 */
		public virtual void Increase(object _value)
		{

		}

		// -------------------------------------------
		/* 
		 * Decrease the value
		 */
		public virtual void Decrease(object _value)
		{

		}


		// -------------------------------------------
		/* 
		 * Will register in the system a new network variable
		 */
		public virtual void RegisterNewNetworkVariable()
		{
			NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_NETWORKVARIABLE_REGISTER_NEW, this);
		}

		// -------------------------------------------
		/* 
		 * Will check if it allows the modification
		 */
		public virtual bool AllowModification()
		{
			if (m_networkObject == null)
			{
				Debug.Log("NetworkString::SetValue::ERROR, THERE IS NO NETWORK OBJECT LINKED TO THIS OBJECT");
				return false;
			}

			bool allowModify = true;
			if (!m_allowServerChange && CommunicationsController.Instance.IsServer && m_networkObject.GetComponent<INetworkObject>().IsLocalPlayer())
			{
				allowModify = false;
			}
			if (!m_allowClientChange && !CommunicationsController.Instance.IsServer && m_networkObject.GetComponent<INetworkObject>().IsLocalPlayer())
			{
				allowModify = false;
			}

			return allowModify;
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public virtual void Destroy()
		{
			if (m_networkObject != null)
			{
				m_hasBeenDestroyed = true;
				GameObject tempReference = m_networkObject;
				m_networkObject = null;
				GameObject.Destroy(tempReference);
			}
		}

		// -------------------------------------------
		/* 
		 * Will dispatch an event
		 */
		public void DispatchNetworkTypeEvent(string _nameEvent, INetworkType _networkObject, params object[] _list)
		{
			if (NetworkTypeEvent != null)
			{
				NetworkTypeEvent(_nameEvent, _networkObject, _list);
			}
		}

		// -------------------------------------------
		/* 
		 * Will dispatch an event
		 */
		public void InitRemoteNetworkObject(GameObject _networkObject)
		{
			if (_networkObject.GetComponent<INetworkObject>() == null)
			{
				Debug.Log("NetworkWorldObject::REMOTE OBJECT INSTANTIATION FAILED::_networkObject=" + _networkObject.name + "------------------------------");
			}
			else
			{
				m_assignedName = _networkObject.GetComponent<INetworkObject>().AssignedName;
				m_allowServerChange = _networkObject.GetComponent<INetworkObject>().AllowServerChange;
				m_allowClientChange = _networkObject.GetComponent<INetworkObject>().AllowClientChange;

				m_networkObject = _networkObject;

				NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkTypeEvent);
				Debug.Log("NetworkWorldObject::REMOTE OBJECT INSTANTIATION SUCCESS::_networkObject[" + _networkObject.name + "]::ASSIGNED NAME[" + m_assignedName + "]+++++++++++++++++++++++++++++++++++++");
			}
		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		public virtual void OnNetworkTypeEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (_nameEvent == NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_DESTROY_CONFIRMATION_NETWORK_OBJECT)
			{
				string nameToDestroy = (string)_list[0];
				int uidToDestroy = (int)_list[1];
				int netIDToDestroy = (int)_list[2];
				string typeObjectToDestroy = (string)_list[3];
				string assignedName = (string)_list[4];

				if (assignedName == this.m_assignedName)
				{
					m_networkObject = null;
					Destroy();
				}
			}
			if (_nameEvent == NetworkEventController.EVENT_NETWORKVARIABLE_STATE_REPORT)
			{
				GameObject reportObject = (GameObject)_list[0];
				if (reportObject == m_networkObject)
				{
					DispatchNetworkTypeEvent(EVENT_NETWORKTYPE_CHANGED_VALUE, this);
				}
			}
			if (_nameEvent == NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_CREATION_CONFIRMATION_NETWORK_OBJECT)
			{
				GameObject reference = (GameObject)_list[0];
				if (m_networkObject != null)
				{
					if (reference == m_networkObject)
					{
						NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_CREATION_CONFIRMATION_REGISTRATION, (GameObject)_list[0], this);
					}
				}
			}
		}

	}
}