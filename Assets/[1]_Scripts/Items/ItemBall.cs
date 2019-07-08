using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
    public class ItemBall : Item
    {		
		protected BricksController bricksController;
		

		public void Init (BricksController _controller) 
		{	
			bricksController = _controller;
			gameObject.tag = StaticPrm.TAG_ITEM_BALL;	
		}


		public override void ContactReaction()
		{
			GameManager.Instance.ExtraBallAdd();
			Destroy(gameObject);
		}


		public void Disappear() 
		{
			Destroy(gameObject);
		}


		void OnDestroy()
		{						
			bricksController.RemoveItem(this);			
		}
		
    }
}
