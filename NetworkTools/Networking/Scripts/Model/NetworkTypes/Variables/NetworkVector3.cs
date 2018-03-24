using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourNetworkingTools
{
	/******************************************
	* 
	* NetworkVector3
	* 
	* An vector3 variable shared by all connected clients
	* 
	* @author Esteban Gallardo
	*/
	public class NetworkVector3 : NetworkVariable, INetworkVariable
	{
		public const char TOKEN_SEPARATOR = ',';

		private Vector3 m_value = Vector3.zero;

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
				if (m_value != (Vector3)_value)
				{
					UpdateValue((Vector3)_value);
					NetworkEventController.Instance.DispatchNetworkEvent(NetworkEventController.EVENT_SYSTEM_VARIABLE_SET, m_name, m_value.x.ToString() + TOKEN_SEPARATOR + m_value.y.ToString() + TOKEN_SEPARATOR + m_value.z.ToString());
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
				Vector3 finalValue = Vector3.zero;
				if (_value is String)
				{
					float valueEvaluated = -1;
					string[] valuesVector = ((string)_value).Split(TOKEN_SEPARATOR);
					if (float.TryParse(valuesVector[0], out valueEvaluated))
					{
						finalValue.x = valueEvaluated;
					}
					if (float.TryParse(valuesVector[1], out valueEvaluated))
					{
						finalValue.y = valueEvaluated;
					}
					if (float.TryParse(valuesVector[2], out valueEvaluated))
					{
						finalValue.z = valueEvaluated;
					}
				}
				if (_value is Vector3)
				{
					finalValue = (Vector3)_value;
				}

				if (finalValue != Vector3.zero)
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
			throw new Exception("NetworkVector3::GetTypeValue::DON'T WORK WITH System.GetType");
		}

		// -------------------------------------------
		/* 
		* Get the type of the variable
		*/
		public override string GetTypeValueInString()
		{
			return "UnityEngine.Vector3";
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
		public static string GetTypeVector3()
		{
			return "UnityEngine.Vector3";
		}

		// -------------------------------------------
		/* 
		* Apply a random increase, used for testing
		*/
		public override void RandomIncrease(int _value)
		{
			SetValue((Vector3)GetValue() + (((_value > 0) ? 1 : -1) * Vector3.one));
		}

	}
}
