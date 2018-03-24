using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourNetworkingTools
{
	/******************************************
	* 
	* NetworkInteger
	* 
	* An integer variable shared by all connected clients
	* 
	* @author Esteban Gallardo
	*/
	public class NetworkInteger : NetworkVariable, INetworkVariable
	{
		private int m_value = -1;

		// -------------------------------------------
		/* 
		* Get the value of the variable
		*/
		public override object GetValue()
		{
			return m_value;
		}

		// -------------------------------------------
		/* 
		* Set the value of the variable
		*/
		public override void SetValue(object _value)
		{
			try
			{
				if (m_value != (int)_value)
				{
					UpdateValue((int)_value);
					NetworkEventController.Instance.DispatchNetworkEvent(NetworkEventController.EVENT_SYSTEM_VARIABLE_SET, m_name, m_value.ToString());
				}
			}
			catch (Exception err)
			{
				Debug.LogError("NetworkInteger::SetValue::TYPE MISTMATCH::err=" + err.Message);
			}
		}

		// -------------------------------------------
		/* 
		* Update the local value
		*/
		public override void UpdateValue(object _value)
		{
			try
			{
				int finalValue = -999999999;
				if (_value is String)
				{
					if (!int.TryParse((string)_value, out finalValue))
					{
						finalValue = -999999999;
					}
				}
				if (_value is int)
				{
					finalValue = (int)_value;
				}

				if (finalValue != -999999999)
				{
					if (m_value != finalValue)
					{
						m_value = finalValue;
						DispatchNetworkVariableEvent(EVENT_NETWORKVARIABLE_UPDATED);
					}
				}
			}
			catch (Exception err)
			{
				Debug.LogError("NetworkInteger::UpdateValue::TYPE MISTMATCH::err=" + err.Message);
			}
		}

		// -------------------------------------------
		/* 
		* Get the type of the variable
		*/
		public override Type GetTypeValue()
		{
			return Type.GetType("System.Int32");
		}

		// -------------------------------------------
		/* 
		* Get the type of the variable
		*/
		public override string GetTypeValueInString()
		{
			return GetTypeValue().ToString();
		}

		// -------------------------------------------
		/* 
		* Get a description text of the variable
		*/
		public override string GetInformation()
		{
			return m_name + ":" + m_value.ToString();
		}

		// -------------------------------------------
		/* 
		* Get the type of the variable
		*/
		public static Type GetTypeInteger()
		{
			return Type.GetType("System.Int32");
		}

		// -------------------------------------------
		/* 
		* Apply a random increase, used for testing
		*/
		public override void RandomIncrease(int _value)
		{
			SetValue((int)GetValue() + _value);
		}
	}
}
