using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourNetworkingTools
{
	public delegate void NetworkVariableEventHandler(string _nameEvent, string _nameVariable, params object[] _list);

	/******************************************
	* 
	* NetworkVariable
	* 
	* An variable shared by all connected clients
	* 
	* @author Esteban Gallardo
	*/
	public class NetworkVariable : INetworkVariable
	{
		public event NetworkVariableEventHandler NetworkVariableEvent;

		// -----------------------------------------
		// EVENTS
		// -----------------------------------------
		public const string EVENT_NETWORKVARIABLE_CREATE = "EVENT_NETWORKVARIABLE_CREATE";
		public const string EVENT_NETWORKVARIABLE_UPDATED = "EVENT_NETWORKVARIABLE_UPDATED";
		public const string EVENT_NETWORKVARIABLE_DELETE = "EVENT_NETWORKVARIABLE_DELETE";

		// -----------------------------------------
		// PRIVATE VARIABLES
		// -----------------------------------------
		protected int m_owner;
		protected string m_name;
		protected bool m_hasBeenDestroyed = false;

		// -----------------------------------------
		// GETTERS/SETTERS
		// -----------------------------------------
		public int Owner
		{
			get { return m_owner; }
		}
		public string Name
		{
			get { return m_name; }
		}

		// -------------------------------------------
		/* 
		* Initialize resources
		*/
		public virtual void InitRemote(params object[] _list)
		{
			if (_list.Length != 3)
			{
				throw new Exception("NetworkVariable::YOU SHOULD SET 3 VALUES FOR THE VARIABLE, OWNER+NAME+VALUE");
			}
			if (_list[0].GetType() != Type.GetType("System.Int32"))
			{
				throw new Exception("NetworkVariable::YOU SHOULD SET UP THE NAME OF THE VARIABLE AS AN INTEGER");
			}
			if (_list[1].GetType() != Type.GetType("System.String"))
			{
				throw new Exception("NetworkVariable::YOU SHOULD SET UP THE NAME OF THE VARIABLE AS A STRING");
			}
			m_owner = (int)_list[0];
			m_name = (string)_list[1];
			UpdateValue(_list[2]);
			NetworkEventController.Instance.NetworkEvent += OnNetworkEvent;
			NetworkEventController.Instance.DispatchNetworkEvent(NetworkEventController.EVENT_SYSTEM_VARIABLE_CREATE_REMOTE, m_owner.ToString(), m_name, GetValue().ToString(), GetTypeValueInString());
		}

		// -------------------------------------------
		/* 
		* Initialize resources
		*/
		public virtual void InitLocal(params object[] _list)
		{
			if (_list.Length != 3)
			{
				throw new Exception("NetworkVariable::YOU SHOULD SET 3 VALUES FOR THE VARIABLE, OWNER+NAME+VALUE");
			}
			if (_list[0].GetType() != Type.GetType("System.Int32"))
			{
				throw new Exception("NetworkVariable::YOU SHOULD SET UP THE OWNER OF THE VARIABLE AS AN INTEGER");
			}
			if (_list[1].GetType() != Type.GetType("System.String"))
			{
				throw new Exception("NetworkVariable::YOU SHOULD SET UP THE NAME OF THE VARIABLE AS A STRING");
			}
			m_owner = (int)_list[0];
			m_name = (string)_list[1];
			UpdateValue(_list[2]);
			NetworkEventController.Instance.NetworkEvent += OnNetworkEvent;
			NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_SYSTEM_VARIABLE_CREATE_LOCAL, this);
		}

		// -------------------------------------------
		/* 
		* Release resources
		*/
		public virtual void Destroy()
		{
			if (m_hasBeenDestroyed) return;
			m_hasBeenDestroyed = true;

			NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
			NetworkEventController.Instance.DispatchNetworkEvent(EVENT_NETWORKVARIABLE_DELETE, m_name);
		}

		// -------------------------------------------
		/* 
		* Report changes to all the actors who are listening
		*/
		public void DispatchNetworkVariableEvent(string _nameEvent, params object[] _list)
		{
			if (NetworkVariableEvent != null) NetworkVariableEvent(_nameEvent, m_name, _list);
		}

		// -------------------------------------------
		/* 
		* Manager of network events
		*/
		public virtual void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (_networkOriginID == YourNetworkTools.Instance.GetUniversalNetworkID()) return;

			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED)
			{
				NetworkEventController.Instance.DelayNetworkEvent(NetworkEventController.EVENT_SYSTEM_VARIABLE_CREATE_REMOTE, 2, m_name, GetValue().ToString(), GetTypeValueInString());
			}
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_VARIABLE_SET)
			{
				string nameVariable = (string)_list[0];
				if (m_name == nameVariable)
				{
					UpdateValue(_list[1]);
				}
			}
		}

		// -------------------------------------------
		/* 
		* Get the value of the variable
		*/
		public virtual object GetValue()
		{
			throw new Exception("NetworkVariable::GetValue::SHOULD BE IMPLEMENTED");
		}

		// -------------------------------------------
		/* 
		* Set the value of the variable in all connected clients
		*/
		public virtual void SetValue(object _value)
		{
			throw new Exception("NetworkVariable::SetValue::SHOULD BE IMPLEMENTED");
		}

		// -------------------------------------------
		/* 
		* Update the local value
		*/
		public virtual void UpdateValue(object _value)
		{
			throw new Exception("NetworkVariable::UpdateValue::SHOULD BE IMPLEMENTED");
		}

		// -------------------------------------------
		/* 
		* Get the type of the variable
		*/
		public virtual Type GetTypeValue()
		{
			throw new Exception("NetworkVariable::GetTypeValue::SHOULD BE IMPLEMENTED");
		}

		// -------------------------------------------
		/* 
		* Get the type of the variable
		*/
		public virtual string GetTypeValueInString()
		{
			throw new Exception("NetworkVariable::GetTypeValueInString::SHOULD BE IMPLEMENTED");
		}

		// -------------------------------------------
		/* 
		* Get a description text of the variable
		*/
		public virtual string GetInformation()
		{
			throw new Exception("NetworkVariable::GetInformation::SHOULD BE IMPLEMENTED");
		}

		// -------------------------------------------
		/* 
		* Check equality
		*/
		public virtual bool Equals(INetworkVariable _other)
		{
			return (m_name == _other.Name);
		}

		// -------------------------------------------
		/* 
		* Apply a random increase, used for testing
		*/
		public virtual void RandomIncrease(int _value)
		{
			throw new Exception("NetworkVariable::RandomIncrease::SHOULD BE IMPLEMENTED");
		}
	}
}
