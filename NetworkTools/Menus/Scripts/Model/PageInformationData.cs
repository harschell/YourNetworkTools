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
	 * PageInformation
	 * 
	 * It is used only for the example code, it's not necessary for your project
	 * 
	 * @author Esteban Gallardo
	 */
	[System.Serializable]
	public class PageInformationData
	{
		public string MyTitle;
		public string MyText;
		public Sprite MySprite;
		public string EventData;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public PageInformationData(string _title, string _text, Sprite _sprite, string _eventData)
		{
			MyTitle = _title;
			MyText = _text;
			MySprite = _sprite;
			EventData = _eventData;
		}

		// -------------------------------------------
		/* 
		 * Clone
		 */
		public PageInformationData Clone()
		{
			return new PageInformationData(MyTitle, MyText, MySprite, EventData);
		}
	}
}