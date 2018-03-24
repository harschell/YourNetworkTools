using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * INetworkType
	 * 
	 * interface network type
	 * 
	 * @author Esteban Gallardo
	 */
	public interface INetworkType
	{
		// GETTERS/SETTERS
		event NetworkTypeEventHandler NetworkTypeEvent;
		string AssignedName { get; }
		GameObject NetworkObject { get; }

		// FUNCTIONS
		bool AllowModification();
		void Destroy();
		void InitRemoteNetworkObject(GameObject _networkObject);
		void OnNetworkTypeEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list);
		void SetObjectValue(object _value);
		object GetObjectValue();
		void Increase(object _value);
		void Decrease(object _value);
	}
}