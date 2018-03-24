using System;
using System.Collections;
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
			MenuScreenController.Instance.CreateNewScreen(ScreenMenuMainView.SCREEN_NAME, ScreenTypePreviousActionEnum.DESTROY_ALL_SCREENS, true);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{

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