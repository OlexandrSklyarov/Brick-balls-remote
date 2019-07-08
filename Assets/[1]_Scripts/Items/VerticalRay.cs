using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks 
{
	public class VerticalRay : ItemBrick 
	{

		protected override void DestroyEffect () 
		{
			base.DestroyEffect ();

			var effect = Instantiate (data.verticalBlastEffect, transform.position, Quaternion.identity) as GameObject;
			Destroy (effect, data.timeDeleyOnDestroyBlastEffect);
			AudioManager.Instance.Play (StaticPrm.SOUND_FIRE_EXPLOSION_BRICK);
		}

		void OnDestroy () 
		{
			StopAllCoroutines ();
			bricksController.RemoveItem (this);
			bricksController.DestroyVerticalLine (transform.position.x);
		}

		protected override void SetColor () {
			//оставляем пустой метод, чтоб не менять цвет
		}
	}
}