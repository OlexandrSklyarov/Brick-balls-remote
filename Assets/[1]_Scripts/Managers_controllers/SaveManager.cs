using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace BrakeBricks
{		
	public static class SaveManager
	{
		static string SAVE_GAME_PATH = Application.persistentDataPath + "/save.gd";

		public static void Save()
		{			
			BinaryFormatter bf = new BinaryFormatter();        				
        	FileStream file = new FileStream(SAVE_GAME_PATH, FileMode.Create);

			var game = GameManager.CurrentGame;	

			Save save = new Save();
			save.SaveGame(game);			

        	bf.Serialize(file, save);
        	file.Close();				
		}


		public static bool Load()
		{			
			if (File.Exists(SAVE_GAME_PATH))
			{
				BinaryFormatter bf = new BinaryFormatter();
				FileStream file = new FileStream(SAVE_GAME_PATH, FileMode.Open);
				
				Save save = (Save)bf.Deserialize(file);	
				GameManager.CurrentGame.LoadGame(save);	

				file.Close();		

				return true;
			}
			
			return false;			
		}
		
	}


	[System.Serializable]
	public class Save
	{

	#region Save struct

		[System.Serializable]
		public struct LevelSave
		{
			public int index;
			public int rating;
			public StatusLevel status;
		}


		[System.Serializable]
		public struct ActionsForce
		{
			public GameActionType type;
			public int count;
		}
	

	#endregion


	#region Var

		public List<LevelSave> levelSave = new List<LevelSave>();
		public List<ActionsForce> actionsForce = new List<ActionsForce>();
		public List<int> BoughtBalls = new List<int>();
		
		public string DateLastVisit;
		public Statistic StatisticGame;		
		public int GameScore;
		public int HiscoreClassicMode;
		public int MoneyCount;
		public int RatingCount; 	
		public int RewardScale;		
		public float ContentAnchorPosition_Y;
		public bool IsSoundPlay; 
		public int CurrentBallSpriteIndex;
		public DemoGame Demo;

	#endregion


	#region Save method

		public void SaveGame(Game game)
		{
			SaveLevels(game.Levels);
			SaveActionsForce(game.actionItems);

			BoughtBalls = game.BoughtBalls;
			DateLastVisit = game.DateLastVisit;
			StatisticGame = game.StatisticGame;
			GameScore = game.GameScore;
			HiscoreClassicMode = game.HiscoreClassicMode;
			MoneyCount = game.MoneyCount;
			RatingCount	= game.RatingCount;
			RewardScale = game.RewardScale;		
			ContentAnchorPosition_Y = game.ContentAnchorPosition_Y;
			IsSoundPlay = game.IsSoundPlay;
			CurrentBallSpriteIndex = game.CurrentBallSpriteIndex;
			Demo = game.Demo;
		}


		void SaveLevels(GameLevel[] levels)
		{		
			foreach(var l in levels)
			{
				var ls = new LevelSave();

				ls.index = l.Index;
				ls.rating = l.Rating;
				ls.status = l.Status;

				levelSave.Add(ls);				
			}
		}


		void SaveActionsForce(Dictionary<GameActionType,int> actionItems)
		{
			
			foreach(var a in actionItems)
			{
				var act = new ActionsForce();

				act.type = a.Key;
				act.count = a.Value;	

				actionsForce.Add(act);
			}
		}

	#endregion
	}
}