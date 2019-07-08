using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
	[RequireComponent(typeof(Rigidbody2D))]	
	[RequireComponent(typeof(Collider2D))]	
	public class BallMove : MonoBehaviour 
	{

		enum StateBall
		{
			STAY, MOVE, RETURN
		}

	#region Var

		Vector3 ReturnPosition {get {return ballController.Spawn.ballReturnPoint;}}

		StateBall state; //состояния мяча
		Rigidbody2D rb;	
		Collider2D collider2D;		
		BallController ballController; 	
		bool isGraund; // преземлидся ли мяч	
		bool isLongFly;	
		float addedSpeed;
		
		const float GRAUND_OFFSET = 0.01f;
		const float MIN_DIST = 0.01f;
		const float MOVE_TIME = 7f;
		const float ACCELERATION = 1.2f;
		const float ACCEL_TIME_DELEY = 0.5f;

	#endregion


	#region Event

		public event System.Action BallReturnEvent;
		public event System.Action TakeNewBallEvent;
		public event System.Action<Vector3> LandingBall;

	#endregion


	#region Init

		public void Init(BallController controller)
		{
			if (ballController == null)
				ballController = controller;
			
			addedSpeed = 1f;	
			
		}		
		

		void Awake()
		{
			rb = GetComponent<Rigidbody2D>();	
			collider2D = GetComponent<Collider2D>();		
			state = StateBall.STAY; //состояния мяча

		}

	#endregion


	#region Collision
		
		
		void OnTriggerEnter2D(Collider2D col) 
		{
			// стлкновение с новым мячиком
			if (col.gameObject.tag == StaticPrm.TAG_ITEM_BALL) 
			{
				col.gameObject.GetComponent<Item>().ContactReaction();

				var vfxGO = Instantiate(ballController.Data.takeBallffect, 
							col.transform.position, Quaternion.identity); 

				Destroy(vfxGO, 0.3f);

				if (TakeNewBallEvent != null)
					TakeNewBallEvent();

				AudioManager.Instance.Play(StaticPrm.SOUND_TAKE_ITEM_BALL);	
			}
		}


		void OnCollisionEnter2D(Collision2D col) 
		{
			// стлкновение с кирпичами
			if (col.gameObject.tag == StaticPrm.TAG_BRICK) 
			{
				col.gameObject.GetComponent<Item>().ContactReaction();								
			}

			// стлкновение с землёй
			if (col.gameObject.tag == StaticPrm.TAG_GRAUND) 
			{
			}					
				
			CorectionMove(); //корректировка отскока
			BallAcceleration();
			AudioManager.Instance.Play(StaticPrm.SOUND_CONTACT_SPHERE);			
			
		}
		

	#endregion


	#region Update

		void Update () 
		{
			CheckGraund();	
		}

		
		void FixedUpdate()
		{
			UpdateState();						
		}


		void UpdateState()
		{
			switch(state)
			{
				case StateBall.STAY:
					
				break;
				case StateBall.MOVE:
					if(isGraund)
						state = StateBall.RETURN;					
				break;
				case StateBall.RETURN:
					MoveToStartPosition();
					StopCoroutine(TimerSpeed());															
				break;
			}				
		}


		//коректировка отскока
		void CorectionMove()
		{
			
			if (state == StateBall.MOVE && ballController.Data.isMoveCorrect)
			{
				if (Mathf.Abs(rb.velocity.y) <  ballController.Data.corectionValue)
				{
					if (rb.velocity.y > 0f)
						rb.velocity = new Vector2(rb.velocity.x, ballController.Data.corectionValue);
					else
						rb.velocity = new Vector2(rb.velocity.x, -ballController.Data.corectionValue);
				}	

				if (Mathf.Abs(rb.velocity.x) <  ballController.Data.corectionValue)
				{
					if (rb.velocity.x > 0f)
						rb.velocity = new Vector2(ballController.Data.corectionValue, rb.velocity.y);
					else
						rb.velocity = new Vector2(-ballController.Data.corectionValue, rb.velocity.y);
				}

				if (rb.velocity.magnitude < ballController.Data.ballSpeed)
				{
					NormolizeBallVelocity();
				}				
			}
		}


		//выравнивает текущую скорость
		void NormolizeBallVelocity()
		{
			var velocity = rb.velocity.normalized * (ballController.Data.ballSpeed + addedSpeed);
			rb.velocity = velocity;							
		}


		//ускоряет мячи, если пришло время
		void BallAcceleration()
		{
			//в мультиплеере не включать эту возможность 
			if (GameManager.Instance.ModeGame == GameMode.MULTIPLAYER)
				return;

			if (isLongFly)
			{
				isLongFly = false;

				addedSpeed += ACCELERATION;
				NormolizeBallVelocity();

				StartCoroutine(TimerSpeed());
			}
		}		


		public bool IsReady()
		{
			return state == StateBall.STAY;
		}
		

		//проверка, достиг ли мяч дна игрового уровня
		void CheckGraund()
		{
			if (state == StateBall.MOVE)
			{
				if (transform.position.y < ReturnPosition.y - GRAUND_OFFSET)
				{
					//рассылаем сообщение
					if (LandingBall != null) LandingBall(transform.localPosition);	

					isGraund = true;									
					ResetForce();								
				}				
			}		
		}


		//движение мячика к стартовой позиции
		void  MoveToStartPosition()
		{
			rb.isKinematic = true;
			collider2D.enabled = false;
					
			var startPos = ReturnPosition;
			var dist = Vector3.Distance(transform.position, startPos);

			transform.position = Vector3.Lerp(transform.position, startPos, MOVE_TIME * Time.deltaTime);						
				
			if (dist <= 0.01f) 
			{
				rb.velocity = Vector2.zero;
				rb.isKinematic = false;
				collider2D.enabled = true;

				var tp = transform.position;
				tp = startPos;							
				transform.position = tp;					

				state = StateBall.STAY;

				if (BallReturnEvent != null)
					BallReturnEvent();
			}			
					
		}


		//сброс силы движения мяча
		void ResetForce()
		{
			isLongFly = false;
			addedSpeed = 0f;
			rb.velocity = Vector2.zero;		

			
			var tp = transform.position;
			tp.y = ReturnPosition.y;
			tp.z = 0f;
			transform.position = tp;	
					
		}


		//запуск мячика
		public void Push(Vector2 force)
		{
			if (!IsReady())
				return;
		
			rb.velocity = force;
			isGraund = false;				
			state = StateBall.MOVE;
			StartCoroutine( TimerSpeed() );
		}	


		//задает мячу состояние возврата на старт
		public void BackToStart()
		{
			if (state == StateBall.MOVE)
			{
				state = StateBall.RETURN;

				isGraund = true;	

				isLongFly = false;
				addedSpeed = 1f;
				rb.velocity = Vector2.zero;
			}

			//если мячи не двигаются, но они не на позиции для возврата - меняем состояние на Return
			if (state == StateBall.STAY && Vector3.Distance(transform.position, ReturnPosition) > 0.01f)
			{
				var tp = transform.position;
				tp = ReturnPosition;							
				transform.position = tp;
			}		
		}


		IEnumerator TimerSpeed()
		{
			yield return new WaitForSeconds(ACCEL_TIME_DELEY);
			isLongFly = true;
		}


		void OnDestroy()
		{
			ballController.RemoveBall(this);
		}


		void OnDisable()
		{
			rb.velocity = Vector2.zero;		

			var tp = transform.position;
			tp = ReturnPosition;
			tp.z = 0f;
			transform.position = tp;
			
			state = StateBall.STAY;
		}

	#endregion



	}
}