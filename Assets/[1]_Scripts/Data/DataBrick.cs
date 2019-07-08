using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
	[CreateAssetMenu(menuName = "Data/DataBrick", fileName = "DataBrick")]
	public class DataBrick : ScriptableObject 
	{
		public Color[] colors; 

		[Space]
		[Header("Effects")]
		public GameObject horizontalBlastEffect;
		public GameObject verticalBlastEffect;
		public GameObject crossBlastEffect;
		public GameObject destroyEffect;
		
		public float timeDeleyDestroyEffect;
		public float timeDeleyOnDestroyBlastEffect;
		
	}
}