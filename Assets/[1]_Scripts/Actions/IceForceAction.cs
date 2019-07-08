using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{   
    public class IceForceAction : ActionGame
    {
        
        #region Event

			public override event Action<GameActionType> AddItemToAchievEvent;

        #endregion


        #region Init

        public override void InitAction()
        {
            TypeAction = GameActionType.ICE_FORCE;
            LastCompleteLevelIndex = 0; 
			UpdateItemCount();			
            Reset();
        }	


	#endregion

		//обновляет количество использования GameActions
		public override void UpdateItemCount()
		{
			if (GameManager.Instance.ModeGame == GameMode.MULTIPLAYER)
			{
				ItemCount = (GameManager.Instance.MP_GameActions.ContainsKey(TypeAction)) ?
					GameManager.Instance.MP_GameActions[TypeAction] : 0;
			}
			else
			{
				//получаем данные о количестве по своему индексу
				ItemCount = (GameManager.CurrentGame.actionItems.ContainsKey(TypeAction)) ?
					GameManager.CurrentGame.actionItems[TypeAction] : 0;
			}
			
			if (ItemCount > 0)
				Activate();
			else
				Deactivate();
		}	
		

        //реакция на событие старта игры
		public override void StartGameReaction()
        {
			UpdateItemCount();			

			int curLevel = GameManager.Instance.CurrentLevel.Index;

			//если текущий уровень идет не подряд  с предведущим
			//то сбрасываем достижения
			if (curLevel-1 != LastCompleteLevelIndex)
			{
				LastCompleteLevelIndex = curLevel-1;
				Reset();
			}				
		}


        //реакция на событие прохождения уровня
        public override void GameWinReaction()
		{
			AddCounter();
		}
        

		//увеличивает счетчмк предметов, ели достигли нужного уровня
		protected override void AddItemToAchiev()
		{
			if (СountCompletedLevels >= data.iceMaxCompletedLevels)
			{
				AddItemToAchievEvent?.Invoke(TypeAction);
				Reset();
			}
		}



         //сбрасывает значения по умолчанию
		protected override void Reset()
		{		
			//сьрасываем счётчик пройденых уровней на стартовое значение
			СountCompletedLevels = data.iceStartValue;
		}
        

        
    }
}