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
	 * WorldObjectController
	 * 
	 * Manager of the network objects of the world
	 * 
	 * @author Esteban Gallardo
	 */
	public class WorldObjectController : MonoBehaviour, INetworkResources
	{
		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------
		public GameObject[] WorldObjects;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		protected List<GameObject> m_objects = new List<GameObject>();
		protected int m_currentMaximumObjects = 0;

		protected List<GameObject> m_types = new List<GameObject>();
		protected int m_currentMaximumTypes = 0;

		protected bool m_requestedDestroyEverything = false;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------
		public GameObject[] AppWorldObjects
		{
			get { return WorldObjects; }
			set { WorldObjects = value; }
		}

		// -------------------------------------------
		/* 
		* Initialitzation
		*/
		public virtual void Start()
		{
			this.name = this.GetType().Name;
			NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
		}

		// -------------------------------------------
		/* 
		* Initialitzation
		*/
		public virtual void Initialitzation()
		{
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public virtual void Destroy()
		{
			NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
		}

		// -------------------------------------------
		/* 
		* Get the maximum unique identificator to assign to the new cube
		*/
		private int GetMaximumUniqueIdentificatorObjects()
		{
			int finalUID = 0;
			CleanNULLObjects();
			for (int i = 0; i < m_objects.Count; i++)
			{
				INetworkObject obj = m_objects[i].GetComponent<INetworkObject>();
				if (finalUID <= obj.UID)
				{
					finalUID = obj.UID + 1;
				}
			}

			if (finalUID <= m_currentMaximumObjects)
			{
				finalUID = m_currentMaximumObjects + 1;
			}

			m_currentMaximumObjects = finalUID;

			return finalUID;
		}

		// -------------------------------------------
		/* 
		* Get the maximum unique identificator to assign to the new cube
		*/
		private int GetMaximumUniqueIdentificatorTypes()
		{
			int finalUID = 0;
			CleanNULLObjects();
			for (int i = 0; i < m_types.Count; i++)
			{
				INetworkObject type = m_types[i].GetComponent<INetworkObject>();
				if (finalUID <= type.UID)
				{
					finalUID = type.UID + 1;
				}
			}

			if (finalUID <= m_currentMaximumTypes)
			{
				finalUID = m_currentMaximumTypes + 1;
			}

			m_currentMaximumTypes = finalUID;

			return finalUID;
		}

		// -------------------------------------------
		/* 
		* Get the the object in the list of objects created by UID
		*/
		protected GameObject GetInstanceByUID(string _uidName)
		{
			GameObject output = GetObjectByUID(_uidName);
			if (output == null)
			{
				output = GetTypeByUID(_uidName);
			}
			return output;
		}

		// -------------------------------------------
		/* 
		* Get the the object in the list of objects created by UID
		*/
		protected GameObject GetObjectByUID(string _uidName)
		{
			for (int i = 0; i < m_objects.Count; i++)
			{
				INetworkObject obj = m_objects[i].GetComponent<INetworkObject>();
				if (PlayerConnectionController.GetNameIdentificator(obj.PrefabName, obj.UID, obj.NetID) == _uidName)
				{
					return m_objects[i];
				}
			}
			return null;
		}

		// -------------------------------------------
		/* 
		* Get the the object in the list of objects created by UID
		*/
		protected GameObject GetTypeByUID(string _uidName)
		{
			for (int i = 0; i < m_types.Count; i++)
			{
				INetworkObject obj = m_types[i].GetComponent<INetworkObject>();
				if (PlayerConnectionController.GetNameIdentificator(obj.PrefabName, obj.UID, obj.NetID) == _uidName)
				{
					return m_types[i];
				}
			}
			return null;
		}

		// -------------------------------------------
		/* 
		* Get the the object in the list of objects created by assignedName
		*/
		protected GameObject GetInstanceByAssignedName(string _assignedName)
		{
			CleanNULLObjects();
			GameObject output = GetObjectByAssignedName(_assignedName);
			if (output == null)
			{
				output = GetTypeByAssignedName(_assignedName);
			}
			return output;
		}

		// -------------------------------------------
		/* 
		* Get the the object in the list of objects created by assignedName
		*/
		protected GameObject GetObjectByAssignedName(string _assignedName)
		{
			for (int i = 0; i < m_objects.Count; i++)
			{
				INetworkObject obj = m_objects[i].GetComponent<INetworkObject>();
				if (obj.AssignedName == _assignedName)
				{
					return m_objects[i];
				}
			}
			return null;
		}

		// -------------------------------------------
		/* 
		* Get the the object in the list of types created by assignedName
		*/
		protected GameObject GetTypeByAssignedName(string _assignedName)
		{
			for (int i = 0; i < m_types.Count; i++)
			{
				INetworkObject obj = m_types[i].GetComponent<INetworkObject>();
				if (obj.AssignedName == _assignedName)
				{
					return m_types[i];
				}
			}
			return null;
		}

		// -------------------------------------------
		/* 
		* Clean the list from null objects
		*/
		protected int CleanNULLObjects()
		{
			int nullObjects = 0;
			for (int i = 0; i < m_objects.Count; i++)
			{
				if (m_objects[i] == null)
				{
					m_objects.RemoveAt(i);
					i--;
					nullObjects++;
				}
			}

			for (int i = 0; i < m_types.Count; i++)
			{
				if (m_types[i] == null)
				{
					m_types.RemoveAt(i);
					i--;
					nullObjects++;
				}
			}
			return nullObjects;
		}

		// -------------------------------------------
		/* 
		* Manager of global events
		*/
		protected virtual void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_INITIALITZATION_LOCAL_COMPLETED)
			{
				Initialitzation();
			}
			if (_nameEvent == NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_REQUEST_TO_CREATE_NETWORK_OBJECT)
			{
				string nameClassResources = (string)_list[0];
				string typeObjects = (string)_list[1];
				string prefabName = (string)_list[2];
				Vector3 positionCreate = (Vector3)_list[3];
				string assignedName = (string)_list[4];
				bool allowServerChange = (bool)_list[5];
				bool allowClientChange = (bool)_list[6];

				int newUniqueIdentificator = -1;
				switch (typeObjects)
				{
					case NetworkEventController.REGISTER_PREFABS_OBJECTS:
						newUniqueIdentificator = GetMaximumUniqueIdentificatorObjects();
						break;
				}

				GameObject instanceExisting = GetInstanceByAssignedName(assignedName);
				if (instanceExisting == null)
				{
					NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_CREATE_NETWORK_OBJECT, this.GetType().Name, typeObjects, prefabName, newUniqueIdentificator, positionCreate, false, assignedName, allowServerChange, allowClientChange);
				}
				else
				{
					Debug.LogError("WorldObjectController::ERROR::THE ASSIGNED NAME NETWORK VARIABLE EXISTS[" + assignedName + "]");
				}
			}
			if (_nameEvent == NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_CREATION_CONFIRMATION_NETWORK_OBJECT)
			{
				GameObject reference = (GameObject)_list[0];
				INetworkObject objData = reference.GetComponent<INetworkObject>();
				reference.name = PlayerConnectionController.GetNameIdentificator(objData.PrefabName, objData.UID, objData.NetID);
				if (GetInstanceByAssignedName(objData.AssignedName) == null)
				{
					switch (objData.TypeObject)
					{
						case NetworkEventController.REGISTER_PREFABS_OBJECTS:
							Debug.Log("CONFIRMATION NETWORK **OBJECT** CREATED[" + reference.name + "] ASSIGNED NAME[" + objData.AssignedName + "] FROM CLIENT [" + objData.NetID + "]+++++++++++++++++++++++++++++++++++++++++++++++++++++");
							m_objects.Add(reference);
							break;
					}
				}
				else
				{
					Debug.LogError("WorldObjectController::ERROR::ASSIGNED NAME ALREADY USED!!!!!!!!!!!!!!!!!!!!!!!!!!");
				}
			}
			if (_nameEvent == NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_DESTROY_NETWORK_OBJECT)
			{
				string nameToDestroy = (string)_list[0];
				GameObject objectToDestroy = GetInstanceByUID(nameToDestroy);
				if (objectToDestroy != null)
				{
					if (objectToDestroy.GetComponent<INetworkObject>() != null)
					{
						string assignedName = objectToDestroy.GetComponent<INetworkObject>().AssignedName;
						switch (objectToDestroy.GetComponent<INetworkObject>().TypeObject)
						{
							case NetworkEventController.REGISTER_PREFABS_OBJECTS:
								if (m_objects.Remove(objectToDestroy))
								{
									Debug.Log("WorldObjectController::CONFIRMATION NETWORK **OBJECT** DESTROYED[" + nameToDestroy + "]---------------------------------------");
									GameObject.Destroy(objectToDestroy);
									objectToDestroy = null;
								}
								break;
						}
						NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_WORLDOBJECTCONTROLLER_DESTROY_CONFIRMATION, assignedName);
					}
				}
			}
			if (_nameEvent == NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_DESTROY_CONFIRMATION_NETWORK_OBJECT)
			{
				int cleaned = CleanNULLObjects();
				Debug.Log("WorldObjectController::TOTAL NETWORK OBJECTS DELETED[" + cleaned + "]!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
			}
			if (_nameEvent == NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_REGISTER_ALL_NETWORK_PREFABS)
			{
				// REGISTER ALL PREFABS FOR NETWORK
				for (int i = 0; i < WorldObjects.Length; i++)
				{
					NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_REGISTER_PREFAB, this.GetType().Name, NetworkEventController.REGISTER_PREFABS_OBJECTS, WorldObjects[i].name);
				}
			}
			if (_nameEvent == YourNetworkTools.EVENT_YOURNETWORKTOOLS_DESTROYED_GAMEOBJECT)
			{
				GameObject destroyedObject = (GameObject)_list[0];
				for (int i = 0; i < m_objects.Count; i++)
				{
					if (m_objects[i] == destroyedObject)
					{
						m_objects.RemoveAt(i);
						return;
					}
				}
			}
		}
	}
}
