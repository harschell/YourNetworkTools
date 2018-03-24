using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourNetworkingTools
{
	/******************************************
	* 
	* NetworkString
	* 
	* A string variable shared by all connected clients
	* 
	* @author Esteban Gallardo
	*/
	public class NetworkString : NetworkVariable, INetworkVariable
	{
		private string m_value;

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
				UpdateValue((string)_value);
				NetworkEventController.Instance.DispatchNetworkEvent(NetworkEventController.EVENT_SYSTEM_VARIABLE_SET, m_name, m_value.ToString());
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
				m_value = (string)_value;
				DispatchNetworkVariableEvent(EVENT_NETWORKVARIABLE_UPDATED);
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
			return Type.GetType("System.String");
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
			return m_name + ":" + m_value;
		}

		// -------------------------------------------
		/* 
		* Get the type of the variable
		*/
		public static Type GetTypeString()
		{
			return Type.GetType("System.String");
		}

		// -------------------------------------------
		/* 
		* Apply a random increase, used for testing
		*/
		public override void RandomIncrease(int _value)
		{
			string originalValue = (string)GetValue();
			if (_value < 0)
			{
				if (originalValue.Length > 2)
				{
					originalValue = originalValue.Substring(0, originalValue.Length - 1);
				}
			}
			else
			{
				originalValue = originalValue + "!";
			}
			SetValue(originalValue);
		}
	}
}
