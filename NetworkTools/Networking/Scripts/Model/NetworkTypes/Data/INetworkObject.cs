
/******************************************
 * 
 * INetworkObject
 * 
 * The interface for the class that define a network object
 * 
 * @author Esteban Gallardo
 */
using UnityEngine;

namespace YourNetworkingTools
{

	public interface INetworkObject
	{
		// VARIABLE
		int UID { get; set; }
		int NetID { get; set; }
		string PrefabName { get; set; }
		string TypeObject { get; set; }
		bool PreserveTransform { get; set; }
		string AssignedName { get; set; }
		bool AllowServerChange { get; set; }
		bool AllowClientChange { get; set; }

		// FUNCTIONS
		string GetInformation();
		void InvokeFunction(string _nameFunction, object _value);
		bool IsLocalPlayer();
	}
}