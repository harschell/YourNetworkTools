using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * SelectableButtonView
	 * 
	 * This class will be added automatically (or manually)
	 * to all interactable elements of an screen
	 * 
	 * @author Esteban Gallardo
	 */
	public class SelectableButtonView : MonoBehaviour
	{
		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const string SELECTOR_COMPONENT_NAME = "Selector";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_selector;

		// -------------------------------------------
		/* 
		 * We add a visual selector (if there is not already one with the name "Selector")
		 * and we also add a box collider to be able for the screen to be used
		 * with systems like Leap Motion
		 */
		public void Initialize()
		{
			if (transform.Find(SELECTOR_COMPONENT_NAME) != null)
			{
				m_selector = transform.Find(SELECTOR_COMPONENT_NAME).gameObject;
				m_selector.SetActive(false);
			}
			else
			{
				GameObject nodeImage = new GameObject();
				nodeImage.transform.SetParent(transform, false);
				Rect rectBase = GetComponent<RectTransform>().rect;
				Rect mySpriteRect = new Rect(0, 0, MenuScreenController.Instance.SelectorGraphic.rect.width, MenuScreenController.Instance.SelectorGraphic.rect.height);
				UtilitiesNetwork.AddSprite(nodeImage, MenuScreenController.Instance.SelectorGraphic, mySpriteRect, rectBase, new Vector2(0.5f, 0.5f));
				nodeImage.name = SELECTOR_COMPONENT_NAME;
				nodeImage.transform.SetAsFirstSibling();
				m_selector = transform.Find(SELECTOR_COMPONENT_NAME).gameObject;
				m_selector.SetActive(false);
			}
			m_selector.layer = LayerMask.NameToLayer("UI");
		}

		// -------------------------------------------
		/* 
		 * Destroy all the references
		 */
		public void Destroy()
		{
			this.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
			m_selector = null;
		}

		// -------------------------------------------
		/* 
		 * Will be called to invoke the button functionality
		 */
		public void InvokeButton()
		{
			if (this.gameObject.GetComponent<Button>() != null)
			{
				this.gameObject.GetComponent<Button>().onClick.Invoke();
			}
		}

		// -------------------------------------------
		/* 
		 * Will enable the selector component
		 */
		public void EnableSelector(bool _value)
		{
			if (m_selector != null)
			{
				m_selector.SetActive(_value);
			}
		}
	}
}