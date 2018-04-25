using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * PlayerConnectionData
	 * 
	 * Keeps the information about the player's connection
	 * 
	 * @author Esteban Gallardo
	 */
	public class PlayerConnectionData : IEquatable<PlayerConnectionData>
	{
		// ----------------------------------------------
		// PRIVATE VARIABLES
		// ----------------------------------------------	
		private int m_id;
		private string m_networkAddress;
		private GameObject m_referenceObject;
		private List<Dictionary<string, object>> m_messages = new List<Dictionary<string, object>>();
		private byte[] m_binaryData;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public int Id
		{
			get { return m_id; }
		}
		public string Name
		{
			get { return "Client[" + m_id + "]"; }
		}
		public string NetworkAddress
		{
			get { return m_networkAddress; }
			set
			{
				m_networkAddress = value;
				NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_PLAYERCONNECTIONDATA_NETWORK_ADDRESS, m_id, m_networkAddress);
			}
		}
		public GameObject Reference
		{
			get { return m_referenceObject; }
		}


		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public PlayerConnectionData(int _id, GameObject _reference)
		{
			m_id = _id;
			m_referenceObject = _reference;
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Destroy()
		{
			m_referenceObject = null;
		}

		// -------------------------------------------
		/* 
		 * PushMessage
		 */
		public void PushMessage(Dictionary<string, object> _data)
		{
			m_messages.Add(_data);
		}

		// -------------------------------------------
		/* 
		 * SetBinaryData
		 */
		public void SetBinaryData(byte[] _binaryData)
		{
			// GET NAME EVENT
			int counter = 0;
			int sizeNameEvent = BitConverter.ToInt32(_binaryData, counter);
			counter += 4;
			byte[] binaryNameEvent = new byte[sizeNameEvent];
			Array.Copy(_binaryData, counter, binaryNameEvent, 0, sizeNameEvent);
			counter += sizeNameEvent;
			string nameEvent = Encoding.ASCII.GetString(binaryNameEvent);

			// GET DATA CONTENT
			int sizeContentEvent = BitConverter.ToInt32(_binaryData, counter);
			counter += 4;
			m_binaryData = new byte[sizeContentEvent];
			Array.Copy(_binaryData, counter, m_binaryData, 0, sizeContentEvent);

			// DISPATCH LOCAL EVENT
			NetworkEventController.Instance.DispatchLocalEvent(nameEvent, m_id, m_binaryData);
		}

		// -------------------------------------------
		/* 
		 * PopMessage
		 */
		public Dictionary<string, object> PopMessage()
		{
			if (m_messages.Count == 0) return null;
			Dictionary<string, object> message = m_messages[m_messages.Count - 1];
			m_messages.RemoveAt(m_messages.Count - 1);
			return message;
		}

		// -------------------------------------------
		/* 
		 * GetHashCode
		 */
		public int GetHashCode(PlayerConnectionData obj)
		{
			return obj.Id;
		}

		// -------------------------------------------
		/* 
		 * Equals
		 */
		public bool Equals(PlayerConnectionData _other)
		{
			return Id == _other.Id;
		}
	}
}