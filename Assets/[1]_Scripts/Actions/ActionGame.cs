using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{		
	[System.Serializable]
	public abstract class ActionGame //: MonoBehaviour 
	{
       

    #region Var		

		public GameActionType TypeAction {get; protected set;}

		public int СountCompletedLevels {get; protected set;}
		public int LastCompleteLevelIndex {get; protected set;}
        public bool IsActive {get; protected set;}			

		protected DataGameActions data = Resources.Load<DataGameActions>("Data/DataGameActions");  
		public int ItemCount {get; protected set;}//количество возможных использований этого Action

	#endregion


	#region Event

		public abstract event Action<GameActionType> AddItemToAchievEvent;

	#endregion
	
		
		public abstract void InitAction();			
		public abstract void StartGameReaction();	
		public abstract void GameWinReaction();
		//обновляет количество испоьзования
		public abstract void UpdateItemCount();
		

		protected abstract void Reset();			
		protected abstract void AddItemToAchiev();	
		
		


		//Задействует Action
	 	public bool ExecuteAction()
        {
            if (IsActive)
			{	
				СountCompletedLevels = 0;
				ItemCount--;

				if (ItemCount < 1)
					Deactivate();

				return true;
			}

			return false;
        }


		//Активация Action
		protected void Activate()
		{
			IsActive = true;
		}


		//Деактивация Action
		protected void Deactivate()
		{
			IsActive = false;
		}


		//увеличивает счетчик пройденных уровней
		protected void AddCounter()
		{
			int curLevel = GameManager.Instance.CurrentLevel.Index;

			//если новый индекс уровня это следующий за последним записаным, то увеличиваем счётчик
			if (LastCompleteLevelIndex == curLevel-1)
			{
				СountCompletedLevels++;
				LastCompleteLevelIndex = curLevel;

				AddItemToAchiev();							
			}				
			else //просто запоминаем текущий уровень
			{
				LastCompleteLevelIndex = curLevel;
			}			
			//Debug.Log("last save level: " + lastCompleteLevelIndex);	
		}
		
	}	

	[System.Serializable]
	public enum GameActionType
	{
		TIME_BOMB, ICE_FORCE
	}
}