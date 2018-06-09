using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourNetworkingTools
{
	/******************************************
	* 
	* NetworkQuaternion
	* 
	* An Quaternion variable shared by all connected clients
	* 
	* @author Esteban Gallardo
	*/
	public class NetworkQuaternion : NetworkVariable, INetworkVariable
	{
		public const char TOKEN_SEPARATOR = ',';

		private Quaternion m_value = Quaternion.identity;

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
				if (m_value != (Quaternion)_value)
				{
					UpdateValue((Quaternion)_value);
					NetworkEventController.Instance.DispatchNetworkEvent(NetworkEventController.EVENT_SYSTEM_VARIABLE_SET, m_name, m_value.x.ToString() + TOKEN_SEPARATOR + m_value.y.ToString() + TOKEN_SEPARATOR + m_value.z.ToString() + TOKEN_SEPARATOR + m_value.w.ToString());
				}
			}
			catch (Exception err)
			{
				Debug.LogError("NetworkQuaternion::SetValue::TYPE MISTMATCH::err=" + err.Message);
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
				Quaternion finalValue = Quaternion.identity;
				if (_value is string)
				{
					float valueEvaluated = -1;
					string valueTmp = ((string)_value).TrimStart('(');
					valueTmp = valueTmp.TrimEnd(')');
					string[] valuesVector = valueTmp.Split(TOKEN_SEPARATOR);
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
					if (float.TryParse(valuesVector[3], out valueEvaluated))
					{
						finalValue.w = valueEvaluated;
					}
				}
				if (_value is Quaternion)
				{
					finalValue = (Quaternion)_value;
				}

				if (!finalValue.Equals(Quaternion.identity))
				{
					if (!m_value.Equals(finalValue))
					{
						m_value = finalValue;
						DispatchNetworkVariableEvent(EVENT_NETWORKVARIABLE_UPDATED);
					}
				}
			}
			catch (Exception err)
			{
				Debug.LogError("NetworkQuaternion::UpdateValue::TYPE MISTMATCH::err=" + err.Message);
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
			return "UnityEngine.Quaternion";
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
		public static string GetTypeQuaternion()
		{			
			return "UnityEngine.Quaternion";
		}

		// -------------------------------------------
		/* 
		* Apply a random increase, used for testing
		*/
		public override void RandomIncrease(int _value)
		{
			SetValue((Quaternion)GetValue());
		}

	}
}
