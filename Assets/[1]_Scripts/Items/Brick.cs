using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks
{
	public class Brick : ItemBrick 
	{

	#region Update	

		void OnDestroy()
		{			
			StopAllCoroutines();
			bricksController.RemoveItem(this);			
		}		

	#endregion

	}
}