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
	 * ClientInstalledApps
	 *    
	 * @author Esteban Gallardo
	 */
	public class ClientInstalledApps
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_FIRSTCONNECTION_REGISTER_INSTALLED_APPS_IN_SERVER = "EVENT_FIRSTCONNECTION_REGISTER_INSTALLED_APPS_IN_SERVER";
		public const string EVENT_CLIENT_INSTALLED_APPS_GET_LIST_APPS = "EVENT_CLIENT_INSTALLED_APPS_GET_LIST_APPS";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string TOKEN_PARAMETER_SEPARATOR = "<ap>";
		public const string TOKEN_LINE_SEPARATOR = "<al>";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private bool m_hasRegisterListener = false;
		protected List<string> m_installedApps = new List<string>();

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
		}

		// -------------------------------------------
		/* 
		 * Will register a new app of the application
		 */
		public void RegisterEvent(string _appName)
		{
			m_installedApps.Add(_appName);
		}

		// -------------------------------------------
		/* 
		 * Will get a package string with the installed app list
		 */
		private string PackData()
		{
			string output = "";

			for (int i = 0; i < m_installedApps.Count; i++)
			{
				string installedApp = m_installedApps[i];
				output += installedApp;
				if (i + 1 < m_installedApps.Count)
				{
					output += TOKEN_LINE_SEPARATOR;
				}
			}
			return output;
		}

		// -------------------------------------------
		/* 
		 * Unpack the register event package
		 */
		public void UnPackInstalledApps(string _data)
		{
			m_installedApps = UnPackData(_data);
		}

		// -------------------------------------------
		/* 
		 * Unpack the register event package
		 */
		public static List<string> UnPackData(string _data)
		{
			List<string> output = new List<string>();
			string[] events = _data.Split(new string[] { TOKEN_LINE_SEPARATOR }, StringSplitOptions.None);

			for (int i = 0; i < events.Length; i++)
			{
				output.Add(events[i]);
			}

			Debug.Log("ClientInstalledApps::UnPackData::TOTAL APPS INSTALLED[" + output.Count + "]");
			for (int i = 0; i < output.Count; i++)
			{
				Debug.Log(output[i].ToString());
			}

			return output;
		}

		// -------------------------------------------
		/* 
		 * It will sent the package with the list of installed apps
		 */
		public void SendInstalledAppsToServer()
		{
			if (!m_hasRegisterListener)
			{
				NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
			}
			else
			{
				return;
			}
			m_hasRegisterListener = true;

			// FAKE RESPONSE        
			NetworkEventController.Instance.DispatchLocalEvent(EVENT_CLIENT_INSTALLED_APPS_GET_LIST_APPS, "Paint,Word,Visual Studio");
			// HTTPComms.Instance.Request("http://localhost/api/app/packagemanager/packages", "", EVENT_CLIENT_INSTALLED_APPS_GET_LIST_APPS, HTTPComms.METHOD_GET);
		}

		// -------------------------------------------
		/* 
		* Manager of network events
		*/
		protected virtual void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (_nameEvent == EVENT_CLIENT_INSTALLED_APPS_GET_LIST_APPS)
			{
				string[] dataAppNames = ((string)_list[0]).Split(',');
				for (int i = 0; i < dataAppNames.Length; i++)
				{
					m_installedApps.Add(dataAppNames[i]);
				}
				NetworkEventController.Instance.DispatchNetworkEvent(EVENT_FIRSTCONNECTION_REGISTER_INSTALLED_APPS_IN_SERVER, PackData());
			}
		}

	}
}