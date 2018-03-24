using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * SelectableItemView
	 * 
	 * Base class for all selectable items
	 * 
	 * @author Esteban Gallardo
	 */
	public class BaseItemView : MonoBehaviour, IBasicItemView
	{
		// ----------------------
		// PUBLIC EVENTS
		// ----------------------
		public const string EVENT_ITEM_SELECTED = "EVENT_ITEM_SELECTED";
		public const string EVENT_ITEM_CLICKED = "EVENT_ITEM_CLICKED";
		public const string EVENT_ITEM_DELETE = "EVENT_ITEM_DELETE";
		public const string EVENT_ITEM_APPLY_ACTION = "EVENT_ITEM_APPLY_ACTION";

		// PRIVATE MEMBERS
		protected GameObject containerParent;
		protected GameObject goSelected;
		protected GameObject goForeground;
		protected Transform btnDelete;
		protected Transform btnApplyAction;
		protected bool selected = false;

		// GETTERS/SETTERS
		public virtual bool Selected
		{
			get { return selected; }
			set
			{
				selected = value;
				if (selected)
				{
					if (goSelected != null) goSelected.SetActive(true);
					if (goSelected != null) goForeground.SetActive(true);
					if (btnDelete != null) btnDelete.gameObject.SetActive(true);
					if (btnApplyAction != null) btnApplyAction.gameObject.SetActive(true);
				}
				else
				{
					if (goSelected != null) goSelected.SetActive(false);
					if (goForeground != null) goForeground.SetActive(false);
					if (btnDelete != null) btnDelete.gameObject.SetActive(false);
					if (btnApplyAction != null) btnApplyAction.gameObject.SetActive(false);
				}
			}
		}
		public GameObject ContainerParent
		{
			get { return containerParent; }
			set { containerParent = value; }
		}

		// -------------------------------------------
		/* 
		 * Initialitzation of all the references to the graphic resources
		 */
		public virtual void Initialize(params object[] _list)
		{
			string messageNoContainer = "BaseItemView::Initialize::The item should always have a reference to the container parent";
			if (_list == null)
			{
				Debug.LogError(messageNoContainer);
				return;
			}
			if (_list.Length == 0)
			{
				Debug.LogError(messageNoContainer);
				return;
			}
			if (!(_list[0] is GameObject))
			{
				Debug.LogError(messageNoContainer);
				return;
			}
			containerParent = (GameObject)_list[0];

			if (transform.Find("Selected") != null)
			{
				goSelected = transform.Find("Selected").gameObject;
			}
			if (transform.Find("Foreground") != null)
			{
				goForeground = transform.Find("Foreground").gameObject;
			}
			Selected = false;

			btnDelete = transform.Find("BtnDelete");
			if (btnDelete != null)
			{
				if (btnDelete.gameObject.GetComponent<Button>() != null)
				{
					btnDelete.gameObject.GetComponent<Button>().onClick.AddListener(OnButtonDelete);
					btnDelete.gameObject.SetActive(false);
				}
				else
				{
					btnDelete = null;
				}
			}

			btnApplyAction = transform.Find("BtnApplyAction");
			if (btnApplyAction != null)
			{
				if (btnApplyAction.gameObject.GetComponent<Button>() != null)
				{
					btnApplyAction.gameObject.GetComponent<Button>().onClick.AddListener(OnButtonApplyAction);
					btnApplyAction.gameObject.SetActive(false);
				}
				else
				{
					btnApplyAction = null;
				}
			}

			if (this.gameObject.GetComponent<Button>() != null)
			{
				this.gameObject.GetComponent<Button>().onClick.AddListener(OnClickButton);
			}
		}


		// -------------------------------------------
		/* 
		 * GetGameObject
		 */
		public GameObject GetGameObject()
		{
			return this.gameObject;
		}

		// -------------------------------------------
		/* 
		 * Destroy all the references		
		 */
		public virtual void Destroy()
		{
			containerParent = null;
			if (this.gameObject.GetComponent<Button>() != null)
			{
				this.gameObject.GetComponent<Button>().onClick.RemoveListener(OnClickButton);
			}
			if (btnApplyAction != null)
			{
				if (btnApplyAction.gameObject.GetComponent<Button>() != null)
				{
					btnApplyAction.gameObject.GetComponent<Button>().onClick.RemoveListener(OnButtonApplyAction);
				}
				btnApplyAction = null;
			}
			if (btnDelete != null)
			{
				if (btnDelete.gameObject.GetComponent<Button>() != null)
				{
					btnDelete.gameObject.GetComponent<Button>().onClick.RemoveListener(OnButtonDelete);
				}
				btnDelete = null;
			}
		}

		// -------------------------------------------
		/* 
		 * Call the button interaction
		 */
		public virtual void OnClickButton()
		{
			Selected = !Selected;
		}

		// -------------------------------------------
		/* 
		 * Call the delete button 
		 */
		public virtual void OnButtonDelete()
		{
		}

		// -------------------------------------------
		/* 
		 * Call the delete button 
		 */
		public virtual void OnButtonApplyAction()
		{
		}

		// -------------------------------------------
		/* 
		 * SetActivation
		 */
		public void SetActivation(bool _activation)
		{
			throw new NotImplementedException();
		}


		// -------------------------------------------
		/* 
		 * Runs an action
		 */
		public virtual void ApplyAction()
		{
		}
	}
}