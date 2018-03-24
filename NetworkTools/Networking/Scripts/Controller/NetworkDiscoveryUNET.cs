// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * NetworkDiscoveryUNET
	 * 
	 * Connection process:
	 * 
	 *  1 - Tries to look for a server, if it finds it, the it connects as a client
	 *  2 - If it fails to find a server, it starts as a server
	 * 
	 * @author Esteban Gallardo
	 */
	public class NetworkDiscoveryUNET : NetworkDiscovery
	{
		public bool receivedBroadcast { get; private set; }
		public int BroadcastInterval = 1000;
		public string ServerIp { get; private set; }

		private bool m_isServer = false;

		// -------------------------------------------
		/* 
		 * Start looking for a server to work as a client
		 */
		private void Start()
		{
			Debug.Log("NetworkDiscoveryUNET::START!!!!");

			// Initializes NetworkDiscovery.
			Initialize();

			if (!CheckComponents())
			{
				Debug.Log("Invalid configuration detected. Network Discovery disabled.");
				Destroy(this);
				return;
			}

			broadcastInterval = BroadcastInterval;

			// Start listening for broadcasts.
			StartAsClient();

			float InvokeWaitTime = 3 * BroadcastInterval + Random.value * 3 * BroadcastInterval;
			Invoke("MaybeInitAsServer", InvokeWaitTime * 0.001f);

			NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnBasicEvent);
		}

		// -------------------------------------------
		/* 
		 * Check all the needed components to start the process
		 */
		private bool CheckComponents()
		{
#if !UNITY_EDITOR
			if (GenericNetworkTransmitter.Instance == null)
			{
				Debug.Log("Need a UNetNetworkTransmitter in the scene for sending data");
				return false;
			}
#endif
			if (NetworkManager.singleton == null)
			{
				Debug.Log("Need a NetworkManager in the scene");
				return false;
			}

			if (SceneManager.GetActiveScene().name != "NetworkScene")
			{
				if (GameObject.FindObjectOfType<NetworkVariablesController>() == null)
				{
					Debug.Log("Need a NetworkVariablesController in the scene");
					return false;
				}
			}

			return true;
		}

		// -------------------------------------------
		/* 
		 * Fallback call that is called when it fails to find a server
		 */
		private void MaybeInitAsServer()
		{
			// If we Recieved a broadcast then we should not start as a server.
			if (receivedBroadcast)
			{
				return;
			}

			StartCoroutine(InitAsServer());
		}

		// -------------------------------------------
		/* 
		 * Starts working as a server
		 */
		private IEnumerator InitAsServer()
		{
			Debug.Log("Acting as host");

			CommunicationsController.Instance.IsServer = true;
			m_isServer = true;

			StopBroadcast();
			yield return null;

			NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_REGISTER_ALL_NETWORK_PREFABS);

			NetworkManager.singleton.StartHost();
			yield return null;

			// Start broadcasting for other clients.
			StartAsServer();

			Debug.Log("NetworkDiscoveryWithAnchors::InitAsServer::STARTED AS A SERVER.");
		}

		// -------------------------------------------
		/* 
		 * Receives the response of the broadcast of connection and connects as a client
		 */
		public override void OnReceivedBroadcast(string fromAddress, string data)
		{
			if (receivedBroadcast)
			{
				return;
			}

			Debug.Log("Acting as client");
			receivedBroadcast = true;
			StopBroadcast();

			NetworkManager.singleton.networkAddress = fromAddress;
			ServerIp = fromAddress.Substring(fromAddress.LastIndexOf(':') + 1);

#if !UNITY_EDITOR
			// Tell the network transmitter the IP to request anchor data from if needed.
			GenericNetworkTransmitter.Instance.SetServerIP(ServerIp);
#else
			Debug.LogWarning("This script will need modification to work in the Unity Editor");
#endif
			NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_COMMUNICATIONSCONTROLLER_REGISTER_ALL_NETWORK_PREFABS);

			// And join the networked experience as a client.
			NetworkManager.singleton.StartClient();

			CommunicationsController.Instance.IsServer = false;

			// REPORT STARTED AS CLIENT 
			Debug.Log("NetworkDiscoveryWithAnchors::OnReceivedBroadcast::STARTED AS A CLIENT.");
		}

		// -------------------------------------------
		/* 
		* Destroy resources
		*/
		private void Destroy()
		{
			if (m_isServer)
			{
				NetworkManager.singleton.StopHost();
			}
			else
			{
				NetworkManager.singleton.StopClient();
			}
			NetworkEventController.Instance.NetworkEvent -= OnBasicEvent;
			Destroy(this);
		}

		// -------------------------------------------
		/* 
		* Manager of global events
		*/
		private void OnBasicEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_DESTROY_NETWORK_COMMUNICATIONS)
			{
				Destroy();
			}
		}
	}
}