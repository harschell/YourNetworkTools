using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using YourCommonTools;

namespace YourNetworkingTools
{
	/******************************************
	 * 
	 * IGameNetworkActor
	 * 
	 * Interface that should be implemented by all the game network actors
	 * 
	 * @author Esteban Gallardo
	 */
	public interface IGameNetworkActor : IGameActor
	{
		// GETTERS
		void Awake();
		void Start();
		void InitializeCommon();
		bool IsMine();
		NetworkID NetworkID { get; }
		string EventNameObjectCreated { set; }
	}
}
