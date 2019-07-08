using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
	public class ExplosionEffect : MonoBehaviour 
	{
		
		void Start () 
		{
			Invoke("Explosion", 0.5f);
						
		}


		void Explosion()
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_ICE_EXPLOSION);					
			AddExplosionForceChild();			
		}
		

		//применяем силу взрыва к дочерним объектам
		void AddExplosionForceChild()
		{
			foreach (Transform child in transform)
			{
				var body = child.gameObject.GetComponent<Rigidbody>();	
				if (body)
				{
					body.AddExplosionForce(200f, transform.position, 30f);
				}
			}
		}
	}	

		
	
}