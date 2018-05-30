using System;
using System.Collections;
using UnityEngine;
using YourCommonTools;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * ScreenSplashView
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenSplashView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_SPLASH";

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

			StartCoroutine(ShowSplashDelay());
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		IEnumerator ShowSplashDelay()
		{
			yield return new WaitForSeconds(5);
			MenuScreenController.Instance.CreateNewScreen(ScreenMenuMainView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true);
		}

		// -------------------------------------------
		/* 
		 * GetGameObject
		 */
		public GameObject GetGameObject()
		{
			return this.gameObject;
		}
	}
}