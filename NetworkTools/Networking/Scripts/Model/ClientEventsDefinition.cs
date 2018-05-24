using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine.SceneManagement;
using YourCommonTools;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * ClientEventsDefinition
	 *    
	 * @author Esteban Gallardo
	 */
	public class ClientEventsDefinition
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_FIRSTCONNECTION_REGISTER_EVENTS_IN_SERVER = "EVENT_FIRSTCONNECTION_REGISTER_EVENTS_IN_SERVER";

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const string PARAMETER_INT = "int";
		public const string PARAMETER_FLOAT = "float";
		public const string PARAMETER_DOUBLE = "double";
		public const string PARAMETER_STRING = "string";
		public const string PARAMETER_VECTOR3 = "vector3";

		public const string TOKEN_PARAMETER_SEPARATOR = "<ep>";
		public const string TOKEN_LINE_SEPARATOR = "<el>";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		protected List<AppEventData> m_events = new List<AppEventData>();

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public List<AppEventData> Events
		{
			get { return m_events; }
		}

		// -------------------------------------------
		/* 
		 * Will register a new event of the application a delayed network event (if "_isLocalEvent==false") 
		 */
		public void RegisterEvent(string _nameEvent, int _configuration, params string[] _list)
		{
			m_events.Add(new AppEventData(_nameEvent, _configuration, true, -1, -1, _list));
		}

		// -------------------------------------------
		/* 
		 * Will check if the event is in the list of allowed events for the client
		 */
		public AppEventData CheckValidEvent(string _nameEvent, params object[] _list)
		{
			for (int i = 0; i < m_events.Count; i++)
			{
				AppEventData sEvent = m_events[i];
				if (sEvent.NameEvent == _nameEvent)
				{
					for (int j = 0; j < _list.Length; j++)
					{
						switch ((string)sEvent.ListParameters[j])
						{
							case PARAMETER_INT:
								if (!Utilities.IsStringInteger((string)_list[j]))
								{
									return null;
								}
								break;

							case PARAMETER_FLOAT:
								if (!Utilities.IsStringFloat((string)_list[j]))
								{
									return null;
								}
								break;

							case PARAMETER_DOUBLE:
								if (!Utilities.IsStringDouble((string)_list[j]))
								{
									return null;
								}
								break;

							case PARAMETER_VECTOR3:
								if (!Utilities.IsStringVector3((string)_list[j]))
								{
									return null;
								}
								break;

							case PARAMETER_STRING:
								break;
						}
					}
					return sEvent;
				}
			}

			return null;
		}

		// -------------------------------------------
		/* 
		* Will get a package string with the event definition
		*/
		private string PackData()
		{
			string output = "";

			for (int i = 0; i < m_events.Count; i++)
			{
				AppEventData sEvent = m_events[i];
				output += sEvent.NameEvent;
				output += TOKEN_PARAMETER_SEPARATOR;
				output += sEvent.Configuration;
				if (sEvent.ListParameters.Length > 0)
				{
					output += TOKEN_PARAMETER_SEPARATOR;
					for (int j = 0; j < sEvent.ListParameters.Length; j++)
					{
						output += sEvent.ListParameters[j];
						if (j + 1 < sEvent.ListParameters.Length)
						{
							output += TOKEN_PARAMETER_SEPARATOR;
						}
					}
				}
				if (i + 1 < m_events.Count)
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
		public void UnPackEvents(string _data)
		{
			m_events = UnPackData(_data);
		}

		// -------------------------------------------
		/* 
		 * Unpack the register event package
		 */
		public static List<AppEventData> UnPackData(string _data)
		{
			List<AppEventData> output = new List<AppEventData>();
			string[] events = _data.Split(new string[] { TOKEN_LINE_SEPARATOR }, StringSplitOptions.None);

			for (int i = 0; i < events.Length; i++)
			{
				string[] eventData = events[i].Split(new string[] { TOKEN_PARAMETER_SEPARATOR }, StringSplitOptions.None);
				string nameEvent = eventData[0];
				int configuration = int.Parse(eventData[1]);
				string[] parametersEvent = null;
				if (eventData.Length > 2)
				{
					parametersEvent = new string[eventData.Length - 2];
					for (int j = 2; j < eventData.Length; j++)
					{
						parametersEvent[j - 2] = eventData[j];
					}
				}
				if (parametersEvent == null)
				{
					output.Add(new AppEventData(nameEvent, configuration, true, -1, -1));
				}
				else
				{
					output.Add(new AppEventData(nameEvent, configuration, true, -1, -1, parametersEvent));
				}
			}

			Debug.Log("ClientEventsDefinition::UnPackData::TOTAL EVENTS[" + output.Count + "]");
			for (int i = 0; i < output.Count; i++)
			{
				Debug.Log(output[i].ToString());
			}

			return output;
		}

		// -------------------------------------------
		/* 
		 * It will sent the package of events to the server in order for it to register
		 */
		public void SendEventDefinitionToServer()
		{
			NetworkEventController.Instance.DispatchNetworkEvent(EVENT_FIRSTCONNECTION_REGISTER_EVENTS_IN_SERVER, PackData());
		}
	}
}