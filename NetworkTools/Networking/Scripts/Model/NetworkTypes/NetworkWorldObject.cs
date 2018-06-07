using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * NetworkWorldObject
	 * 
	 * A world object shared in the network
	 * 
	 * @author Esteban Gallardo
	 */
	public class NetworkWorldObject : NetworkType, INetworkType
	{
		// -----------------------------------------
		// PRIVATE VARIABLES
		// -----------------------------------------
		private string m_namePrefabObject;
		private Vector3 m_initialPosition = Vector3.zero;
		private Vector3 m_initialForward = Vector3.zero;
		private Vector3 m_initialScale = Vector3.one;
		private object m_initialData;

		// -------------------------------------------
		/* 
		 * NetworkWorldObject (CREATE IN SERVER)
		 * By default the variable is created in the server and 
		 * only the server can change its value.  
		 */
		public NetworkWorldObject(string _assignedName, string _namePrefabObject, Vector3 _initialPosition, Vector3 _initialForward, Vector3 _initialScale, object _initialData) : this(_assignedName, _namePrefabObject, _initialPosition, _initialForward, _initialScale, _initialData, true, true, true)
		{

		}

		// -------------------------------------------
		/* 
		 * NetworkWorldObject
		 * Set the reference of the object created remotely
		 */
		public NetworkWorldObject(GameObject _networkObject)
		{
			InitRemoteNetworkObject(_networkObject);
		}

		// -------------------------------------------
		/* 
		* NetworkWorldObject
		* 
		* @params _assignedName: It's the identification name of the variable
		* @params _indexPrefabObject: Position in the array
		* @params _initialPosition: Initial position of the object
		* @params _initialRotation: Initial rotation of the object
		* @params _initialScale: Initial scale of the object
		* @params _allowServerChange: We allow the server to change the variable
		* @params _allowClientChange: We allow the client to change the variable
		* @params _createInServer: We create the variable on the server or locally
		*/
		public NetworkWorldObject(string _assignedName, string _namePrefabObject, Vector3 _initialPosition, Vector3 _initialForward, Vector3 _initialScale, object _initialData, bool _allowServerChange, bool _allowClientChange, bool _createInServer)
		{
			m_assignedName = _assignedName;
			m_initialPosition = _initialPosition;
			m_initialForward = _initialForward;
			m_initialScale = _initialScale;
			m_initialData = _initialData;
			m_namePrefabObject = _namePrefabObject;
			m_allowServerChange = _allowServerChange;
			m_allowClientChange = _allowClientChange;

			NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkTypeEvent);

			if (CommunicationsController.Instance.IsServer)
			{
				// IS SERVER: CREATE THE OBJECT
				Debug.Log("**********NETWORK VARIABLE["+ m_assignedName + "," + m_namePrefabObject + "] REQUESTED BY SERVER");
				NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_REQUEST_TO_CREATE_NETWORK_OBJECT,
																NetworkEventController.CLASS_WORLDOBJECTCONTROLLER_NAME,
																NetworkEventController.REGISTER_PREFABS_OBJECTS,
																m_namePrefabObject,
																Vector3.zero,
																m_assignedName,
																m_allowServerChange,
																m_allowClientChange);
			}
			else
			{

				// IS CLIENT: THEN ASK THE SERVER TO CREATE THE OBJECT
				Debug.Log("++++++++++++++NETWORK CUBE REQUESTED BY CLIENT");
				if (_createInServer == false)
				{
					Debug.Log("++++CREATE LOCALLY");
					NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_REQUEST_TO_CREATE_NETWORK_OBJECT,
																	NetworkEventController.CLASS_WORLDOBJECTCONTROLLER_NAME,
																	NetworkEventController.REGISTER_PREFABS_OBJECTS,
																	m_namePrefabObject,
																	Vector3.zero,
																	m_assignedName,
																	m_allowServerChange,
																	m_allowClientChange);
				}
				else
				{
					Debug.Log("++++CREATE REMOTELLY");
					string message = CommunicationsController.MessageCreateObject(CommunicationsController.Instance.NetworkID,
																	NetworkEventController.CLASS_WORLDOBJECTCONTROLLER_NAME,
																	NetworkEventController.REGISTER_PREFABS_OBJECTS,
																	m_namePrefabObject,
																	Vector3.zero,
																	m_assignedName,
																	m_allowServerChange,
																	m_allowClientChange);
					NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_SEND_MESSAGE_CLIENT_TO_SERVER, message);
				}
			}

			RegisterNewNetworkVariable();
		}

		// -------------------------------------------
		/* 
		 * Get the position of the object
		 */
		public Vector3 GetPosition()
		{
			if (m_networkObject == null)
			{
				Debug.Log("NetworkWorldObject::GetPosition::ERROR, THERE IS NO NETWORK OBJECT LINKED TO THIS OBJECT");
				return Vector3.zero;
			}
			return m_networkObject.GetComponent<NetworkWorldObjectData>().GetPosition();
		}

		// -------------------------------------------
		/* 
		 * Set the position
		 */
		public void SetPosition(Vector3 _newValue)
		{
			if (AllowModification())
			{
				// Debug.Log("SETTING NEW VALUE[" + _newValue.ToString() + "]");
				m_networkObject.GetComponent<NetworkWorldObjectData>().SetPosition(_newValue);
			}
			else
			{
				Debug.Log("NetworkWorldObject::SetValue::ERROR, YOU ARE NOT ALLOW TO MODIFY");
			}
		}

		// -------------------------------------------
		/* 
		 * Get the forward of the object
		 */
		public Vector3 GetForward()
		{
			if (m_networkObject == null)
			{
				Debug.Log("NetworkWorldObject::GetForward::ERROR, THERE IS NO NETWORK OBJECT LINKED TO THIS OBJECT");
				return Vector3.zero;
			}
			return m_networkObject.GetComponent<NetworkWorldObjectData>().GetForward();
		}

		// -------------------------------------------
		/* 
		 * Set the forward
		 */
		public void SetForward(Vector3 _newValue)
		{
			if (AllowModification())
			{
				// Debug.Log("SETTING NEW VALUE[" + _newValue.ToString() + "]");
				m_networkObject.GetComponent<NetworkWorldObjectData>().SetForward(_newValue);
			}
			else
			{
				Debug.Log("NetworkWorldObject::SetRotation::ERROR, YOU ARE NOT ALLOW TO MODIFY");
			}
		}


		// -------------------------------------------
		/* 
		 * Get the scale of the object
		 */
		public Vector3 GetScale()
		{
			if (m_networkObject == null)
			{
				Debug.Log("NetworkWorldObject::GetScale::ERROR, THERE IS NO NETWORK OBJECT LINKED TO THIS OBJECT");
				return Vector3.zero;
			}
			return m_networkObject.GetComponent<NetworkWorldObjectData>().GetScale();
		}

		// -------------------------------------------
		/* 
		 * Set the scale
		 */
		public void SetScale(Vector3 _newValue)
		{
			if (AllowModification())
			{
				// Debug.Log("SETTING NEW VALUE[" + _newValue.ToString() + "]");
				m_networkObject.GetComponent<NetworkWorldObjectData>().SetScale(_newValue);
			}
			else
			{
				Debug.Log("NetworkWorldObject::SetRotation::ERROR, YOU ARE NOT ALLOW TO MODIFY");
			}
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override void Destroy()
		{
			base.Destroy();
			NetworkEventController.Instance.NetworkEvent -= OnNetworkTypeEvent;
		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		public override void OnNetworkTypeEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (_nameEvent == NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_CREATION_CONFIRMATION_NETWORK_OBJECT)
			{
				if (m_networkObject == null)
				{
					GameObject reference = (GameObject)_list[0];
					if (reference.GetComponent<NetworkWorldObjectData>() != null)
					{
						if (reference.GetComponent<NetworkWorldObjectData>().AssignedName == m_assignedName)
						{
							m_networkObject = reference;
							// Debug.LogError("ASSIGNED NETWORK WORLD OBJECT DATA FOR[" + m_assignedName + "]::POSITION["+ m_initialPosition.ToString()+ "]::ROTATION["+ m_initialForward.ToString() + "]::ROTATION[" + m_initialScale.ToString() + "]-----------------------------");
							m_networkObject.GetComponent<NetworkWorldObjectData>().SetPosition(m_initialPosition);
							m_networkObject.GetComponent<NetworkWorldObjectData>().SetForward(m_initialForward);
							m_networkObject.GetComponent<NetworkWorldObjectData>().SetScale(m_initialScale);
							if (m_networkObject.GetComponent<IGameNetworkActor>() != null)
							{
								m_networkObject.GetComponent<IGameNetworkActor>().Initialize(m_initialData);
							}
						}
					}
				}
			}

			// IT SHOULD RUN AFTER THE PREVIOUS CODE
			base.OnNetworkTypeEvent(_nameEvent, _isLocalEvent, _networkOriginID, _networkTargetID, _list);
		}
	}
}