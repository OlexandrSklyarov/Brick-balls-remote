using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
	[CreateAssetMenu(menuName = "Data/DataContentScrolling", fileName = "DataContentScrolling")]
	public class DataContentScrolling : ScriptableObject
	{

		public GameObject prefamPanelLevel; //префаб панельки уровня
		public int countHorizontal; //количество панелек по горизонтали
		
		public float maxDist; //максю дист. до панельки с уровнем

		
		
		
	}
}