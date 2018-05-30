using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using YourCommonTools;

namespace YourNetworkingTools
{
	public class GetListRoomsHTTP : BaseDataHTTP, IHTTPComms
	{
		public const char TOKEN_SEPARATOR_PARAMETER = ',';
		public const char TOKEN_SEPARATOR_ROOM = ';';

		public const string EVENT_CLIENT_HTTP_LIST_OF_GAME_ROOMS = "EVENT_CLIENT_HTTP_LIST_OF_GAME_ROOMS";

		private string m_urlRequest = "https://www.yourvrexperience.com/yournetworkingtools/GetListRoomsHTTP.php";

		public string UrlRequest
		{
			get { return m_urlRequest; }
		}

		public string Build(params object[] _list)
		{
			return "?lobby=" + (string)_list[0] + "&userid=" + (string)_list[1];
		}

		public override void Response(string _response)
		{
			if (!ResponseCode(_response))
			{
				UIEventController.Instance.DispatchUIEvent(EVENT_CLIENT_HTTP_LIST_OF_GAME_ROOMS);
				return;
			}

			List<ItemMultiTextEntry> roomsAvailable = new List<ItemMultiTextEntry>();
			string[] rooms = _response.Split(new string[] { CommController.TOKEN_SEPARATOR_LINES }, StringSplitOptions.None);
			for (int i = 0; i < rooms.Length; i++)
			{
				string[] room = rooms[i].Split(new string[] { CommController.TOKEN_SEPARATOR_EVENTS }, StringSplitOptions.None);
				if (room.Length == 5)
				{
					string idRoom = room[0];
					string nameRoom = room[1];
					string ipMachine = room[2];
					string portMachine = room[3];
					string extraData = room[4];
					roomsAvailable.Add(new ItemMultiTextEntry(idRoom, nameRoom, ipMachine, portMachine, extraData));
				}
			}
			UIEventController.Instance.DispatchUIEvent(EVENT_CLIENT_HTTP_LIST_OF_GAME_ROOMS, roomsAvailable);
		}
	}
}