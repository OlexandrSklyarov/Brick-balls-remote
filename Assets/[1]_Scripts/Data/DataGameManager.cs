using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
	[CreateAssetMenu(menuName = "Data/DataGameManager", fileName = "DataGameManager")]
	public class DataGameManager : ScriptableObject 
	{

		public GameObject levelPrefab; //префаб уровня
		public GameObject  ballControllerPrefab; //контроллер мячей

		[Space]
		[Header("Backgraund level")]
		public Sprite levelsMode_backgraund;
		public Sprite classicMode_backgraund;
		
	}
}
