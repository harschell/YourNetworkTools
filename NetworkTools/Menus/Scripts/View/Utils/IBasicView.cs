using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourNetworkingTools
{

	/******************************************
	 * 
	 * IBasicView
	 * 
	 * Basic interface that force the programmer to 
	 * initialize and free resources to avoid memory leaks
	 * 
	 * @author Esteban Gallardo
	 */
	public interface IBasicView
	{
		// FUNCTIONS
		void Initialize(params object[] _list);
		void Destroy();
		void SetActivation(bool _activation);
		GameObject GetGameObject();
	}
}