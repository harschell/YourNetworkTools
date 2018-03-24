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
	 * ItemStringView
	 * 
	 * Display an item that only contains a textfield
	 * 
	 * @author Esteban Gallardo
	 */
	public class ItemStringView : BaseItemView, IBasicView
	{
		// ----------------------
		// PRIVATE MEMBERS
		// ----------------------
		private string data = "";

		// ----------------------
		// GETTERS/SETTERS
		// ----------------------
		public string Data
		{
			get { return data; }
		}

		// -------------------------------------------
		/* 
		 * Initialitzation of all the references to the graphic resources
		 */
		public override void Initialize(params object[] _list)
		{
			base.Initialize();
			data = (string)_list[0];
			this.gameObject.transform.Find("Text").GetComponent<Text>().text = data;
		}
	}
}