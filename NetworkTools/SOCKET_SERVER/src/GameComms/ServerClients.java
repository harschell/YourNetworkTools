package GameComms;

import java.awt.Point;
import java.net.Socket;
import java.nio.ByteBuffer;
import java.io.*;
import java.util.*;
import java.nio.ByteOrder;

import Common.BasicController;
import Main.*;

public class ServerClients extends Thread {

	// ----------------------------------------------
	// EVENTS
	// ----------------------------------------------
	public static String EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID 	= "EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID";
	public static String EVENT_CLIENT_TCP_ROOM_ID 				= "EVENT_CLIENT_TCP_ROOM_ID";
	public static String EVENT_CLIENT_TCP_PLAYER_UID			= "EVENT_CLIENT_TCP_PLAYER_UID";
	public static String EVENT_CLIENT_TCP_LIST_OF_GAME_ROOMS	= "EVENT_CLIENT_TCP_LIST_OF_GAME_ROOMS";
	
	public static String EVENT_SERVERCLIENTS_DELETE_CLIENT		= "EVENT_SERVERCLIENTS_DELETE_CLIENT";
	
	public static int TOTAL_FREE_ROOM_COUNTER = 10000000;
	public static int TOTAL_PLAYER_MAX_COUNTER = 10000000;
	
	// ----------------------------------------------
	// PRIVATE VARIABLES
	// ----------------------------------------------	
    private Vector<ClientConnection> m_listClients = new Vector<ClientConnection>();
	private Vector<ServerRoom> m_listRooms = new Vector<ServerRoom>();
	private ClientConnection m_currentConnection;
	private int m_nextFreeRoom = 0;
	private int m_counterPlayers = 0;
	private boolean m_running = true;
	
	// ----------------------------------------------
	// FUNCTIONS
	// ----------------------------------------------	
    public ServerClients()
    {
    }
    
    public void Destroy()
    {
    	m_running = false;
		for (int i = 0; i < m_listRooms.size(); i++)
        {
			ServerRoom serverRoom = m_listRooms.elementAt(i);
			serverRoom.Destroy();
        }
    }
	
	private ServerRoom GetRoomByID(int _roomNumber)
	{
		for (int i = 0; i < m_listRooms.size(); i++)
        {
			ServerRoom serverRoom = m_listRooms.elementAt(i);
			if (serverRoom.IdRoom == _roomNumber) return serverRoom;
        }
		return null;
	}
    
	public boolean DeleteRoom(ServerRoom _serverRoom) 
	{
		for (int i = 0; i < m_listRooms.size(); i++)
        {
			ServerRoom serverRoom = m_listRooms.elementAt(i);
			if (serverRoom == _serverRoom)
			{
				m_listRooms.removeElementAt(i);
				return true;
			}
        }
		return false;
	}
	
    private boolean ExistClient(Socket _newClient) 
	{
		for (int i = 0; i < m_listClients.size(); i++)
        {
			ClientConnection clientSocket = m_listClients.elementAt(i);
			if (clientSocket.GetSocket() == _newClient) return true;
        }
		return false;
	}

	private boolean DeleteClient(ClientConnection _clientClosed, boolean _sendEvent) 
	{
		for (int i = 0; i < m_listClients.size(); i++)
        {
			ClientConnection clientSocket = m_listClients.elementAt(i);
			if (clientSocket == _clientClosed)
			{
				if (_sendEvent)
				{
					BasicController.getInstance().DispatchMyEvent(EVENT_SERVERCLIENTS_DELETE_CLIENT, clientSocket.GetLocalID());
				}
				m_listClients.removeElementAt(i);
				return true;
			}
        }
		return false;
	}
	
