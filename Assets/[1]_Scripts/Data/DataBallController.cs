using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
	[CreateAssetMenu(menuName = "Data/DataBallController", fileName = "DataBallController")]
	public class DataBallController : ScriptableObject 
	{

		[Header("Prefabs")]
		public GameObject ballPrefab;

		[Space]
		[Header("Settings")]
		public float ballSpeed; //скорость полёта мяча		
		public int startCountBall; //количество мячей при старте
		public float timeDelayPushBall; //задержка пере следующим запуском мяча

		[Space]
		[Header("Settings")]
		public bool isMoveCorrect; //влаг включающий коррекцию при направления при отскоках
		[Range(0.0001f, 1f)]public float corectionValue; //корректировачное значение при отскоке от стенок мячиков

		[Space]
		[Header("Draw Line")]
		public LayerMask borderLeyar; //слой с бортиками

		[Space]
		[Header("VFX")]
		public GameObject takeBallffect; //визуальный эфект при подборе нового мячика
		
	}
}