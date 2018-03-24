using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * AppEventData
	 * 
	 * Class used to dispatch events with a certain delay in time
	 * 
	 * @author Esteban Gallardo
	 */
	public class AppEventData
	{
		public const int CONFIGURATION_INTERNAL_EVENT = 0;
		public const int CONFIGURATION_SERVER_EVENT = 1;
		public const int CONFIGURATION_CLIENT_EVENT = 2;
		public const int CONFIGURATION_SERVER_AND_CLIENT_EVENT = 3;

		private string nameEvent;
		private int configuration;
		private bool isLocalEvent;
		private float time;
		private int networkID;
		private object[] listParameters;

		public string NameEvent
		{
			get { return nameEvent; }
		}
		public int Configuration
		{
			get { return configuration; }
		}
		public bool IsLocalEvent
		{
			get { return isLocalEvent; }
		}
		public float Time
		{
			get { return time; }
			set { time = value; }
		}
		public int NetworkID
		{
			get { return networkID; }
			set { networkID = value; }
		}
		public object[] ListParameters
		{
			get { return listParameters; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public AppEventData(string _nameEvent, int _configuration, bool _isLocalEvent, int _networkID, float _time, params object[] _list)
		{
			nameEvent = _nameEvent;
			configuration = _configuration;
			isLocalEvent = _isLocalEvent;
			networkID = _networkID;
			time = _time;
			listParameters = _list;
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			listParameters = null;
		}

		// -------------------------------------------
		/* 
		 * ToStringParameters
		 */
		public string ToStringParameters()
		{
			string parameters = "";
			if (listParameters != null)
			{
				for (int i = 0; i < listParameters.Length; i++)
				{
					if (UtilitiesNetwork.IsNumber(listParameters[i]))
					{
						parameters += (float)UtilitiesNetwork.GetDouble(listParameters[i]);
					}
					else
					{
						if (listParameters[i] is string)
						{
							parameters += (string)listParameters[i];
						}
						else
						{
							parameters += listParameters[i].ToString();
						}
					}
					if (i + 1 < listParameters.Length)
					{
						parameters += ",";
					}
				}
			}
			return parameters;
		}

		// -------------------------------------------
		/* 
		 * ToString
		 */
		public override string ToString()
		{
			string parameters = ToStringParameters();
			if (parameters.Length > 0)
			{
				parameters = "," + parameters;
			}

			return nameEvent + parameters;
		}

		// -------------------------------------------
		/* 
		 * IsInternalEvent
		 */
		public bool IsInternalEvent()
		{
			return (configuration == CONFIGURATION_INTERNAL_EVENT);
		}
	}
}