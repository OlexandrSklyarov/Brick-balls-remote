using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks
{
	[CreateAssetMenu(menuName = "Data/DataScrollElement", fileName = "DataScrollElement")]
	public class DataScrollElement : ScriptableObject
	{

		public Sprite closeImage;
		public Sprite blackStarImage;
		public Sprite whiteStarImage;

		[Space]
		[Header("OPEN / CLOSE colors")]
		public Color OpenColor;
		public Color CloseColor;
		public Color ComplitedColor;
	}
}
