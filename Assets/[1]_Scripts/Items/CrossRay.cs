using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
    public class CrossRay : ItemBrick
    {
			protected override void DestroyEffect()
			{
				base.DestroyEffect();

				var effect = Instantiate(data.crossBlastEffect, transform.position, Quaternion.identity) as GameObject;
				Destroy(effect, data.timeDeleyOnDestroyBlastEffect);
				AudioManager.Instance.Play(StaticPrm.SOUND_FIRE_EXPLOSION_BRICK);
			}


			void OnDestroy()
			{	
				StopAllCoroutines();
				bricksController.RemoveItem(this);
				bricksController.DestroyCrosslLine(transform.position.x, transform.position.y);					
			}


			protected override void SetColor()
			{
				//оставляем пустой метод, чтоб не менять цвет
			}     
    }
}