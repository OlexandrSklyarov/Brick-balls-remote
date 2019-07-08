using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks
{
	public abstract class ItemBrick : Item 
	{
		protected BricksController bricksController;
		protected SpriteRenderer spriteRenderer;
		protected Animator animator;
		protected Text textLive;
		protected DataBrick data;
		protected int currentLive;
		protected int startLive;

		protected Coroutine playCoroutine;


		public void Init (BricksController _controller, int lives) 
		{			
			bricksController = _controller;	
			data = Resources.Load<DataBrick>("Data/DataBrick");
			animator = transform.Find("VFX").GetComponent<Animator>();
			gameObject.tag = StaticPrm.TAG_BRICK;
			spriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
			textLive = transform.Find("Canvas/Text").GetComponent<Text>();
			currentLive = lives;
			startLive = currentLive;

			UpdateLiveText();
			//SetColor();
			SetColorRandom();
			
		}	


		//реакция на удары мячиком
		public override void ContactReaction()
		{				
			PlayAnimBound();			
			SetColor();	
			Damage(1);	
			UpdateLiveText();								
		}	


		//реакция на воздействие внешних сил :)
		public void ExternalForceReaction(float damagePercentage)
		{			
			PlayAnimBound();			
			SetColor();

			var damage = (int)(currentLive * damagePercentage);
			//Debug.Log("minus live: " + damage);

			Damage(damage);
			UpdateLiveText();
		}


		void Damage(int damage)
		{
			currentLive -= damage;
			
			if (currentLive <= 0)
			{				
				Explode();
			}
		}


		//обновляет текст счета на предмете
		protected virtual void UpdateLiveText()
		{
			textLive.text = currentLive.ToString();
		}

		
		//изменяет цвет предмету
		protected virtual void SetColor()
		{	 
			Color color = Color.white;
			
			int index = currentLive / data.colors.Length;
			index = Mathf.Clamp(index, 0, data.colors.Length-1);
			color = data.colors[index];			
			
			spriteRenderer.color = color;
		}

		protected virtual void SetColorRandom()
		{	 
			Color color = Color.white;
			
			int index = Random.Range(0, data.colors.Length);			
			color = data.colors[index];			
			
			spriteRenderer.color = color;
		}	


		protected void Explode()
		{
			GameManager.Instance.AddScore(startLive);				
			AudioManager.Instance.Play(StaticPrm.SOUND_DESTROY_BRICK);
			DestroyEffect();
			Destroy(gameObject);
		}

		protected void PlayAnimBound()
		{			
			animator.SetBool(StaticPrm.ANIM_BOOL_BOUND, true);

			if (playCoroutine != null) StopCoroutine(playCoroutine);
			playCoroutine = StartCoroutine( StopAnimBound(0.04f) );			
		}		


		IEnumerator StopAnimBound(float timer)
		{
			yield return new WaitForSeconds(timer);
			animator.SetBool(StaticPrm.ANIM_BOOL_BOUND, false);
		}



		//эффект от взрыва предмета
		protected virtual void DestroyEffect()
		{
			var effect = Instantiate(data.destroyEffect, transform.position, Quaternion.identity) as GameObject;
			effect.transform.parent = bricksController.VFX_conteiner.transform;
			Destroy(effect, data.timeDeleyDestroyEffect);
		}
	}
}