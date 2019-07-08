using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
	[System.Serializable]
	public struct GameLevel
	{
		public StatusLevel Status;//статус (открыт, закрыт)

		public int Index; //индекс уровня			
		
		public int Rating 
		{
			get {return rating;}
			set 
			{
				if (value > rating && value <= 3)
				{
					rating = value;
				}
			}
		}

		int rating; //рейнинг уровня добытый игроком		
	}


	[System.Serializable]
	public enum StatusLevel
	{
		CLOSE, OPEN, COMPLETED
	}
}
