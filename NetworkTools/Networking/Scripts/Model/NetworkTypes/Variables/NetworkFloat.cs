using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourNetworkingTools
{
	/******************************************
	* 
	* NetworkFloat
	* 
	* An float variable shared by all connected clients
	* 
	* @author Esteban Gallardo
	*/
	public class NetworkFloat : NetworkVariable, INetworkVariable
	{
		private float m_value = -1;

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
				if (m_value != (float)_value)
				{
					UpdateValue((float)_value);
					NetworkEventController.Instance.DispatchNetworkEvent(NetworkEventController.EVENT_SYSTEM_VARIABLE_SET, m_name, m_value.ToString());
				}
			}
			catch (Exception err)
			{
				Debug.LogError("NetworkFloat::SetValue::TYPE MISTMATCH::err=" + err.Message);
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
				float finalValue = -999999999;
				if (_value is String)
				{
					if (!float.TryParse((string)_value, out finalValue))
					{
						finalValue = -999999999;
					}
				}
				if (_value is float)
				{
					finalValue = (float)_value;
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
				Debug.LogError("NetworkFloat::UpdateValue::TYPE MISTMATCH::err=" + err.Message);
			}
		}

		// -------------------------------------------
		/* 
		* Get the type of the variable
		*/
		public override Type GetTypeValue()
		{
			return Type.GetType("System.Double");
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
		public static Type GetTypeFloat()
		{
			return Type.GetType("System.Double");
		}

		// -------------------------------------------
		/* 
		* Apply a random increase, used for testing
		*/
		public override void RandomIncrease(int _value)
		{
			SetValue((float)GetValue() + (((float)_value) / 2));
		}
	}
}
