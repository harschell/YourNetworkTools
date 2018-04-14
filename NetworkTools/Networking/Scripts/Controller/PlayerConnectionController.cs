using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * PlayerConnectionController
	 * 
	 * Instance of the player in the world 
	 * created by the NetworkManager once the
	 * application starts.
	 * 
	 * It will handle the network connection
	 * 
	 * @author Esteban Gallardo
	 */
	[NetworkSettings(sendInterval = 0.033f)]
	public class PlayerConnectionController : NetworkBehaviour
	{
		// -----------------------------------------
		// PUBLIC VARIABLES
		// -----------------------------------------
		public GameObject NetworkMessage;       // Prefab used in Command to send a message to server

		// -----------------------------------------
		// PRIVATE VARIABLES
		// -----------------------------------------
		/// The transform of the shared world anchor.
		private Transform m_sharedWorldAnchorTransform;

		// DEBUG VARIABLES
		private bool m_isMousePressed = false;
		private float m_timePressed = 0;

		// -------------------------------------------
		/* 
		 * Create the instance of the player in all the running clients.
		 * In the case of the server, it adds the new player to the list
		 * of connected clients.
		 */
		private void Start()
		{
			if (SharedCollection.Instance == null)
			{
				Debug.Log("This script required a SharedCollection script attached to a gameobject in the scene");
				Destroy(this);
				return;
			}

			if (isLocalPlayer)
			{
				CommunicationsController.Instance.SetLocalInstance((int)this.netId.Value, this);
				NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnBasicEvent);
			}
			else
			{
				// Adds the new connections to the list of connected clients in the server
				CommunicationsController.Instance.ClientNewConnection((int)this.netId.Value, this.gameObject);
				Debug.Log("PlayerController::Start::REMOTE PLAYER");
			}

			m_sharedWorldAnchorTransform = SharedCollection.Instance.gameObject.transform;
			transform.SetParent(m_sharedWorldAnchorTransform);

			if (isLocalPlayer)
			{
				NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_SYSTEM_INITIALITZATION_LOCAL_COMPLETED, (int)this.netId.Value);
			}
			else
			{
				NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_SYSTEM_INITIALITZATION_REMOTE_COMPLETED, ((int)this.netId.Value).ToString());
			}
		}

		// -------------------------------------------
		/* 
		 * Disconnects from the network
		 */
		void OnDestroy()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * Disconnects from the network
		 */
		private void Destroy()
		{
			if (!isLocalPlayer)
			{
				if (CommunicationsController.Instance != null)
				{
					if (CommunicationsController.Instance.IsServer)
					{
						// REMOVE A PLAYER FROM THE LIST OF CONNECTIONS OF THE SERVER
						CommunicationsController.Instance.ClientDisconnected((int)this.netId.Value);
					}
				}
			}
			else
			{
				NetworkEventController.Instance.NetworkEvent -= OnBasicEvent;
			}
		}

		// -------------------------------------------
		/* 
		 * Format the unique name to identify this object
		 */
		public static string GetNameIdentificator(INetworkObject _obj)
		{
			return GetNameIdentificator(_obj.PrefabName, _obj.UID, _obj.NetID);
		}

		// -------------------------------------------
		/* 
		 * Format the unique name to identify this object
		 */
		public static string GetNameIdentificator(string _prefabName, int _uid, int _netID)
		{
			return _prefabName + "_" + _uid + "_" + _netID;
		}

		// -------------------------------------------
		/* 
		 * Create an animated object
		 */
		[Command]
		void CmdNetworkObject(string _classNetworkResources, string _typeObjects, string _prefabName, int _uid, int _netID, Vector3 _position, bool _preservePosition, string _assignedName, bool _allowServerChange, bool _allowClientChange)
		{
			Vector3 networkObjectPos = _position;

			GameObject prefab = CommunicationsController.Instance.GetPrefabFromClass(_classNetworkResources, _typeObjects, _prefabName);
			if (prefab != null)
			{
				GameObject newNetworkObject = (GameObject)Instantiate(prefab, m_sharedWorldAnchorTransform.InverseTransformPoint(networkObjectPos), new Quaternion(0, 0, 0, 0));
				newNetworkObject.GetComponent<INetworkObject>().UID = _uid;
				newNetworkObject.GetComponent<INetworkObject>().NetID = _netID;
				newNetworkObject.GetComponent<INetworkObject>().PrefabName = prefab.name;
				newNetworkObject.GetComponent<INetworkObject>().TypeObject = _typeObjects;
				newNetworkObject.GetComponent<INetworkObject>().PreserveTransform = _preservePosition;
				newNetworkObject.GetComponent<INetworkObject>().AssignedName = _assignedName;
				newNetworkObject.GetComponent<INetworkObject>().AllowServerChange = _allowServerChange;
				newNetworkObject.GetComponent<INetworkObject>().AllowClientChange = _allowClientChange;				

				NetworkServer.Spawn(newNetworkObject);
			}
			else
			{
				Debug.Log("PlayerConnectionController::CmdNetworkObject::THE OBJECT[" + _classNetworkResources + "," + _prefabName + "] COULD NOT BE SPAWNED");
			}
		}

		// -------------------------------------------
		/* 
		 * Update an object's property (Quaternion)
		 */
		[Command]
		void CmdUpdateObjectPropertyQuaternion(int _uid, int _netID, string _prefab, string _functionName, Quaternion _value)
		{
			GameObject objectToUpdate = GameObject.Find(GetNameIdentificator(_prefab, _uid, _netID));
			if (objectToUpdate != null) objectToUpdate.GetComponent<INetworkObject>().InvokeFunction(_functionName, _value);
		}

		// -------------------------------------------
		/* 
		 * Update an object's property (Vector3)
		 */
		[Command]
		void CmdUpdateObjectPropertyVector3(int _uid, int _netID, string _prefab, string _functionName, Vector3 _value)
		{
			GameObject objectToUpdate = GameObject.Find(GetNameIdentificator(_prefab, _uid, _netID));
			if (objectToUpdate != null) objectToUpdate.GetComponent<INetworkObject>().InvokeFunction(_functionName, _value);
		}

		// -------------------------------------------
		/* 
		 * Update an object's property (int)
		 */
		[Command]
		void CmdUpdateObjectPropertyInt(int _uid, int _netID, string _prefab, string _functionName, int _value)
		{
			GameObject objectToUpdate = GameObject.Find(GetNameIdentificator(_prefab, _uid, _netID));
			if (objectToUpdate != null) objectToUpdate.GetComponent<INetworkObject>().InvokeFunction(_functionName, _value);
		}

		// -------------------------------------------
		/* 
		 * Update an object's property (long)
		 */
		[Command]
		void CmdUpdateObjectPropertyLong(int _uid, int _netID, string _prefab, string _functionName, long _value)
		{
			GameObject objectToUpdate = GameObject.Find(GetNameIdentificator(_prefab, _uid, _netID));
			if (objectToUpdate != null) objectToUpdate.GetComponent<INetworkObject>().InvokeFunction(_functionName, _value);
		}

		// -------------------------------------------
		/* 
		 * Update an object's property (float)
		 */
		[Command]
		void CmdUpdateObjectPropertyFloat(int _uid, int _netID, string _prefab, string _functionName, float _value)
		{
			GameObject objectToUpdate = GameObject.Find(GetNameIdentificator(_prefab, _uid, _netID));
			if (objectToUpdate != null) objectToUpdate.GetComponent<INetworkObject>().InvokeFunction(_functionName, _value);
		}

		// -------------------------------------------
		/* 
		 * Update an object's property (double)
		 */
		[Command]
		void CmdUpdateObjectPropertyDouble(int _uid, int _netID, string _prefab, string _functionName, double _value)
		{
			GameObject objectToUpdate = GameObject.Find(GetNameIdentificator(_prefab, _uid, _netID));
			if (objectToUpdate != null) objectToUpdate.GetComponent<INetworkObject>().InvokeFunction(_functionName, _value);
		}


		// -------------------------------------------
		/* 
		 * Update an object's property (double)
		 */
		[Command]
		void CmdUpdateObjectPropertyString(int _uid, int _netID, string _prefab, string _functionName, string _value)
		{
			GameObject objectToUpdate = GameObject.Find(GetNameIdentificator(_prefab, _uid, _netID));
			if (objectToUpdate != null) objectToUpdate.GetComponent<INetworkObject>().InvokeFunction(_functionName, _value);
		}

		// -------------------------------------------
		/* 
		 * Create a message from the server to the clients
		 */
		[ClientRpc]
		private void RpcMessageFromServerToClients(int _playerConnectionID, int _playerOriginConnectionID, string _message)
		{
			if (CommunicationsController.Instance.IsServer)
			{
				return;
			}

			CommunicationsController.Instance.MessageFromServerToClients(_playerConnectionID, _playerOriginConnectionID, _message);
		}

		// -------------------------------------------
		/* 
		 * Create a message from the clients to the server
		 */
		[Command]
		private void CmdMessageFromClientsToServer(int _playerConnectionID, string _message)
		{
			CommunicationsController.Instance.MessageFromClientsToServer(_playerConnectionID, _message);
		}

		// -------------------------------------------
		/* 
		 * Send the texture data to the server
		 */
		[Command]
		private void CmdTextureFromClientsToServer(int _playerConnectionID, int texWidth, int textHeight, byte[] _textureData)
		{
			CommunicationsController.Instance.SetTextureFromClientsToServer(_playerConnectionID, texWidth, textHeight, _textureData);
		}


		// -------------------------------------------
		/* 
		* Manager of global events
		*/
		private void OnBasicEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS)
			{
				Destroy();
			}
			if (isLocalPlayer)
			{
				if (_nameEvent == NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_KICK_OUT_PLAYER)
				{
					if (isLocalPlayer)
					{
						if (int.Parse((string)_list[0]) == (int)this.netId.Value)
						{
							NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS);
							NetworkEventController.Instance.DelayLocalEvent(NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_CONFIRMATION_KICKED_OUT_PLAYER, 0.1f);
						}
					}
				}
				/////////////////////////////////
				// LISTEN ONLY LOCAL PLAYER
				/////////////////////////////////
				if (_nameEvent == NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_CREATE_NETWORK_OBJECT)
				{
					string classNetworkResources = (string)_list[0];
					string typeObjects = (string)_list[1];
					string prefabName = (string)_list[2];
					int uid = (int)_list[3];
					Vector3 position = (Vector3)_list[4];
					bool preservePosition = (bool)_list[5];
					string assignedName = (string)_list[6];
					bool allowServerChange = (bool)_list[7];
					bool allowClientChange = (bool)_list[8];

					CmdNetworkObject(classNetworkResources, typeObjects, prefabName, uid, (int)this.netId.Value, position, preservePosition, assignedName, allowServerChange, allowClientChange);
				}
				if (_nameEvent == NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_UPDATE_PROPERTY)
				{
					INetworkObject networkObject = ((GameObject)_list[0]).GetComponent<INetworkObject>();
					string functionName = (string)_list[1];
					object newValue = (object)_list[2];

					Type t = newValue.GetType();
					if (t.Equals(typeof(Quaternion)))
						CmdUpdateObjectPropertyQuaternion(networkObject.UID, networkObject.NetID, networkObject.PrefabName, functionName, (Quaternion)newValue);
					else if (t.Equals(typeof(Vector3)))
						CmdUpdateObjectPropertyVector3(networkObject.UID, networkObject.NetID, networkObject.PrefabName, functionName, (Vector3)newValue);
					else if (t.Equals(typeof(int)))
						CmdUpdateObjectPropertyInt(networkObject.UID, networkObject.NetID, networkObject.PrefabName, functionName, (int)newValue);
					else if (t.Equals(typeof(long)))
						CmdUpdateObjectPropertyLong(networkObject.UID, networkObject.NetID, networkObject.PrefabName, functionName, (long)newValue);
					else if (t.Equals(typeof(float)))
						CmdUpdateObjectPropertyFloat(networkObject.UID, networkObject.NetID, networkObject.PrefabName, functionName, (float)newValue);
					else if (t.Equals(typeof(double)))
						CmdUpdateObjectPropertyDouble(networkObject.UID, networkObject.NetID, networkObject.PrefabName, functionName, (double)newValue);
					else if (t.Equals(typeof(string)))
						CmdUpdateObjectPropertyString(networkObject.UID, networkObject.NetID, networkObject.PrefabName, functionName, (string)newValue);
				}
				if (_nameEvent == NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_MESSAGE)
				{
					CmdMessageFromClientsToServer((int)this.netId.Value, (string)_list[0]);
				}
				if (_nameEvent == NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_RPC_MESSAGE)
				{
					RpcMessageFromServerToClients((int)_list[0], (int)_list[1], (string)_list[2]);
				}
				if (_nameEvent == NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_TEXTURE)
				{
					CmdTextureFromClientsToServer((int)this.netId.Value, (int)_list[0], (int)_list[1], (byte[])_list[2]);
				}
			}
		}
	}
}