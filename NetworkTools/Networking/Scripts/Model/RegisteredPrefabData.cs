using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * RegisteredPrefabData
	 * 
	 * Keeps the information of the registered prefab
	 * 
	 * @author Esteban Gallardo
	 */
	public class RegisteredPrefabData
	{
		// -----------------------------------------
		// PRIVATE VARIABLES
		// -----------------------------------------
		private GameObject m_prefab;
		private string m_classRegisteredResources;
		private string m_typeObjects;
		private string m_prefabName;

		// -----------------------------------------
		// GETTERS/SETTERS
		// -----------------------------------------
		public GameObject Prefab
		{
			get { return m_prefab; }
		}
		public string ClassRegisteredResources
		{
			get { return m_classRegisteredResources; }
		}
		public string TypeObjects
		{
			get { return m_typeObjects; }
		}
		public string PrefabName
		{
			get { return m_prefabName; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public RegisteredPrefabData(GameObject _prefab, string _classNetworkResources, string _typeObjects, string _prefabName)
		{
			m_prefab = _prefab;
			m_classRegisteredResources = _classNetworkResources;
			m_typeObjects = _typeObjects;
			m_prefabName = _prefabName;
		}

	}
}