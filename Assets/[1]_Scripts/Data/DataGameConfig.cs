using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
	[CreateAssetMenu(menuName = "Data/DataGameConfig", fileName = "DataGameConfig")]
	public class DataGameConfig : ScriptableObject
	{
		public bool isFirstStartGame; //запускалась ли уже игра
		public bool isTestMP; //режим теста мультиплеера
		
		[Space]
		[Header("START LEVEL DELEY")]
		[Range (0.01f, 2f)] public float timeDeleyBallCreate; //задержка до подготовки мячей

		[Space]
		[Header("COUNT LEVELS")]
		[Range(1, 3000)]public int countLevels; //количество уровней в игре
		[Range(1, 3000)]public int openLevels; //количество открытых уровней при старте игры
		
		[Space]
		[Header("MAX REWARD POINT")]
		[Range(0, 3000)]public int maxPointReward; //макс. количество очков рейтинга до награды
		
		[Space]		
		[Header("REWARD PLAYER")]
		[Range(0, 1000)]public int everydayReward; //награда игроку
		[Range(0, 1000)]public int maxScaleReward; //награда игроку
		[Range(0, 1000)]public int videoReward; //награда игроку  за просмотр видео

		[Space]
		[Header("COST GAME ACTION")]
		[Range(0, 1000)]public int costAction; //стоимость покупки Action

		[Space]
		[Header("COST CONTINUE GAME")]
		[Range(0, 1000)]public int costContinueGame; //стоимость продолжения игры
	}
}
