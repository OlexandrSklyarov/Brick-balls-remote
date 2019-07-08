using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks
{	
	public abstract class Item : MonoBehaviour 
	{
		//флаг, пожет ли двигаться предмет
		public bool isItemFixed;

		//реакция предмета на контакт
		public abstract void ContactReaction();	


		//сдвигает предмет вниз
		public void ShiftDown(float sizeStep)
		{
			var posItem = transform.position;
            posItem.y -= sizeStep;
        	transform.position = posItem;
		}
					
	}
}