using UnityEngine;

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * INetworkResources
	 * 
	 * Interface of the class that contains the world objects
	 * 
	 * @author Esteban Gallardo
	 */
	public interface INetworkResources
	{
		// VARIABLE
		GameObject[] AppWorldObjects { get; }
	}
}