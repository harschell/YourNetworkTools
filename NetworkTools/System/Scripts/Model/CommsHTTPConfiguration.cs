using YourCommonTools;

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * CommsHTTPConfiguration
	 * 
	 * Functionallity of HTTP comms
	 * 
	 * @author Esteban Gallardo
	 */
	public static class CommsHTTPConfiguration
	{
		// ----------------------------------------------
		// COMM EVENTS
		// ----------------------------------------------	
		public const string EVENT_COMM_CREATE_NEW_ROOM = "YourNetworkingTools.CreateNewRoomHTTP";
		public const string EVENT_COMM_GET_LIST_ROOMS = "YourNetworkingTools.GetListRoomsHTTP";

		// -------------------------------------------
		/* 
		 * DisplayLog
		 */
		public static void DisplayLog(string _data)
		{
			CommController.Instance.DisplayLog(_data);
		}

		// -------------------------------------------
		/* 
		 * CreateNewRoom
		 */
		public static void CreateNewRoom(bool _isLobby, string _nameRoom, string _players, string _extraData)
		{
			CommController.Instance.Request(EVENT_COMM_CREATE_NEW_ROOM, false, (_isLobby ? "1" : "0"), _nameRoom, _players, _extraData);
		}

		// -------------------------------------------
		/* 
		 * RequestListRooms
		 */
		public static void GetListRooms(bool _isLobby, string _idUser)
		{
			CommController.Instance.Request(EVENT_COMM_GET_LIST_ROOMS, false, (_isLobby ? "1" : "0"), _idUser);
		}

	}
}

