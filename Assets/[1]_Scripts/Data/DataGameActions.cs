using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
	[CreateAssetMenu(menuName = "Data/DataGameActions", fileName = "DataGameActions")]

	public class DataGameActions : ScriptableObject 
	{
		
		[Space]
		[Header("Time bomb action")]
	
		public GameObject VFX_ExplosionBomb; //эффект взрыва		
		[Range(0.1f, 2f)] public float timeAnimExplosion; //время анимации взрыва
		public int bombMaxCompletedLevels; //количество пройденых уровней для активации
		public int bombStartValue; // стартовый счётчик пройденых уровней


		[Space]
		[Header("Ice force action")]
		
		public GameObject VFX_IceForce; //эффект пременения силы заморозки
		[Range(0.1f, 5f)] public float iceForceTimer; //время действования силы		
		public GameObject VFX_IceForceExplosion; //эффукт разлетевшихся частей от взрыва льдышек		
		[Range(0.1f, 2f)] public float iceExplosionTimer; //время жизни частей от эффекта взрыва
		public int iceMaxCompletedLevels; //количество пройденых уровней для активации
		public int iceStartValue; // стартовый счётчик пройденых уровней
		
	}
}
