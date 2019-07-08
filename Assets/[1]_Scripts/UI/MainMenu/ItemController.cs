using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks
{	
	public class ItemController : MonoBehaviour 
	{
	#region Var

		public int CountItems {get{return items.Length;}}

		ScrollItem[] items;

	#endregion

		
		void Awake () 
		{
			items = new ScrollItem[transform.childCount];
			int index = 0;		
			
			foreach(Transform child in transform)
			{
				var item = child.GetComponent<ScrollItem>();
				item. Init(0, 0, StatusLevel.CLOSE);
				items[index] = item;
				++index;
			}
		}
		
		
		public void UpdateItems (GameLevel[] levels) 
		{			
			for(int i = 0; i < levels.Length; i++)
			{				
				items[i].UpdateInfo(levels[i].Index, levels[i].Rating, levels[i].Status);				
			}
		}
	}
}