	public void AddNewClient(Socket _newClient) throws Exception
	{
		if (ServerGame.EnableLogMessages)
		{
			System.out.println("ServerClients::AddNewClient");	
		}		
		if (!ExistClient(_newClient))
    	{
			ClientConnection clientConnection = new ClientConnection(m_counterPlayers, _newClient);
			m_listClients.add(clientConnection);
			String packet = EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID + ServerGame.TOKEN_SEPARATOR_EVENTS
							+ "-1" + ServerGame.TOKEN_SEPARATOR_EVENTS
							+ "-1" + ServerGame.TOKEN_SEPARATOR_EVENTS
							+ "-1" + ServerGame.TOKEN_SEPARATOR_EVENTS
							+ m_nextFreeRoom + ServerGame.TOKEN_SEPARATOR_EVENTS
							+ m_counterPlayers;
			clientConnection.SendEvent(packet);
			m_nextFreeRoom++;
			m_nextFreeRoom = m_nextFreeRoom % TOTAL_FREE_ROOM_COUNTER;
			if (ServerGame.EnableLogMessages)
			{
				System.out.println("New client");	
			}        	
			BasicController.getInstance().DispatchMyEvent(EVENT_CLIENT_TCP_ESTABLISH_NETWORK_ID, m_counterPlayers, "Player["+m_counterPlayers+"]");
			m_counterPlayers++;
			m_counterPlayers = m_counterPlayers % TOTAL_PLAYER_MAX_COUNTER;
    	}
        else
        {
			if (ServerGame.EnableLogMessages)
			{
				System.out.println("Existing client");	
			}        	
        }
	}

	private void BroadCastMessage(String _message) throws Exception
	{
		if (_message != null)
		{
			for (int i = 0; i < m_listClients.size(); i++)
			{
				try 
				{
					ClientConnection clientConnection = m_listClients.elementAt(i);
					clientConnection.SendEvent(_message);
				} catch (Exception err)
				{
					if (ServerGame.EnableLogMessages)
					{
						System.out.println("BroadCastMessage::SendEvent:: EXCEPTION="+err.getMessage());	
					}
				}
			}
			if (ServerGame.EnableLogMessages)
			{
				System.out.println("BroadCastMessage::message="+_message);	
			}			
		}
	}

	private String ReadMessage()
	{
		for (int i = 0; i < m_listClients.size(); i++)
		{
			ClientConnection clientConnection = m_listClients.elementAt(i);
			try 
			{
				if (clientConnection.IsThereDataAvailable())
				{
					ByteArrayOutputStream packet = new ByteArrayOutputStream();
					int typeEventReceived = clientConnection.ReadPacket(packet);
					if (typeEventReceived == -1)
					{
						DeleteClient(clientConnection, false);
						return null;
					}
					else
					if (typeEventReceived == ClientConnection.MESSAGE_EVENT)
					{
						String messageBroadcast = clientConnection.ReadEvent(packet.toByteArray());
						if (messageBroadcast != null)
						{
							m_currentConnection = clientConnection;
							return messageBroadcast;
						}
					}
				}				
			} catch (Exception err)
			{
				DeleteClient(clientConnection, false);
				if (ServerGame.EnableLogMessages)
				{
					System.out.println("ServerClient::ReadMessage:Exception="+err.getMessage());	
				}				
			}
		}
		return null;
	}
	
	private void PackAndSendAllRooms() throws Exception
	{
		String invitations = "";
		for (int i = 0; i < m_listRooms.size(); i++)
        {
			ServerRoom serverRoom = m_listRooms.elementAt(i);
			if (serverRoom.IsRoomOpen)
			{
				if ((serverRoom.IsLobby) ||
					(serverRoom.ExistPlayersID(m_currentConnection.GetPlayerID())))
				{
					if (invitations.length() > 0)
					{
						invitations += ServerGame.TOKEN_SEPARATOR_PLAYERS_IDS;
					}
					invitations += serverRoom.PackRoomPlayersIDs();
				}
			}
        }		
		
		if (ServerGame.EnableLogMessages) System.out.println("+++++++++++++++++++ServerClients::PackAndSendAllRooms::invitations["+invitations+"]");		
		
		String packet = EVENT_CLIENT_TCP_LIST_OF_GAME_ROOMS + ServerGame.TOKEN_SEPARATOR_EVENTS
						+ "-1" + ServerGame.TOKEN_SEPARATOR_EVENTS
						+ "-1" + ServerGame.TOKEN_SEPARATOR_EVENTS
						+ "-1" + ServerGame.TOKEN_SEPARATOR_EVENTS
						+ invitations;		
		BroadCastMessage(packet);
	}
	
