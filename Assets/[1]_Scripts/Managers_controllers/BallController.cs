using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks
{
	public class BallController : MonoBehaviour 
	{

		public class SpawnPoint
		{
			public GameObject go;
			public Vector3 startPosition; //стартовая позиция точки создания шариков
			public Vector3 ballReturnPoint;
			public bool isNewPosition; //флаг, установлена ли новая позиция для spawn point	
		}


		[System.Serializable]
		public class DrawLine
		{
			public LineRenderer lineRenderer; //отрисовка луча полёта мячей
			public Vector2 touchPoint;
			public bool isDrawing;
		}



	#region Var

		public SpawnPoint Spawn {get; private set;} //точка создания шариков
		public DataBallController Data {get; private set;}

		DrawLine draw;

		GameObject canvas;
		Text countBallText; 		
		
		Vector2 pushDirection; //направлеие при стартовом выстреле шариков
		Vector2 ballForce; //сила толчка мячей
		List<BallMove> ballList = new List<BallMove>(); //список всех мячиков
		GameObject conteinerBall; //родительский объект для всех созданых мячей	

		Sprite ballSprite; //спрайт игрового мяча
		
		//максимально возможное колличество созданых мячей
		int maxCountCreateBalls {get {return Data.startCountBall + countExtraBalls;}}
		
		int countBallIsReady; //количество мячей, готовых к запуску		
		int countBallIsMoved; // количество мячей в движении
		int countExtraBalls; //количество дополнительных мячей (собраных во время игры)
		bool isActiveBall; //активированы ли мячи		
		bool isReturnBall; //возвращение мяча вручную

		//запуск мячей через FixedUpdate
		bool isCanPush; //можно ли стартовать мячам
		int countNeedPush; //количество шаров, которое еще нужно запустить
		float timePush; //время с прошлого запуска шарика
				

	#endregion

	#region Event

		public event System.Action BallsToStartPositionEvent; //событие что мячи собрались
		public event System.Action BallsPushEvent; //событие что мячи собрались
		public event System.Action IDestroyEvent; //событие перед уничтожением контроллера

	#endregion
		

	#region Init

		public void Init (Sprite _ballSprite) 
		{
			ballSprite = _ballSprite;
			Data = Resources.Load<DataBallController>("Data/DataBallController");
						
			Subscription();

			//Spawn point
			Spawn = new SpawnPoint();		
			Spawn.go = transform.Find("SpawnPoint").gameObject;
			Spawn.startPosition = Spawn.go.transform.position;
			Spawn.ballReturnPoint = Spawn.startPosition;
			
			conteinerBall = transform.Find("ConteinerBall").gameObject;	

			draw = new DrawLine();
			draw.lineRenderer = transform.Find("LineRenderer").GetComponent<LineRenderer>();

			canvas = Spawn.go.transform.Find("Canvas").gameObject;
			countBallText = canvas.transform.Find("CountBallText").GetComponent<Text>();
			TextCountHide();
			
		}


		void Subscription()
		{
			var inp = InputManager.Instance;
			inp.MoveDirectionEvent += PushBall;	//пуск мяча		
			inp.StartingDirectionEvent += DrawDirectionLine; //отрисовка луча прицеливания
			inp.PressButtonReturnBallEvent += BallsBackForce;	//возврат всех мячей	

			var gm = GameManager.Instance;
			gm.SetNewBallSpriteEvent += UpdateBallSprite;	 //обновляем спрайт всех мячей
		}


		void Unsubscribe()
		{
			var inp = InputManager.Instance;
			inp.MoveDirectionEvent -= PushBall;			
			inp.StartingDirectionEvent -= DrawDirectionLine;
			inp.PressButtonReturnBallEvent -= BallsBackForce;

			var gm = GameManager.Instance;
			gm.SetNewBallSpriteEvent -= UpdateBallSprite;
		}
		
	#endregion
		

	#region Ball

		
		//подготовка мячей к запуску
		public void BallPreparation()
		{
			HideDirectionLine();	

			//проверка, нужно ли создавать новые мячи
			CheckCountBalls();		

			if (!isActiveBall)
				ActivateBall();

			//показываем колличество мячиков
			TextCountActive();	
			TextCountUpdate(ballList.Count);					
		}


		//принудительный возврат мячей
		void BallsBackForce()
		{
			foreach(BallMove ball in ballList)
			{
				ball.BackToStart();
				StopAllCoroutines();
				isReturnBall = true;
				countNeedPush = 0;
			}
		}
		
		
		//вызывается событием возврата одно из мячекй на стартовую позицию
		void  ReturnBall()
		{
			countBallIsReady++;
			countBallIsMoved--;

			TextCountActive();
			TextCountUpdate(countBallIsReady);
			
			
			bool check = (!isReturnBall) ? countBallIsReady >= ballList.Count : countBallIsMoved <= 0;
			
			if (check)
			{					
				//Debug.Log("Return balls - " + countBallIsReady + " list ball - " + ballList.Count);			
				
				countBallIsReady = 0;
				countBallIsMoved = 0;
				isReturnBall = false;	

				//проверяем, нужно ли добавить новые мячи
				CheckCountBalls();
				TextCountUpdate(ballList.Count);			

				if (IsAllBallsReady())
				{
					if (BallsToStartPositionEvent != null)
						BallsToStartPositionEvent();
				}								
			}	
		}


		//устанавливает новую позицию для SpawnPoint
		void SetNewPositionSpawnPoint(Vector3 newPos)
		{
			if (!Spawn.isNewPosition)
			{
				//Debug.Log("Set new  pos for spawnPoint...");

				Spawn.isNewPosition = true;
				var spawnPos = Spawn.go.transform.localPosition;
				spawnPos.x = newPos.x;
				Spawn.go.transform.localPosition = spawnPos;

				Spawn.ballReturnPoint = Spawn.go.transform.position;
			}
		}


		bool IsAllBallsReady()
		{
			int countReady = 0;
			foreach(var b in ballList)
			{
				if (b.IsReady())
					countReady++;
			}

			return countReady == ballList.Count;
		}


		//запуск шариков
		void PushBall(Vector2 direction)
		{	
			HideDirectionLine(); //прячем луч

			if (Input.GetMouseButtonDown(0)) 
				return;			

			if(!GameManager.Instance.IsGameCreate)
				return;


			if (IsAllBallsReady())
			{	
				//проверка, нужно ли создавать новые мячи
				CheckCountBalls();	

				Spawn.isNewPosition = false; //точка может быть по новой установлена		
				SetForce(direction);
				
				TextCountHide();
				//StartCoroutine( StartBall() );
				isCanPush = true;

				//рассылаем, что мячи стартанули
				if (BallsPushEvent != null)
					BallsPushEvent();

			}
			else
			{
				Debug.LogWarning("Ball not start!!!");
			}
		}


		void FixedUpdate()
		{
			if (isCanPush)
			{
				isCanPush = false;
				countNeedPush = ballList.Count;					
				timePush = Time.time;
				//Debug.Log("Set countNeedPush: " + countNeedPush);
			}
			
			//если есть мячи для запуска и пришло нужное время - пускаем
			if(Time.time > timePush + Data.timeDelayPushBall && countNeedPush > 0)
			{
				timePush = Time.time;

				ballList[countNeedPush-1].Push(ballForce);
				countNeedPush--;
				countBallIsMoved++;
				//Debug.Log("Push ball Time: " + Time.time + "needPush: " + countNeedPush);
			}
		}


		void SetForce(Vector2 direction)
		{
			ballForce = direction * Data.ballSpeed;
		}



		//создает новые мячи, если их недостаточно в данный момент
		void CheckCountBalls()
		{
			//проверка, нужно ли создавать новые мячи
			if (ballList.Count < maxCountCreateBalls)
				CreateBalls();
		}


		//корутина запуска шариков
		IEnumerator StartBall()
		{
			int i = 0;			
			
			while(i < ballList.Count)
			{			
				ballList[i].Push(ballForce);
				i++;
				countBallIsMoved++;				
				yield return new WaitForSeconds(Data.timeDelayPushBall); 						
			}	
		}


		//создание шариков
		void CreateBalls()
		{	
			for (int i = ballList.Count; i < maxCountCreateBalls; i++)
			{
				// создать мяч
				var goBall = Instantiate(Data.ballPrefab, Spawn.ballReturnPoint, Quaternion.identity) as GameObject;
					 
				SetBallSprite(goBall);

				//помещаем в контейнер
				goBall.transform.parent = conteinerBall.transform;
				
				var ball = goBall.GetComponent<BallMove>();
				ball.Init(this);
				
				//подписка на события от мячей 
				SubscriptionBallEvent(ball);				

				ballList.Add(ball);	
			}

			//Debug.Log("MAX ball - " + maxCountCreateBalls + " ListBall - " + ballList.Count);
			
		}


		//устанавливает мячу новый спрайт
		void SetBallSprite(GameObject _ball)
		{
			if (ballSprite != null)
			{
				_ball.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = ballSprite;
			}
		}


		//обновляет спрайты всем мячам
		void UpdateBallSprite(Sprite sprite)
		{
			ballSprite = sprite;

			foreach (var ball in ballList)
			{
				SetBallSprite(ball.gameObject);
			}

		}


		void SubscriptionBallEvent(BallMove ball)
		{
			ball.BallReturnEvent += ReturnBall; 
			ball.TakeNewBallEvent += AddExtraBall;
			ball.LandingBall += SetNewPositionSpawnPoint;
		}


		//добавляет допонительный мяч
		void AddExtraBall()
		{
			countExtraBalls++;
			//Debug.Log("ExtraBall: "+ countExtraBalls + " max balls: " + maxCountCreateBalls);
		}


		//удаление мяча
		public void RemoveBall(BallMove ball)
		{
			ball.BallReturnEvent -= ReturnBall;		
			ball.TakeNewBallEvent -= AddExtraBall;

			ballList.Remove(ball);

			if (countExtraBalls > 0)
				countExtraBalls--;			
		}


		//прячем все мячи
		public void Clear()
		{
			DeactivateBall();
		}


		//отключаем все мячи
		void DeactivateBall()
        {
			conteinerBall.SetActive(false);
			isActiveBall = false;
			draw.lineRenderer.gameObject.SetActive(false);	
        }


		//активируем все мячи
		void ActivateBall()
		{
			conteinerBall.SetActive(true);
			isActiveBall = true;			
		}


	#region Draw render line

		//отрисовывает траекторию полёта мячей перед запуском
		void DrawDirectionLine(Vector2 touchPoint)
		{		
			if (!GameManager.Instance.IsGameCreate)
			{		
				HideDirectionLine();		
				return;
			}

			//если мячи стартанули - выходим
			if (!IsAllBallsReady())
			{		
				HideDirectionLine();	
				return;
			}				

			draw.lineRenderer.gameObject.SetActive(true);

			var start = Spawn.ballReturnPoint;
			start.z = 0f;		

			draw.lineRenderer.positionCount = 3;
			draw.lineRenderer.SetPosition(0, start);	

			var dist = 10f;

			var point2 = start + (new Vector3(touchPoint.x, touchPoint.y, 0f) - start).normalized * dist;
			draw.lineRenderer.SetPosition(1, point2);	
	

			RaycastHit2D hit = Physics2D.Linecast(draw.lineRenderer.GetPosition(0), 
												  draw.lineRenderer.GetPosition(1), 
												  Data.borderLeyar);	
			if (hit)
			{	
				var hitPoint3D = new Vector3(hit.point.x, hit.point.y, 0f);
				var hitNorm3D = new Vector3(hit.normal.x, hit.normal.y, 0f);
				var dir = hitPoint3D - draw.lineRenderer.GetPosition(0);

				var reflection = Vector3.Reflect(dir, hitNorm3D);
				draw.lineRenderer.SetPosition(1, hitPoint3D);					

				//CorrectDirectionLine(ref reflection);						

				reflection.Normalize();
				var point_3 = hitPoint3D + reflection;
				
				draw.lineRenderer.SetPosition(2, point_3);				
			}		
		}		


		void CorrectDirectionLine(ref Vector3 reflection)
		{

			if(Mathf.Abs(reflection.x) != Data.corectionValue)
			{
				var x = (reflection.x > 0) ? Data.corectionValue: -Data.corectionValue;
				reflection.x = x;
			}

			if(Mathf.Abs(reflection.y) != Data.corectionValue)
			{
				var y = (reflection.y > 0) ? Data.corectionValue: -Data.corectionValue;
				reflection.y = y;
			}
		}

	#endregion


		void HideDirectionLine()
		{
			//если не null			
			draw.lineRenderer.gameObject?.SetActive(false);
		}


		void OnDestroy()
		{
			Unsubscribe();

			//рассылаем собщение об уничтожении контроллера
			if (IDestroyEvent != null)
				IDestroyEvent();
		}	

	#endregion


	#region Text UI ball

		void TextCountActive()
		{
			canvas.SetActive(true);			
		}


		void TextCountHide()
		{
			canvas.SetActive(false);
		}


		void TextCountUpdate(int countBall)
		{
			countBallText.text = "x " + countBall;
		}

	#endregion

	}
}