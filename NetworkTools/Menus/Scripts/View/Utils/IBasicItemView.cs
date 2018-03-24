using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * IBasicItemView
	 * 
	 * Interface of a basic item
	 * 
	 * @author Esteban Gallardo
	 */
	public interface IBasicItemView : IBasicView
	{
		// FUNCTIONS
		GameObject ContainerParent { get; set; }
	}
}
