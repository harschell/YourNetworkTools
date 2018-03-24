namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * NetworkID
	 * 
	 * Unique identificator for network
	 * 
	 * @author Esteban Gallardo
	 */
	using UnityEngine;

	public class NetworkID : MonoBehaviour
	{
		// VARIABLE
		public int UID = -1;
		public int NetID = -1;
		public int IndexPrefab = -1;

		public bool CheckID(int _netID, int _uID)
		{
			return ((NetID == _netID) && (UID == _uID));
		}

		public bool CheckID(string _netID, string _uID)
		{
			return ((NetID == int.Parse(_netID)) && (UID == int.Parse(_uID)));
		}

		public bool CheckID(string _id)
		{
			string[] data = _id.Split(',');
			return ((NetID == int.Parse(data[0])) && (UID == int.Parse(data[1])));
		}

		public string GetID()
		{
			return NetID + "," + UID;
		}
	}
}