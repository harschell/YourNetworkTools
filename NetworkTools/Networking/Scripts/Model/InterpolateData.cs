using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * InterpolateData
	 * 
	 * Keeps the information of a gameobject to be interpolated
	 * 
	 * @author Esteban Gallardo
	 */
	public class InterpolateData : IEquatable<InterpolateData>
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------
		public const string EVENT_INTERPOLATE_COMPLETED = "EVENT_INTERPOLATE_COMPLETED";

		// -----------------------------------------
		// PRIVATE VARIABLES
		// -----------------------------------------
		private GameObject m_gameActor;
		private Vector3 m_origin;
		private Vector3 m_goal;
		private float m_totalTime;
		private float m_timeDone;

		// -----------------------------------------
		// GETTERS/SETTERS
		// -----------------------------------------
		public GameObject GameActor
		{
			get { return m_gameActor; }
		}
		public Vector3 Goal
		{
			get { return m_goal; }
			set { m_goal = value; }
		}
		public float TotalTime
		{
			get { return m_totalTime; }
			set { m_totalTime = value; }
		}
		public float TimeDone
		{
			get { return m_timeDone; }
			set { m_timeDone = 0; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public InterpolateData(GameObject _actor, Vector3 _origin, Vector3 _goal, float _totalTime, float _timeDone)
		{
			m_gameActor = _actor;
			ResetData(_origin, _goal, _totalTime, _timeDone);
		}

		// -------------------------------------------
		/* 
		 * ResetData
		 */
		public void ResetData(Vector3 _origin, Vector3 _goal, float _totalTime, float _timeDone)
		{
			m_origin = new Vector3(_origin.x, _origin.y, _origin.z);
			m_goal = new Vector3(_goal.x, _goal.y, _goal.z);
			m_totalTime = _totalTime;
			m_timeDone = _timeDone;
		}

		// -------------------------------------------
		/* 
		 * Release resources
		 */
		public void Destroy()
		{
			m_gameActor = null;
		}

		// -------------------------------------------
		/* 
		 * Interpolate the position between two points
		 */
		public bool Inperpolate()
		{
			if (m_gameActor == null) return true;

			m_timeDone += Time.deltaTime;
			if (m_timeDone <= m_totalTime)
			{
				Vector3 forwardTarget = (m_goal - m_origin);
				float increaseFactor = (1 - ((m_totalTime - m_timeDone) / m_totalTime));
				m_gameActor.transform.position = m_origin + (increaseFactor * forwardTarget);
				return false;
			}
			else
			{
				if (m_timeDone <= m_totalTime)
				{
					return false;
				}
				else
				{
					NetworkEventController.Instance.DispatchLocalEvent(EVENT_INTERPOLATE_COMPLETED, m_gameActor);
					return true;
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Equals
		 */
		public bool Equals(InterpolateData _other)
		{
			return m_gameActor == _other.GameActor;
		}
	}
}