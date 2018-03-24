using UnityEngine;

namespace YourNetworkingTools
{

	/// <summary>
	/// This script exists as a stub to allow other scripts to find 
	/// the shared world anchor transform.
	/// </summary>
	public class SharedCollection : MonoBehaviour
	{
		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static SharedCollection instance;

		public static SharedCollection Instance
		{
			get
			{
				if (!instance)
				{
					instance = GameObject.FindObjectOfType(typeof(SharedCollection)) as SharedCollection;
				}
				return instance;
			}
		}
	}
}