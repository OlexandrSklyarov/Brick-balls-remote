using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
	[System.Serializable]
	public class Game
	{	

	#region Var		

		public DemoGame Demo;
		public GameLevel[] Levels;
		public Dictionary<GameActionType,int> actionItems; // количеством предметов для каждого Actions
		public Statistic StatisticGame;
		public List<int> BoughtBalls; //список мячей, которые купил игрок 

		public string DateLastVisit; //дата последнего запуска игры

		public int GameScore;
		public int HiscoreClassicMode;

		public int MoneyCount; //количество денег в игре (рубинов)
		public int RatingCount; //рейтинг игрока	
		public int RewardScale; //шкала вознаграждения (прибавляется текуйщий рейтинг игрока)
		
		public float ContentAnchorPosition_Y;// позиция списка с уровнями по Y 		

		public bool IsSoundPlay; //проигрывается ли звук в игре

		public int CurrentBallSpriteIndex; //индекс спрайта мяча, который выбра игрок
		

	#endregion


		public Game()
		{			
			InitActionForce();
			InitBoughtBalls();
			InitDemoGame();			

			StatisticGame = new Statistic();
			
			DateLastVisit = DateTime.Now.ToString(); //сохраняем текущую дату

			IsSoundPlay = true; //по умолчанию звук проигрывается

			Init(GameManager.Config.countLevels);
		}


		void InitActionForce()
		{
			actionItems = new Dictionary<GameActionType,int>();	
			actionItems.Add(GameActionType.ICE_FORCE, 0);	
			actionItems.Add(GameActionType.TIME_BOMB, 0);
		}

		void InitBoughtBalls()
		{
			BoughtBalls = new List<int>();
			BoughtBalls.Add(0);
		}


		void Init(int countLevels)
		{
			Levels = new GameLevel[countLevels];

			for (int i = 0; i < Levels.Length; i++)
			{
				Levels[i].Index = i+1;
			}

			ContentAnchorPosition_Y = 0f;			
		}	


		void InitDemoGame()
		{
			Demo = new DemoGame();
			Demo.GameLaunchDate = DateTime.Now.ToString(); //сохраняем текущую дату
			Demo.MaxLaunchDays = 5;	
		}


		//загружает сохранённые данные
		public void LoadGame(Save save)
		{
			LoadLevels(save.levelSave);
			LoadActionsForce(save.actionsForce);

			BoughtBalls = save.BoughtBalls;
			DateLastVisit = save.DateLastVisit;
			StatisticGame = save.StatisticGame;
			GameScore = save.GameScore;
			HiscoreClassicMode = save.HiscoreClassicMode;
			MoneyCount = save.MoneyCount;
			RatingCount	= save.RatingCount;
			RewardScale = save.RewardScale;		
			ContentAnchorPosition_Y = save.ContentAnchorPosition_Y;
			IsSoundPlay = save.IsSoundPlay;
			CurrentBallSpriteIndex = save.CurrentBallSpriteIndex;
			Demo = save.Demo;
		}


		void LoadLevels(List<Save.LevelSave> levels)
		{
			Levels = new GameLevel[levels.Count];
			for (int i = 0; i < levels.Count; i++)
			{
				Levels[i].Index = levels[i].index;
				Levels[i].Rating = levels[i].rating;
				Levels[i].Status = levels[i].status;
			}
		}

		void LoadActionsForce(List<Save.ActionsForce> actions)
		{
			actionItems = new Dictionary<GameActionType,int>();	
			for (int i = 0; i < actions.Count; i++)
			{
				actionItems.Add(actions[i].type, actions[i].count);
			}
		}
	}	


	[System.Serializable]
	public struct Statistic
	{
		public int NumWinsMP; //счёт побед в MP
		public int VictoryOneMove; //счёт побед с одного хода
		public int NumMultiplayerLaunches; //количество запусков мультиплеера
		public int NumWinsMultiplayer; //количество побед в мультипплеере	
		public int NumClassicLaunches; //количество запусков классической игры
		public int MaxRatingCount; //количество прохождения уровня с максимальным рейтингом (***)
		public int NumInvitedFriends;//количество приглашенных друзей в мультиплеере
		public int NumUsedIcePower;//количество использования силы льда
		public int NumUsedBombPower; //количество использования бомбы
		public int DailyGameSeries; //количество дней безпрерывной игры (подряд)
		public int NumCompletedLevels; //количество пройденых уровней
		public int NumExtraBalls; //количество собраных мячей  (за всё время)
		public int CountLaunchApp; //количество запусков приложения
	}


	[System.Serializable]
	public struct DemoGame
	{
		public string GameLaunchDate;
		public bool isFirstLaunchGame;
		public int MaxLaunchDays;		
	}
}
