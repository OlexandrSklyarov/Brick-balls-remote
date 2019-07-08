using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
	[CreateAssetMenu(menuName = "Data/DataBrickController", fileName = "DataBrickController")]
	public class DataBrickController : ScriptableObject
	{
		[Header("Items simply bricks")]
		public GameObject quadBrick; //квадрат
		public GameObject hexagonBrick; //шестиугольник
		public GameObject[] triangleBricks; //триугольники

		[Header("Items blast")]
		public GameObject[] blastedBrickPrefabs; //массив блоков, которые взрываются
		
		[Header("Items helpers")]
		public GameObject[] helpBrickPrefabs; //массив блоков, которые помогают в игре

		[Space]
		[Header("Lives")]
		[Range(1, 30)] public int minLives;
		[Range(1, 500)] public int maxLives;
		
		
	}
}