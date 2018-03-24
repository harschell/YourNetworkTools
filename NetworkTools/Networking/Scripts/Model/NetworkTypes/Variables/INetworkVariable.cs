using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * INetworkVariable
	 * 
	 * Interface network variable type
	 * 
	 * @author Esteban Gallardo
	 */
	public interface INetworkVariable : IEquatable<INetworkVariable>
	{
		// GETTERS/SETTERS
		event NetworkVariableEventHandler NetworkVariableEvent;
		string Name { get; }

		// FUNCTIONS
		void InitRemote(params object[] _list);
		void InitLocal(params object[] _list);
		void Destroy();
		object GetValue();
		void SetValue(object _value);
		void UpdateValue(object _value);
		Type GetTypeValue();
		void RandomIncrease(int _value);
		string GetTypeValueInString();
		string GetInformation();
		void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list);
	}
}