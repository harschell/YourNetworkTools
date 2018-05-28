using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * ItemFriendView
	 * 
	 * @author Esteban Gallardo
	 */
	public class ItemFriendView : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_ITEM_FRIEND_SELECTED = "EVENT_ITEM_FRIEND_SELECTED";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private string m_facebookID;
		private string m_facebookName;
		private Text m_text;
		private Image m_background;
		private bool m_selected = false;
		private GameObject m_selector;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public string FacebookID
		{
			get { return m_facebookID; }
		}
		public string FacebookName
		{
			get { return m_facebookName; }
		}
		public virtual bool Selected
		{
			get { return m_selected; }
			set
			{
				m_selected = value;
				if (m_selected)
				{
					m_background.color = Color.cyan;
				}
				else
				{
					m_background.color = Color.white;
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Initialization
		 */
		public void Initialization(string _facebookID, string _facebookName)
		{
			m_facebookID = _facebookID;
			m_facebookName = _facebookName;
			m_text = transform.Find("Text").GetComponent<Text>();
			m_background = transform.GetComponent<Image>();
			transform.GetComponent<Button>().onClick.AddListener(ButtonPressed);
			m_text.text = _facebookName;
		}

		// -------------------------------------------
		/* 
		 * ButtonPressed
		 */
		public void ButtonPressed()
		{
			UIEventController.Instance.DispatchUIEvent(EVENT_ITEM_FRIEND_SELECTED, this);
		}
	}
}