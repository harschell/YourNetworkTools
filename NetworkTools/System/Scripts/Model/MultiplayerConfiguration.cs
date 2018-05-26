using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * MultiplayerConfiguration
	 * 
	 * We keep the global information 
	 * 
	 * @author Esteban Gallardo
	 */
	public static class MultiplayerConfiguration
	{
		public const bool DEBUG_MODE = true;

		public const string NUMBER_OF_PLAYERS_COOCKIE = "NUMBER_OF_PLAYERS_COOCKIE";
		public const int VALUE_FOR_JOINING = -1000;

		public const string SOCKET_SERVER_ADDRESS = "localhost";
		public const int PORT_SERVER_ADDRESS = 8745;

		public const string IP_ADDRESS_COOCKIE = "IP_ADDRESS_COOCKIE";
		public const string PORT_ADDRESS_COOCKIE = "PORT_ADDRESS_COOCKIE";
		public const string ROOM_NUMBER_COOCKIE = "ROOM_NUMBER_COOCKIE";
		public const string MACHINE_ID_HOST_ROOM_COOCKIE = "MACHINE_ID_HOST_ROOM_COOCKIE";

		public const string FACEBOOK_FRIENDS_COOCKIE = "FACEBOOK_FRIENDS_COOCKIE";
		public const string NAME_ROOM_LOOBY_COOCKIE = "NAME_ROOM_LOOBY_COOCKIE";
		public const string IS_ROOM_LOOBY_COOCKIE = "IS_ROOM_LOOBY_COOCKIE";

		public const string CARDBOARD_ENABLE_COOCKIE = "CARDBOARD_ENABLE_COOCKIE";
		public const string EXTRA_DATA_COOCKIE = "EXTRA_DATA_COOCKIE";

		// -------------------------------------------
		/* 
		 * Will save the data for the game scene to load it
		 */
		public static void SaveNumberOfPlayers(int _players)
		{
			PlayerPrefs.SetInt(NUMBER_OF_PLAYERS_COOCKIE, _players);
		}

		// -------------------------------------------
		/* 
		 * Will load the data in the game to decide if it must create a game or join an existing one
		 */
		public static int LoadNumberOfPlayers()
		{
			return PlayerPrefs.GetInt(NUMBER_OF_PLAYERS_COOCKIE, -1);
		}

		// -------------------------------------------
		/* 
		 * Will save the data of the IP address of the server
		 */
		public static void SaveIPAddressServer(string _ipAddress)
		{
			PlayerPrefs.SetString(IP_ADDRESS_COOCKIE, _ipAddress);
		}

		// -------------------------------------------
		/* 
		 * Will load the IP address of the server
		 */
		public static string LoadIPAddressServer()
		{
			return PlayerPrefs.GetString(IP_ADDRESS_COOCKIE, SOCKET_SERVER_ADDRESS);
		}

		// -------------------------------------------
		/* 
		 * Will save the port of the server
		 */
		public static void SavePortServer(int _port)
		{
			PlayerPrefs.SetInt(PORT_ADDRESS_COOCKIE, _port);
		}

		// -------------------------------------------
		/* 
		 * Will load the port server address
		 */
		public static int LoadPortServer()
		{
			return PlayerPrefs.GetInt(PORT_ADDRESS_COOCKIE, PORT_SERVER_ADDRESS);
		}

		// -------------------------------------------
		/* 
		 * Will save the room number to use in the server
		 */
		public static void SaveRoomNumberInServer(int _room)
		{
			PlayerPrefs.SetInt(ROOM_NUMBER_COOCKIE, _room);
		}

		// -------------------------------------------
		/* 
		 * Will load the port server address
		 */
		public static int LoadRoomNumberInServer(int _defaultRoom)
		{
			return PlayerPrefs.GetInt(ROOM_NUMBER_COOCKIE, _defaultRoom);
		}

		// -------------------------------------------
		/* 
		 * Will save the assigned name room for the lobby
		 */
		public static void SaveNameRoomLobby(string _nameRoom)
		{
			PlayerPrefs.SetString(NAME_ROOM_LOOBY_COOCKIE, _nameRoom);
		}

		// -------------------------------------------
		/* 
		 * Will load the assigned name room for the lobby
		 */
		public static string LoadNameRoomLobby()
		{
			return PlayerPrefs.GetString(NAME_ROOM_LOOBY_COOCKIE, "");
		}

		// -------------------------------------------
		/* 
		 * Will save if we are in the lobby
		 */
		public static void SaveIsRoomLobby(bool _value)
		{
			PlayerPrefs.SetInt(IS_ROOM_LOOBY_COOCKIE, (_value ? 1 : 0));
		}

		// -------------------------------------------
		/* 
		 * Will load if we are in the lobby
		 */
		public static bool LoadIsRoomLobby()
		{
			return PlayerPrefs.GetInt(IS_ROOM_LOOBY_COOCKIE, 0) == 1;
		}


		// -------------------------------------------
		/* 
		 * Will save the invited friends to the game
		 */
		public static void SaveFriendsGame(string _friends)
		{
			PlayerPrefs.SetString(FACEBOOK_FRIENDS_COOCKIE, _friends);
		}

		// -------------------------------------------
		/* 
		 * Will load the friends of the game
		 */
		public static string LoadFriendsGame()
		{
			return PlayerPrefs.GetString(FACEBOOK_FRIENDS_COOCKIE, "");
		}

		// -------------------------------------------
		/* 
		 * Will save the id of the machine which host the room
		 */
		public static void SaveMachineIDServer(int _idMachineHostRoom)
		{
			PlayerPrefs.SetInt(MACHINE_ID_HOST_ROOM_COOCKIE, _idMachineHostRoom);
		}

		// -------------------------------------------
		/* 
		 * Will load the friends of the game
		 */
		public static int LoadMachineIDServer(int _idMachineHostRoom)
		{
			return PlayerPrefs.GetInt(MACHINE_ID_HOST_ROOM_COOCKIE, _idMachineHostRoom);
		}

		// -------------------------------------------
		/* 
		* Save a flag to report if we need to use or not the Google VR
		*/
		public static void SaveEnableCardboard(bool _enabledCardboard)
		{
			PlayerPrefs.SetInt(CARDBOARD_ENABLE_COOCKIE, (_enabledCardboard ? 1 : 0));
		}

		// -------------------------------------------
		/* 
		 * Get the if we need to use or not the Google VR
		 */
		public static bool LoadEnableCardboard()
		{
			return (PlayerPrefs.GetInt(CARDBOARD_ENABLE_COOCKIE, 0) == 1);
		}

		// -------------------------------------------
		/* 
		 * Will save generic additional data
		 */
		public static void SaveExtraData(string _extraData)
		{
			PlayerPrefs.SetString(EXTRA_DATA_COOCKIE, _extraData);
		}

		// -------------------------------------------
		/* 
		 * Will load the generic additional data
		 */
		public static string LoadExtraData()
		{
			return PlayerPrefs.GetString(EXTRA_DATA_COOCKIE, "");
		}
	}
}

