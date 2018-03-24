using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * ScreenMenuInformationView
	 * 
	 * (DEBUG CODE) It's only used for debug purposes. 
	 * Screen used to display pages of information
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenMenuInformationView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_LOADING = "SCREEN_LOADING";
		public const string SCREEN_INFORMATION = "SCREEN_INFORMATION";
		public const string SCREEN_INFORMATION_IMAGE = "SCREEN_INFORMATION_IMAGE";
		public const string SCREEN_CONFIRMATION = "SCREEN_CONFIRMATION";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;
		private Button m_okButton;
		private Button m_cancelButton;
		private Button m_nextButton;
		private Button m_previousButton;
		private Button m_abortButton;
		private Text m_textDescription;
		private Text m_title;
		private Image m_imageContent;

		private int m_currentPage = 0;
		private List<PageInformationData> m_pagesInfo = new List<PageInformationData>();
		private bool m_forceLastPage = false;
		private bool m_lastPageVisited = false;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public bool ForceLastPage
		{
			get { return m_forceLastPage; }
			set { m_forceLastPage = value; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

			List<PageInformationData> listPages = (List<PageInformationData>)_list[0];

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			if (m_container.Find("Button_OK") != null)
			{
				m_okButton = m_container.Find("Button_OK").GetComponent<Button>();
				m_okButton.gameObject.GetComponent<Button>().onClick.AddListener(OkPressed);
			}
			if (m_container.Find("Button_Cancel") != null)
			{
				m_cancelButton = m_container.Find("Button_Cancel").GetComponent<Button>();
				m_cancelButton.gameObject.GetComponent<Button>().onClick.AddListener(CancelPressed);
			}
			if (m_container.Find("Button_Next") != null)
			{
				m_nextButton = m_container.Find("Button_Next").GetComponent<Button>();
				m_nextButton.gameObject.GetComponent<Button>().onClick.AddListener(NextPressed);
			}
			if (m_container.Find("Button_Previous") != null)
			{
				m_previousButton = m_container.Find("Button_Previous").GetComponent<Button>();
				m_previousButton.gameObject.GetComponent<Button>().onClick.AddListener(PreviousPressed);
			}
			if (m_container.Find("Button_Abort") != null)
			{
				m_abortButton = m_container.Find("Button_Abort").GetComponent<Button>();
				m_abortButton.gameObject.GetComponent<Button>().onClick.AddListener(AbortPressed);
			}

			if (m_container.Find("Text") != null)
			{
				m_textDescription = m_container.Find("Text").GetComponent<Text>();
			}
			if (m_container.Find("Title") != null)
			{
				m_title = m_container.Find("Title").GetComponent<Text>();
			}

			if (m_container.Find("Image") != null)
			{
				m_imageContent = m_container.Find("Image").GetComponent<Image>();
			}

			if (listPages != null)
			{
				for (int i = 0; i < listPages.Count; i++)
				{
					m_pagesInfo.Add(((PageInformationData)listPages[i]).Clone());
				}
			}

			MenuEventController.Instance.MenuEvent += new MenuEventHandler(OnMenuEvent);

			ChangePage(0);
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
		 * Destroy
		 */
		public void Destroy()
		{
			MenuEventController.Instance.MenuEvent -= OnMenuEvent;
			GameObject.DestroyObject(this.gameObject);
		}

		// -------------------------------------------
		/* 
		 * OkPressed
		 */
		private void OkPressed()
		{
			SoundsController.Instance.PlayFxSelection();

			if (m_currentPage + 1 < m_pagesInfo.Count)
			{
				ChangePage(1);
				return;
			}

			MenuEventController.Instance.DispatchMenuEvent(MenuScreenController.EVENT_MENU_CONFIRMATION_POPUP, this.gameObject, true, m_pagesInfo[m_currentPage].EventData);
			MenuEventController.Instance.DispatchMenuEvent(MenuScreenController.EVENT_MENU_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
		}

		// -------------------------------------------
		/* 
		 * CancelPressed
		 */
		private void CancelPressed()
		{
			SoundsController.Instance.PlayFxSelection();

			MenuEventController.Instance.DispatchMenuEvent(MenuScreenController.EVENT_MENU_CONFIRMATION_POPUP, this.gameObject, false, m_pagesInfo[m_currentPage].EventData);
			MenuEventController.Instance.DispatchMenuEvent(MenuScreenController.EVENT_MENU_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
		}

		// -------------------------------------------
		/* 
		 * AbortPressed
		 */
		private void AbortPressed()
		{
			MenuEventController.Instance.DispatchMenuEvent(MenuScreenController.EVENT_MENU_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
		}

		// -------------------------------------------
		/* 
		 * NextPressed
		 */
		private void NextPressed()
		{
			SoundsController.Instance.PlayFxSelection();
			ChangePage(1);
		}

		// -------------------------------------------
		/* 
		 * PreviousPressed
		 */
		private void PreviousPressed()
		{
			SoundsController.Instance.PlayFxSelection();
			ChangePage(-1);
		}

		// -------------------------------------------
		/* 
		 * Chage the information page
		 */
		private void ChangePage(int _value)
		{
			m_currentPage += _value;
			if (m_currentPage < 0) m_currentPage = 0;
			if (m_pagesInfo.Count == 0)
			{
				return;
			}
			else
			{
				if (m_currentPage >= m_pagesInfo.Count - 1)
				{
					m_currentPage = m_pagesInfo.Count - 1;
					m_lastPageVisited = true;
				}
			}

			if ((m_currentPage >= 0) && (m_currentPage < m_pagesInfo.Count))
			{
				if (m_title != null) m_title.text = m_pagesInfo[m_currentPage].MyTitle;
				if (m_textDescription != null) m_textDescription.text = m_pagesInfo[m_currentPage].MyText;
				if (m_imageContent != null)
				{
					if (m_pagesInfo[m_currentPage].MySprite != null)
					{
						m_imageContent.sprite = m_pagesInfo[m_currentPage].MySprite;
					}
				}
			}

			if (m_cancelButton != null) m_cancelButton.gameObject.SetActive(true);
			if (m_pagesInfo.Count == 1)
			{
				if (m_nextButton != null) m_nextButton.gameObject.SetActive(false);
				if (m_previousButton != null) m_previousButton.gameObject.SetActive(false);
				if (m_okButton != null) m_okButton.gameObject.SetActive(true);
			}
			else
			{
				if (m_currentPage == 0)
				{
					if (m_previousButton != null) m_previousButton.gameObject.SetActive(false);
					if (m_nextButton != null) m_nextButton.gameObject.SetActive(true);
				}
				else
				{
					if (m_currentPage == m_pagesInfo.Count - 1)
					{
						if (m_previousButton != null) m_previousButton.gameObject.SetActive(true);
						if (m_nextButton != null) m_nextButton.gameObject.SetActive(false);
					}
					else
					{
						if (m_previousButton != null) m_previousButton.gameObject.SetActive(true);
						if (m_nextButton != null) m_nextButton.gameObject.SetActive(true);
					}
				}

				MenuEventController.Instance.DispatchMenuEvent(MenuScreenController.EVENT_MENU_CHANGED_PAGE_POPUP, this.gameObject, m_pagesInfo[m_currentPage].EventData);
			}
		}

		// -------------------------------------------
		/* 
		 * SetTitle
		 */
		public void SetTitle(string _text)
		{
			if (m_title != null)
			{
				m_title.text = _text;
			}
		}

		// -------------------------------------------
		/* 
		 * OnMenuBasicEvent
		 */
		private void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == MenuScreenController.EVENT_MENU_FORCE_DESTRUCTION_POPUP)
			{
				MenuEventController.Instance.DispatchMenuEvent(MenuScreenController.EVENT_MENU_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
			}
			if (_nameEvent == MenuEventController.EVENT_SYSTEM_ANDROID_BACK_BUTTON)
			{
				MenuEventController.Instance.DispatchMenuEvent(MenuScreenController.EVENT_MENU_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
			}
			if (_nameEvent == MenuScreenController.EVENT_MENU_FORCE_TRIGGER_OK_BUTTON)
			{
				OkPressed();
			}
		}
	}
}