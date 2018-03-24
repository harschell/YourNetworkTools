using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourNetworkingTools
{

	public class CreateNewRoomHTTP : BaseDataHTTP, IHTTPComms
	{
		public const string EVENT_CLIENT_HTTP_NEW_ROOM_CREATED = "EVENT_CLIENT_HTTP_NEW_ROOM_CREATED";

		private string m_urlRequest = URL_BASE + "CreateNewRoomHTTP.php";

		public string UrlRequest
		{
			get { return m_urlRequest; }
		}

		public string Build(params object[] _list)
		{
			return "?lobby=" + (string)_list[0] + "&room=" + (string)_list[1] + "&players=" + (string)_list[2] + "&extra=" + (string)_list[3];
		}

		public override void Response(string _response)
		{
			if (!ResponseCode(_response))
			{
				MenuEventController.Instance.DispatchMenuEvent(EVENT_CLIENT_HTTP_NEW_ROOM_CREATED);
				return;
			}

			string[] data = m_jsonResponse.Split(new string[] { HTTPController.TOKEN_SEPARATOR_EVENTS }, StringSplitOptions.None);
			if (bool.Parse(data[0]))
			{
				int room = int.Parse(data[1]);
				string ip = (string)data[2];
				int port = int.Parse(data[3]);
				int machineID = int.Parse(data[4]);
				MenuEventController.Instance.DispatchMenuEvent(EVENT_CLIENT_HTTP_NEW_ROOM_CREATED, room, ip, port, machineID);
			}
			else
			{
				MenuEventController.Instance.DispatchMenuEvent(EVENT_CLIENT_HTTP_NEW_ROOM_CREATED);
			}
		}
	}
}