	private void TestClientsAlive()
	{
		for (int i = 0; i < m_listClients.size(); i++)
        {
			ClientConnection clientSocket = m_listClients.elementAt(i);
			if (clientSocket != null)
			{
				if (!clientSocket.TestAlive())
				{
					if (DeleteClient(clientSocket, true))
					{
						i--;
						if (ServerGame.EnableLogMessages) System.out.println("*****************************DELETING CLIENT["+i+"]");
					}
				}
			}
        }		
	}

	private void ProcessMessage(String _message) throws Exception
	{
		String[] dataConnection = _message.split(ServerGame.TOKEN_SEPARATOR_EVENTS);

		if (ServerGame.EnableLogMessages) System.out.println("ProcessMessage::MESSAGE="+_message);
		if (_message.indexOf(EVENT_CLIENT_TCP_PLAYER_UID) != -1)
		{
			String facebookID = dataConnection[4];
			m_currentConnection.SetPlayerID(facebookID);
			PackAndSendAllRooms();
		}	
		else		
		if (_message.indexOf(EVENT_CLIENT_TCP_ROOM_ID) != -1)
		{
			int roomNumber = Integer.parseInt(dataConnection[4]);
			Boolean isRoomLobby = (Integer.parseInt(dataConnection[5]) == 1);
			String roomName = dataConnection[6];
			int hostRoomID = Integer.parseInt(dataConnection[7]);
			if (roomName.length() == 0)
			{
				roomName = m_currentConnection.GetPlayerID();
			}
			String friendsIDs = dataConnection[8];
			String extraData = dataConnection[9];
			ServerRoom serverRoom = GetRoomByID(roomNumber);
			if (serverRoom == null)
			{
				serverRoom = new ServerRoom(isRoomLobby, roomNumber, roomName, friendsIDs, hostRoomID, extraData, this);	
				serverRoom.start();
				m_listRooms.add(serverRoom);
				if (ServerGame.EnableLogMessages)
				{
					System.out.println("ProcessMessage::EVENT_CLIENT_TCP_ROOM_ID:CREATE ROOM");
					System.out.println("++++++++++++++++++roomNumber["+roomNumber +"]");
					System.out.println("++++++++++++++++++isLobby["+isRoomLobby+"]");
					System.out.println("++++++++++++++++++roomName["+roomName+"]");
					System.out.println("++++++++++++++++++friendsIDs["+friendsIDs+"]");
					System.out.println("++++++++++++++++++extraData["+extraData+"]");
				}
			}
			else
			{
				if (ServerGame.EnableLogMessages) System.out.println("+++++++++++++++++++++++++++++JOINING ROOM["+roomNumber+"]");
			}
			serverRoom.AddNewClient(m_currentConnection.GetSocket());
			DeleteClient(m_currentConnection, true);
			BasicController.getInstance().DispatchMyEvent(EVENT_CLIENT_TCP_ROOM_ID, m_currentConnection.GetLocalID(), roomNumber,  roomName, serverRoom.GetTotalNumberPlayers());
			if (roomNumber != -1)
			{
				PackAndSendAllRooms();	
			}			
		}
	}

	@Override
	public void run() 
	{
		while (m_running)
		{
			try 
        	{
				wait(10);
        	} catch (Exception err) {};
			
			try 
        	{
				TestClientsAlive();
				ProcessMessage(ReadMessage());
			} catch (Exception err) { }
		}
	}
	
}
