using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine.SceneManagement;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * NetworkVariablesController
	 * 
	 * It handles the network variables: creation, linkeage, destroy
	 *
	 * @author Esteban Gallardo
	 */
	[AddComponentMenu("WorldObjectController")]
	public class NetworkVariablesController : WorldObjectController, INetworkResources
	{
		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static NetworkVariablesController _instance;

		public static NetworkVariablesController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(NetworkVariablesController)) as NetworkVariablesController;
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		// A NETWORK INTEGER
		protected List<INetworkVariable> m_networkVariables = new List<INetworkVariable>();

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public List<INetworkVariable> NetworkVariables
		{
			get { return m_networkVariables; }
		}

		// -------------------------------------------
		/* 
		* Initialitzation
		*/
		public override void Start()
		{
			this.name = this.GetType().Name;
			NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override void Destroy()
		{
			base.Destroy();

			// VARIABLES
			for (int i = 0; i < m_networkVariables.Count; i++)
			{
				if (m_networkVariables[i] != null)
				{
					try
					{
						m_networkVariables[i].Destroy();
						m_networkVariables[i] = null;
					}
					catch (Exception err)
					{
						Debug.Log("err=" + err.StackTrace);
					}
				}
			}
			m_networkVariables.Clear();

			_instance = null;
		}

		// -------------------------------------------
		/* 
		* Get the network variable by its name
		*/
		protected INetworkVariable GetNetworkVariable(string _name)
		{
			for (int i = 0; i < m_networkVariables.Count; i++)
			{
				if (m_networkVariables[i].Name == _name)
				{
					return m_networkVariables[i];
				}
			}
			return null;
		}

		// -------------------------------------------
		/* 
		* Get the network variable by its name
		*/
		protected INetworkVariable GetNetworkVariable(INetworkVariable _networkVariable)
		{
			for (int i = 0; i < m_networkVariables.Count; i++)
			{
				if (m_networkVariables[i] == _networkVariable)
				{
					return m_networkVariables[i];
				}
			}
			return null;
		}


		// -------------------------------------------
		/* 
		* Remove the network variable by its name
		*/
		protected bool RemoveNetworkVariable(string _name)
		{
			for (int i = 0; i < m_networkVariables.Count; i++)
			{
				if (m_networkVariables[i].Name == _name)
				{
					m_networkVariables[i].Destroy();
					m_networkVariables.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		// -------------------------------------------
		/* 
		* Manager of global events
		*/
		protected override void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			base.OnNetworkEvent(_nameEvent, _isLocalEvent, _networkOriginID, _networkTargetID, _list);

			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS)
			{
				m_requestedDestroyEverything = true;
				Destroy();
			}
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_VARIABLE_CREATE_LOCAL)
			{
				INetworkVariable networkVariable = (INetworkVariable)_list[0];
				if (GetNetworkVariable(networkVariable) == null)
				{
					Debug.Log("ADDING THE NEW VARIABLE TO THE SYSTEM::networkVariable=" + networkVariable.Name);
					m_networkVariables.Add(networkVariable);
				}
			}
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_VARIABLE_CREATE_REMOTE)
			{
				int ownerVariable = int.Parse((string)_list[0]);
				string nameVariable = (string)_list[1];
				string valueVariable = (string)_list[2];
				string typeVariable = (string)_list[3];
				if (GetNetworkVariable(nameVariable) == null)
				{
					INetworkVariable networkVariable = null;
					if (NetworkVector3.GetTypeVector3() == typeVariable)
					{
						NetworkVector3 networkVector3 = new NetworkVector3();
						networkVector3.InitLocal(ownerVariable, nameVariable, valueVariable);
						networkVariable = networkVector3;
					}
					else if (NetworkInteger.GetTypeInteger().ToString() == typeVariable)
					{
						NetworkInteger networkInteger = new NetworkInteger();
						networkInteger.InitLocal(ownerVariable, nameVariable, int.Parse(valueVariable));
						networkVariable = networkInteger;
					}
					else if (NetworkString.GetTypeString().ToString() == typeVariable)
					{
						NetworkString networkString = new NetworkString();
						networkString.InitLocal(ownerVariable, nameVariable, valueVariable);
						networkVariable = networkString;
					}
					else if (NetworkFloat.GetTypeFloat().ToString() == typeVariable)
					{
						NetworkFloat networkFloat = new NetworkFloat();
						networkFloat.InitLocal(ownerVariable, nameVariable, valueVariable);
						networkVariable = networkFloat;
					}

					if (networkVariable != null)
					{
						m_networkVariables.Add(networkVariable);
					}
				}
			}
			if (_nameEvent == NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_DESTROY_CONFIRMATION)
			{
				if (!m_requestedDestroyEverything)
				{
					string nameAssignedToDestroy = (string)_list[0];
					if (!RemoveNetworkVariable(nameAssignedToDestroy))
					{
						Debug.Log("NetworkVariablesController::EVENT_WORLDOBJECTCONTROLLER_DESTROY_CONFIRMATION::The network variable[" + nameAssignedToDestroy + "] was NOT removed");
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		* Manager of network type events
		*/
		private void OnNetworkTypeEvent(string _nameEvent, INetworkType _networkObject, params object[] _list)
		{
			if (_nameEvent == NetworkType.EVENT_NETWORKTYPE_CHANGED_VALUE)
			{
				Debug.Log("[**SERVER**]::DETECTED THE CHANGED VALUE=" + _networkObject.NetworkObject.GetComponent<INetworkObject>().GetInformation() + "++++++");
			}
		}
	}
}