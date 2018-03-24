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
	 * ScreenMenuLoadingView
	 * 
	 * Loading screen
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenMenuLoadingView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_LOADING";

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

			this.gameObject.transform.Find("Content/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("message.loading");

			MenuEventController.Instance.MenuEvent += new MenuEventHandler(OnMenuEvent);
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
		 * OnMenuBasicEvent
		 */
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == MenuScreenController.EVENT_MENU_FORCE_DESTRUCTION_POPUP)
			{
				MenuEventController.Instance.DispatchMenuEvent(MenuScreenController.EVENT_MENU_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
			}
		}
	}
}