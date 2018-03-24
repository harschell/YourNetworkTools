using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * NetworkObjectData
	 * 
	 * Base class of the network object
	 * 
	 * @author Esteban Gallardo
	 */
	[NetworkSettings(sendInterval = 0.033f)]
	[AddComponentMenu("NetworkIdentity")]
	public class NetworkObjectData : NetworkBehaviour, INetworkObject
	{
		// -----------------------------------------
		// SYNCVAR VARIABLES
		// -----------------------------------------
		[SyncVar]
		private int m_uID;              // Unique id in the array

		[SyncVar]
		private int m_netID;            // The unique network identificator of the owner of this object

		[SyncVar]
		private string m_prefabName;    // Prefab name which belongs to

		[SyncVar]
		private string m_typeObject;    // Is a basic primitive or a wold object

		[SyncVar]
		private string m_assignedName;  // The name of the object use by the programmer to identify it

		[SyncVar]
		private bool m_preserveTransform;   // Keeps the reference of the transform

		[SyncVar]
		private bool m_allowServerChange;   // Allows only the server to change the object

		[SyncVar]
		private bool m_allowClientChange;   // Allos the client to change the object


		public string LocalAssignedName = "";

		// -----------------------------------------
		// GETTERS/SETTERS
		// -----------------------------------------
		public int UID
		{
			get { return m_uID; }
			set { m_uID = value; }
		}
		public int NetID
		{
			get { return m_netID; }
			set { m_netID = value; }
		}
		public string PrefabName
		{
			get { return m_prefabName; }
			set { m_prefabName = value; }
		}
		public string TypeObject
		{
			get { return m_typeObject; }
			set { m_typeObject = value; }
		}
		public bool PreserveTransform
		{
			get { return m_preserveTransform; }
			set { m_preserveTransform = value; }
		}
		public bool AllowServerChange
		{
			get { return m_allowServerChange; }
			set { m_allowServerChange = value; }
		}
		public bool AllowClientChange
		{
			get { return m_allowClientChange; }
			set { m_allowClientChange = value; }
		}
		public string AssignedName
		{
			get { return m_assignedName; }
			set
			{
				m_assignedName = value;
				LocalAssignedName = m_assignedName;
			}
		}

		// -------------------------------------------
		/* 
		 * Initialize the name
		 */
		public virtual void Start()
		{
			this.gameObject.name = PlayerConnectionController.GetNameIdentificator(this);
			LocalAssignedName = m_assignedName;
			transform.SetParent(SharedCollection.Instance.transform, m_preserveTransform);
			NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_CREATION_CONFIRMATION_NETWORK_OBJECT, this.gameObject);
		}

		// -------------------------------------------
		/* 
		 * Check the unique identification
		 */
		public bool CheckID(int _netID, int _uid)
		{
			return (NetID == _netID) && (UID == _uid);
		}

		// -------------------------------------------
		/* 
		 * Get string unique identification
		 */
		public string GetID()
		{
			return NetID  + "," + UID;
		}

		// -------------------------------------------
		/* 
		 * Initialize the name
		 */
		void OnDestroy()
		{
			NetworkEventController.Instance.DelayLocalEvent(NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_DESTROY_CONFIRMATION_NETWORK_OBJECT, 0.1f, this.name, m_uID, m_netID, m_prefabName, m_typeObject, m_assignedName);
		}

		// -------------------------------------------
		/* 
		 * Informs if the object belongs to the local player
		 */
		public bool IsLocalPlayer()
		{
			return (CommunicationsController.Instance.NetworkID == NetID);
		}

		// -------------------------------------------
		/* 
		* Update the variable registered
		*/
		public virtual void InvokeFunction(string _nameFunction, object _value)
		{

		}

		// -------------------------------------------
		/* 
		* Will return in a string the important information of this network object
		*/
		public virtual string GetInformation()
		{
			return this.gameObject.name;
		}
	}
}