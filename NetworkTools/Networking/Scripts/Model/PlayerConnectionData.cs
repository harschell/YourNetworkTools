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
		private byte[] m_textureData;

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
		 * SetTexture
		 */
		public void SetTexture(int _texWidth, int _textHeight, byte[] _textureData)
		{
			m_textureData = new byte[_textureData.Length];
			Array.Copy(_textureData, m_textureData, m_textureData.Length);
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_TEXTURE_REMOTE_STREAMING_DATA, m_id, _texWidth, _textHeight, m_textureData);
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