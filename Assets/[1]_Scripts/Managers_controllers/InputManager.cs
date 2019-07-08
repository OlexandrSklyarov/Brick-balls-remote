using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks
{
	public class InputManager : MonoBehaviour 
	{

		enum UserChoice
		{
			MAIN_MENU, RESTART_GAME, QUIT_GAME
		}

	#region Var

		public static InputManager Instance {get; private set;}

		Vector3 StartPoint {get{ return ballController.Spawn.ballReturnPoint; }}
		GameObject pushPoint {get { return ballController.Spawn.go; }}

		Vector2 moveDirection; // стартовое напрвление полёта шариков		
		Vector2 endPoint; //точка куда нажал пользователь
		BallController ballController;		
		const float CORECT_VALUE = 0.25f;

		bool isPressSlider;

		UserChoice userChoise; //выбор пользователя в подменю
				

	#endregion


	#region Event
		
		public event Action<Vector2> MoveDirectionEvent; // событие ввода от пользователя
		public event Action<Vector2> StartingDirectionEvent; // событие прицеливания
		public event Action PressButtonReturnBallEvent; //возврат мячей на позицию
		public event Action<ActionButton> PressButton_Action1_Event; //кнопка действия 1
		public event Action<ActionButton> PressButton_Action2_Event; //кнопка действия 2
		
		public event Action<GameMode> PressButtonGameModeEvent; //клавиша с режимом игры
		public event Action PressButtonMainMenu; //клавиша меню
		public event Action<int> PressButtonChangeLevel; //клавиша выбора уровня
		public event Action PressButtonRetryMultiplayer; //клавиша повторной игры по мультиплееру
		public event Action PressButtonFacebookOpenEvent; //кнопака открытия панели facebook
		public event Action PressButtonQuitGame; //выход из игры
		public event Action PressButtonAchivment; //достижения
		public event Action PressButtonOpenFriendsPanelEvent; //открытие панелы приглашений
		public event Action PressButtonLeaderbord; //таблица лидеров
		public event Action PressButtonWatchVideoAdEvent; //нажатие на кнопку просмотр видео за награду

		public event Action PressButtonNextLevel; //следующий уровень
		public event Action PressButtonRestartLevel; //перезапустить уровень		
		public event Action<bool> PressButtonPause; //пауза в игре
		public event Action<bool> ShowConfirmationPanel; //пауза в игре
		public event Action PressButtonSound; //кнопка звука в игре

		public event Action PressButtonShopOpen; //открытие магазина
		public event Action PressButtonShopClose; //закрытие магазина
		public event Action PressButtonShop_GamsPanel; //отображение панели Gams
		public event Action PressButtonShop_BallPanel; //отображение панели Ball
		public event Action PressButtonShop_PackPanel; //отображение панели Pack
		public event Action<int> ClickRewardPanelEvent; //нажатие на панель награды

		//ExtralifePanel
		public event Action ExLifePanel_PressButtonCloseEvent; //нажатие на кнопку ЗАКРЫТЬ на панели Extralife
		public event Action ExLifePanel_PressButtonVideoAdContinueEvent; //нажатие на ПРОДОЛЖИТЬ за просмотр рекламы
		public event Action ExLifePanel_PressButtonBuyContinueEvent; //нажатие на ПРОДОЛЖИТЬ за внутренюю валюту

		//friends panel
		public event Action FriendsPanel_PressButtonInviteFriendsEvent; //кнопка приглашения друга
		public event Action FriendsPanel_PressButtonInvitationsEvent; //кнопка просмотра приглашений
		public event Action FriendsPanel_PressButtonShareEvent; //кнопка поделиться результатом игры
		public event Action FriendsPanel_PressButtonCloseEvent; //закрытие панели приглашения
		public event Action FriendsPanel_PressButtonLoginEvent; //кнопка входа и выхода из сервисов		

	#endregion

		
	#region Init

		void Awake()
		{
			if (Instance == null)
				Instance = this;
		}


		public void Init (BallController _ballController) 
		{
			ballController = _ballController;
			moveDirection = Vector2.zero;					
			endPoint = Vector2.zero;
		}
		
	#endregion


	#region Touch display
		
		//вызывается при перетаскивании пальца по дисплею
		public void TouchDrag() 
		{
			if (ballController == null) return;			

			endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);	
			SetTargetPoint();
								
		}


		void SetTargetPoint()
		{
			if (StartingDirectionEvent != null)
			{						
				var point = endPoint;
				if (point.y < StartPoint.y + CORECT_VALUE)
				{
					point.y = StartPoint.y + CORECT_VALUE;
				}				
					
				//Debug.Log("Drag input cord: " + point);				

				StartingDirectionEvent(transform.TransformPoint(point));					
			}
		}


		//вызывается при отпускании пальца от дисплея
		public void PointerUp()
		{
			if (ballController == null) return;			
			
			if (endPoint != Vector2.zero )
			{
				moveDirection = GetDirection();		

				if (MoveDirectionEvent != null)
					MoveDirectionEvent(moveDirection);
				
				endPoint = Vector2.zero; //обнуляем
			}	

			ManagerUI.SliderControllValue = 0;
		}

	#endregion


	#region Slider input

		public void SliderDrag(Slider slider)
		{
			if (ballController == null) return;	

			//если слайдер не нажат - выходим
			if (!isPressSlider) return;
			
			var sValue = slider.value / 10f;

			float x = sValue;
			float y = (sValue != 0) ? -Mathf.Abs(sValue) : 0f;
			//Debug.Log(string.Format("x:{0} y:{1}", x, y));

			//endPoint = new Vector2(x, y);
			endPoint = pushPoint.transform.TransformPoint(new Vector2(x, y));

			SetTargetPoint();
		}		


		public void SliderPointerClick()
		{
			isPressSlider = true;
		}


		public void SliderPointerUp(Slider slider)
		{
			PointerUp();
			slider.value = 0f;
			isPressSlider = false;
		}
	
	#endregion


	#region  Update

		//задаем направление
		void SetDirection()
		{			
			if (Input.GetMouseButton(0))
			{
				endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);	

				if (StartingDirectionEvent != null)
				{		
					
					var point = endPoint;
					if (point.y < StartPoint.y + CORECT_VALUE)
						point.y = StartPoint.y + CORECT_VALUE;

					StartingDirectionEvent(transform.TransformPoint(point));					
				}								
			}
			else if (Input.GetMouseButtonUp(0) && endPoint != Vector2.zero )
			{
				moveDirection = GetDirection();		

				if (MoveDirectionEvent != null)
					MoveDirectionEvent(moveDirection);
				
				endPoint = Vector2.zero; //обнуляем
			}		
		}


		Vector2 GetDirection()
		{			
			var dir = new Vector2(endPoint.x, endPoint.y) - new Vector2(StartPoint.x, StartPoint.y);

			if (dir.y < CORECT_VALUE)
				dir.y = CORECT_VALUE;
				
			return dir.normalized;				
		}

	#endregion


	#region User confirmation

		//событие включениия панели выбора
		void ConfirmationPanel(UserChoice choise)
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
			userChoise = choise;
			
			//событие отображения панели подтверждения
			ShowConfirmationPanel?.Invoke(true);
		}


		//подтверждение выбраного пункта меню
		public void PressButton_YES()
		{			
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);

			switch(userChoise)
			{
				case UserChoice.MAIN_MENU:
					PressButtonMainMenu();
				break;	
				case UserChoice.RESTART_GAME:
					PressButton_RestartLevel();
				break;
				case UserChoice.QUIT_GAME:
					PressButton_QuitGame();
				break;
			}
		}


		//отклонение выбора
		public void PressButton_NO()
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
			if (ShowConfirmationPanel != null)
				ShowConfirmationPanel(false);
		}


		//подтверждение выхода в главное меню
		public void MainMenu_Confirm()
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
			ConfirmationPanel(UserChoice.MAIN_MENU);
		}


		//подтверждение перезапуска уровня
		public void RestartGame_Confirm()
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
			ConfirmationPanel(UserChoice.RESTART_GAME);
		}


		//подтверждение выхода из игры
		public void QuitGame_Confirm()
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
			ConfirmationPanel(UserChoice.QUIT_GAME);
		}

	#endregion


	#region Main menu Buttons 

			//рассылает сообщение о нажатии кнопки с режимом игры
			public void PressButton_GameMode_LEVELS()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				if (PressButtonGameModeEvent != null)
					PressButtonGameModeEvent(GameMode.LEVELS);
			}


			public void PressButton_GameMode_CLASSIC()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				if (PressButtonGameModeEvent != null)
					PressButtonGameModeEvent(GameMode.CLASSIC);
			}


			public void PressButton_GameMode_MULTIPLAYER()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButtonGameModeEvent?.Invoke(GameMode.MULTIPLAYER);
			}


			public void PressButton_Facebook()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButtonFacebookOpenEvent?.Invoke();
			}

			
			//главное меню
			public void PressButton_MainMenu()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);

				if (PressButtonMainMenu != null) 
					PressButtonMainMenu();												
			}


			//выбор уровня
			public void PressButton_ChangeLevel(int indexLevel)
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				if (PressButtonChangeLevel != null)
					PressButtonChangeLevel(indexLevel);
			}


			//выход из игры
			public void PressButton_QuitGame()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				if (PressButtonQuitGame != null)
					PressButtonQuitGame();
			}


			//достижения
			public void PressButton_Achivement()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				if (PressButtonAchivment != null)
					PressButtonAchivment();
			}


			//таблица лидеров
			public void PressButton_Leaderbord()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				if (PressButtonLeaderbord != null)
					PressButtonLeaderbord();
			}


			//просмотр видеорекламы за награду
			public void PressButton_WatchVideoAd()
			{
				PressButtonWatchVideoAdEvent?.Invoke();
			}


			//открытие панели приглашений поиграть в мультиплеер
			public void PressButton_OpenFriendsPanel()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButtonOpenFriendsPanelEvent?.Invoke();
			}

	#endregion


	#region Submenu buttons

			//повторная игра в мультиплеер
			public void PressButton_RetryMultiplayer()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButtonRetryMultiplayer?.Invoke();
			}		


			//следующий уровень
			public void PressButton_NextLevel()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButtonNextLevel?.Invoke();
			}


			//перезапуск текущего уровеня
			public void PressButton_RestartLevel()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButtonRestartLevel?.Invoke();
			}


			//продолжение игры
			public void PressButton_ContinueGame()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButtonPause?.Invoke(false);
			}


			//нажатие на кнопку звука в меню
			public void PressButton_Sound()
			{
				if (!GameManager.CurrentGame.IsSoundPlay)
				{
					AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				}					

				PressButtonSound?.Invoke();
			}

	#endregion


	#region Game interface button

			//возврат мячей на позицию
			public void PressButton_ReturnBall()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				if (PressButtonReturnBallEvent != null)
					PressButtonReturnBallEvent();
			}
			

			//активация действия №1
			public void PressButton_Action1_Push()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButton_Action1_Event?.Invoke(ActionButton.PUSH);
			}


			public void PressButton_Action1_Buy()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButton_Action1_Event?.Invoke(ActionButton.BUY);
			}


			public void PressButton_Action1_ShowAd()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButton_Action1_Event?.Invoke(ActionButton.SHOW_AD);
			}


			//активация действия №2
			public void PressButton_Action2_Push()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButton_Action2_Event?.Invoke(ActionButton.PUSH);
			}


			public void PressButton_Action2_Buy()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButton_Action2_Event?.Invoke(ActionButton.BUY);
			}


			public void PressButton_Action2_ShowAd()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButton_Action2_Event?.Invoke(ActionButton.SHOW_AD);
			}


			//меню паузы в игре
			public void PressButton_PauseMenu()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				if (PressButtonPause != null)
					PressButtonPause(true);
			}

	#endregion


	#region Shop buttons			

			//открытие магазина
			public void PressButton_ShopOpen()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButtonShopOpen?.Invoke();
			}


			//закрытие магазина
			public void PressButton_ShopClose()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButtonShopClose?.Invoke();
			}


			//кнопка выбора панели с товарами GAMS
			public void PressButton_Shop_GamsPanel()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButtonShop_GamsPanel?.Invoke();
			}


			//кнопка выбора панели с товарами BALL
			public void PressButton_Shop_BallPanel()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButtonShop_BallPanel?.Invoke();
			}


			//кнопка выбора панели с товарами PACK
			public void PressButton_Shop_PackPanel()
			{
				AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
				PressButtonShop_PackPanel?.Invoke();
			}

	#endregion


	#region Get reward

		//вызывается при нажатии на ранель награды игроку
		public void GetReward(int countReward)
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
			ClickRewardPanelEvent?.Invoke(countReward);
		}

	#endregion


	#region Extralife panel

		//нажатие кнопки закрыть на ExtralifePanel
		public void ExtraLifePanel_Button_Close()
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
			ExLifePanel_PressButtonCloseEvent?.Invoke();
		}


		//нажатие кнопки продолжить игру за просмотр рекламы
		public void ExtraLifePanel_Button_VideoAdContinue()
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
			ExLifePanel_PressButtonVideoAdContinueEvent?.Invoke();
		}


		//нажатие кнопки продолжить игру за внутренюю валюту
		public void ExtraLifePanel_Button_BuyContinue()
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
			ExLifePanel_PressButtonBuyContinueEvent?.Invoke();
		}		

	#endregion


	#region Friends panel

		//нажатие кнопки пригласить друга
		public void FriendsPanel_PressButtonInviteFriends()
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
			FriendsPanel_PressButtonInviteFriendsEvent?.Invoke();
		}


		//нажатие кнопки просмотр приглашений
		public void FriendsPanel_PressButtonInvitations()
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
			FriendsPanel_PressButtonInvitationsEvent?.Invoke();
		}


		//нажатие кнопки поделиться результатом игры
		public void FriendsPanel_PressButtonShare()
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
			FriendsPanel_PressButtonShareEvent?.Invoke();
		}


		//закрытие панели приглашения
		public void FriendsPanel_PressButtonClose()
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
			FriendsPanel_PressButtonCloseEvent?.Invoke();
		}


		//вход и выход из сервисов
		public void FriendsPanel_PressButtonLogin()
		{
			AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
			FriendsPanel_PressButtonLoginEvent?.Invoke();
		}

	#endregion

	}
	
}