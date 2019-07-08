using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks 
{
	public class HorizontaRay : ItemBrick 
	{

		protected override void DestroyEffect () 
		{
			base.DestroyEffect ();

			var effect = Instantiate (data.horizontalBlastEffect, transform.position, Quaternion.identity) as GameObject;
			Destroy (effect, data.timeDeleyOnDestroyBlastEffect);
			AudioManager.Instance.Play (StaticPrm.SOUND_FIRE_EXPLOSION_BRICK);
		}

		void OnDestroy () 
		{
			StopAllCoroutines ();
			bricksController.RemoveItem (this);
			bricksController.DestroyHorizontalLine (transform.position.y);
		}

		protected override void SetColor () 
		{
			//оставляем пустой метод, чтоб не менять цвет
		}
	}
}