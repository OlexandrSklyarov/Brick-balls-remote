using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{	
	public class ActionManager : MonoBehaviour 
	{	

		#region Event

			public event Action<int> Action_1_ActivateEvent; //one			
			public event Action<int> Action_2_ActivateEvent; //two
			public event Action<GameActionType> ExecuteActionEvent; //использует
			public event Action<GameActionType, int> AddActionEvent; //добавляет

		#endregion


		#region Var

			const int ACTION_COUNT = 2;			
			ActionGame[] gameActions;	
			

		#endregion


		#region Init

			void Start()
			{
				AddActions();
				Subscriptions();

				//обновим UI всех кнопок Actions
				foreach (var a in gameActions)
				{
					a.StartGameReaction();
					Action_UpdateUI(a.TypeAction);
				}								
			}


			void AddActions()
			{
				gameActions = new ActionGame[ACTION_COUNT];

				gameActions[0] = new IceForceAction();
				gameActions[1] = new TimeBombAction();			

				//Debug.Log("timeBomb: " + gameActions[0] + " iceForce: " + gameActions[1] );

				//инициализируем actions				
				foreach(var act in gameActions)
				{
					act.InitAction();

					//подписываемся на событие добавления GameAction при достижения нужного уровня
					act.AddItemToAchievEvent += (GameActionType type) => 
					{ 
						//добавляем 1шт нужного типа
						AddActionEvent?.Invoke(type, 1); 
					};
				}					
			}					


			void Subscriptions()
			{
				GameManager.Instance.StartGameEvent += StartGameReaction;
				GameManager.Instance.GameWinEvent += GameWinReaction;
				GameManager.Instance.AddActionItemEvent += UpdateActionsCount;

				InputManager.Instance.PressButton_Action1_Event += Action_1;
				InputManager.Instance.PressButton_Action2_Event += Action_2;				
			}			


			//обработка начала игры
			void StartGameReaction()
			{
				foreach (var a in gameActions)
				{
					a.StartGameReaction();
					Action_UpdateUI(a.TypeAction);
				}				
			}


			//обработка выиграша 
			void GameWinReaction()
			{
				if (GameManager.Instance.ModeGame == GameMode.MULTIPLAYER)
					return;

				foreach (var a in gameActions)
				{
					a.GameWinReaction();
					Action_UpdateUI(a.TypeAction);
				}							
			}

        #endregion


        #region Actions

			//добавляет количество GameActions при покупке, или за просмотр рекламы
			void UpdateActionsCount()
			{
				foreach(var action in gameActions)
				{
					//обновляем количество
					action.UpdateItemCount();

					//обновляем каждую кнопку
					Action_UpdateUI(action.TypeAction);						
				}
			}


        	//обновляет визуальное состояние действия на панеле UI 
        	void Action_UpdateUI(GameActionType type)
			{
				switch(type)
				{
					case GameActionType.ICE_FORCE:
						Action_1_ActivateEvent?.Invoke(gameActions[0].ItemCount);
					break;
					case GameActionType.TIME_BOMB:
						Action_2_ActivateEvent?.Invoke(gameActions[1].ItemCount);
					break;
				}
			}		
		

			// метод для управления из UI
			void Action_1(ActionButton typeActionButton)
			{
				//если нажали запуск
				if (typeActionButton == ActionButton.PUSH)
				{
					ExecuteAction(GameActionType.ICE_FORCE);
					Action_UpdateUI(GameActionType.ICE_FORCE);					
				}				
			}


			// метод для управления из UI
			void Action_2(ActionButton typeActionButton)
			{
				//если нажали запуск
				if (typeActionButton == ActionButton.PUSH)
				{
					ExecuteAction(GameActionType.TIME_BOMB);
					Action_UpdateUI(GameActionType.TIME_BOMB);	
				}				
			}


			//задействует action по типу
			void ExecuteAction(GameActionType type)
			{
				foreach(var action in gameActions)
				{
					if (action.TypeAction == type)
					{
						//если действие можно задействовать
						if (action.ExecuteAction())
							ExecuteActionEvent?.Invoke(action.TypeAction);
					}
				}								
			}			

		#endregion
		
	}

	public enum ActionButton
	{
		PUSH, BUY, SHOW_AD
	}
}