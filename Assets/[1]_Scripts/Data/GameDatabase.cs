using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
	[CreateAssetMenu(menuName = "Data/GameDatabase", fileName = "GameDatabase")]
	public class GameDatabase : ScriptableObject 
	{
		[SerializeField]
		bool isInit;
		
		
		[Header("SCORES")]
		public int gameScore;
		public int hiscoreClassicMode;


		[Header("CONTENT ANCHOR POSITION")]
		public Vector3 contentAnchorPosition; //координаты привязки центра контента


		[Header("LEVELS")]
		public GameLevel[] Levels;			

		
		public bool IsBaseInit
		{
			get {return isInit;} 
			private set {isInit = value;}
		}


		public void Init()
		{
			for (int i = 0; i < Levels.Length; i++)
			{
				Levels[i].Index = i+1;
			}

			contentAnchorPosition = Vector3.zero;

			IsBaseInit = true;
		}


	}


}
