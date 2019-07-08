using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
	[CreateAssetMenu(menuName = "Data/DataMainMenu", fileName = "DataMainMenu")]
	public class DataMainMenu : ScriptableObject 
	{
		public RectTransform сontainerLevelsMenu; //префаб панелек уровней в главном меню
		public int savedNexIndex;
		public float contentAnchoredPosition;
	}
}
