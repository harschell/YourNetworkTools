using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * NetworkWorldObjectData
	 * 
	 * World object that inherints the functionality of Network Object
	 * 
	 * @author Esteban Gallardo
	 */
	[RequireComponent(typeof(NetworkIdentity))]
	public class NetworkWorldObjectData : NetworkObjectData, INetworkObject
	{
		// -----------------------------------------
		// CONSTANTS
		// -----------------------------------------
		public const string COMMAND_TRANSFORM_POSITION = "CmdTransformPosition";
		public const string COMMAND_TRANSFORM_FORWARD = "CmdTransformRotation";
		public const string COMMAND_TRANSFORM_SCALE = "CmdTransformScale";

		// -----------------------------------------
		// SYNCVARS
		// -----------------------------------------
		[SyncVar]
		private Vector3 m_localForward;

		[SyncVar]
		private Vector3 m_localPosition;

		[SyncVar]
		private Vector3 m_localScale;

		// -------------------------------------------
		/* 
		* Invoke with a command function (function that runs in server)
		*/
		public override void InvokeFunction(string _nameFunction, object _value)
		{
			switch (_nameFunction)
			{
				case COMMAND_TRANSFORM_POSITION:
					CmdPosition((Vector3)_value);
					break;

				case COMMAND_TRANSFORM_FORWARD:
					CmdForward((Vector3)_value);
					break;

				case COMMAND_TRANSFORM_SCALE:
					CmdScale((Vector3)_value);
					break;

				default:
					break;
			}
		}

		// -------------------------------------------
		/* 
		 * Sets the m_localPosition on clients.
		 */
		private void CmdPosition(Vector3 _position)
		{
			if (!m_localPosition.Equals(_position)) NetworkEventController.Instance.DelayLocalEvent(NetworkEventController.EVENT_NETWORKVARIABLE_STATE_REPORT, 0.01f, this.gameObject, AssignedName, _position);
			m_localPosition = _position;
			transform.localPosition = m_localPosition;
		}

		// -------------------------------------------
		/* 
		* GetPosition
		*/
		public Vector3 GetPosition()
		{
			return m_localPosition;
		}

		// -------------------------------------------
		/* 
		* GetPosition
		*/
		public void SetPosition(Vector3 _position)
		{
			if (IsLocalPlayer())
			{
				if (!m_localPosition.Equals(_position)) NetworkEventController.Instance.DelayLocalEvent(NetworkEventController.EVENT_NETWORKVARIABLE_STATE_REPORT, 0.01f, this.gameObject, AssignedName, _position);

				m_localPosition = _position;
				transform.localPosition = m_localPosition;
			}

			// MODIFY LOCAL CLIENT
			NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_UPDATE_PROPERTY, this.gameObject, COMMAND_TRANSFORM_POSITION, _position);
		}

		// -------------------------------------------
		/* 
		 * Sets the m_localRotation on clients.
		 */
		private void CmdForward(Vector3 _forward)
		{
			if (!m_localForward.Equals(_forward)) NetworkEventController.Instance.DelayLocalEvent(NetworkEventController.EVENT_NETWORKVARIABLE_STATE_REPORT, 0.01f, this.gameObject, AssignedName, _forward);
			m_localForward = _forward;
			transform.forward = m_localForward;
		}

		// -------------------------------------------
		/* 
		* GetForward
		*/
		public Vector3 GetForward()
		{
			return m_localForward;
		}

		// -------------------------------------------
		/* 
		* SetForward
		*/
		public void SetForward(Vector3 _forward)
		{
			if (IsLocalPlayer())
			{
				if (!m_localForward.Equals(_forward)) NetworkEventController.Instance.DelayLocalEvent(NetworkEventController.EVENT_NETWORKVARIABLE_STATE_REPORT, 0.01f, this.gameObject, AssignedName, _forward);

				m_localForward = _forward;
				transform.forward = m_localForward;
			}

			// MODIFY LOCAL CLIENT
			NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_UPDATE_PROPERTY, this.gameObject, COMMAND_TRANSFORM_FORWARD, _forward);
		}

		// -------------------------------------------
		/* 
		 * Sets the m_localScale on clients.
		 */
		private void CmdScale(Vector3 _scale)
		{
			if (!m_localScale.Equals(_scale)) NetworkEventController.Instance.DelayLocalEvent(NetworkEventController.EVENT_NETWORKVARIABLE_STATE_REPORT, 0.01f, this.gameObject, AssignedName, _scale);
			m_localScale = _scale;
			transform.localScale = m_localScale;
		}

		// -------------------------------------------
		/* 
		* GetScale
		*/
		public Vector3 GetScale()
		{
			return m_localScale;
		}

		// -------------------------------------------
		/* 
		* SetScale
		*/
		public void SetScale(Vector3 _scale)
		{
			if (IsLocalPlayer())
			{
				if (!m_localScale.Equals(_scale)) NetworkEventController.Instance.DelayLocalEvent(NetworkEventController.EVENT_NETWORKVARIABLE_STATE_REPORT, 0.01f, this.gameObject, AssignedName, _scale);

				m_localScale = _scale;
				transform.localScale = m_localScale;
			}

			// MODIFY LOCAL CLIENT
			NetworkEventController.Instance.DispatchLocalEvent(NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_COMMAND_UPDATE_PROPERTY, this.gameObject, COMMAND_TRANSFORM_SCALE, _scale);
		}

		// -------------------------------------------
		/* 
		 * Update the rotation locally and synchronize with the others
		 */
		void Update()
		{
			if (!IsLocalPlayer())
			{
				if (!m_localPosition.Equals(transform.localPosition)) NetworkEventController.Instance.DelayLocalEvent(NetworkEventController.EVENT_NETWORKVARIABLE_STATE_REPORT, 0.01f, this.gameObject, AssignedName, m_localPosition);
				if (!m_localForward.Equals(transform.forward)) NetworkEventController.Instance.DelayLocalEvent(NetworkEventController.EVENT_NETWORKVARIABLE_STATE_REPORT, 0.01f, this.gameObject, AssignedName, m_localForward);
				if (!m_localScale.Equals(transform.localScale)) NetworkEventController.Instance.DelayLocalEvent(NetworkEventController.EVENT_NETWORKVARIABLE_STATE_REPORT, 0.01f, this.gameObject, AssignedName, m_localScale);

				transform.localPosition = m_localPosition;
				transform.forward = m_localForward;
				transform.localScale = m_localScale;
				return;
			}
		}

		// -------------------------------------------
		/* 
		* Will return in a string the important information of this network object
		*/
		public override string GetInformation()
		{
			return m_localPosition.ToString() + "::" + m_localForward.ToString() + "::" + m_localScale.ToString();
		}

	}
}