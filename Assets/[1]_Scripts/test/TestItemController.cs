using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks
{
	public class TestItemController : MonoBehaviour 
	{
		[System.Serializable]
		public class ContentData
		{
			public GameLevel[] levelsArray;
			public Queue< TestScrollItem> scrollItems;

			public int countItems = 140;

			const float SIZE = 130f;
			const float OFFSET_CELL = 20f;
			const int WIDTH_COUNT_ITEN = 7;
		

			public ContentData (int countLevels, RectTransform content)
			{
				levelsArray = new GameLevel[countLevels];
				InitLevels();

				var w = content.sizeDelta.x;
				var h = content.sizeDelta.y;
				var countItems = (w / (SIZE + OFFSET_CELL)) * (h / (SIZE + OFFSET_CELL));

				scrollItems = new Queue<TestScrollItem>(); //new TestScrollItem[countLevels + WIDTH_COUNT_ITEN * 2];

			}


			void InitLevels()
			{
				for (int i = 0; i < levelsArray.Length; i++)
				{
					levelsArray[i].Index = i+1;
					levelsArray[i].Status = StatusLevel.OPEN;
				}
			}
		}
		

	#region Var

		public bool IsScrolling {get; private set;}	 

		[SerializeField]
		GameObject itemPrefab;

		RectTransform contentRect;
		ScrollRect scrollRect;
		ContentData data;	
		


	#endregion


	#region Init

		void Start()
		{
			scrollRect = GetComponentInParent<ScrollRect>(); //находим у родителя этот компонент
			contentRect = GetComponent<RectTransform>();
			data = new ContentData(3000, contentRect);

			CreateItems();
		}


		void CreateItems()
		{
			for (int i = 0; i < data.countItems; i++)
			{
				var level = data.levelsArray[i];
				var goItem = Instantiate(itemPrefab, transform.position, Quaternion.identity) as GameObject;
				goItem.transform.parent = gameObject.transform;
				goItem.transform.localScale = Vector3.one;
				
				var item = goItem.GetComponent<TestScrollItem>();
				item.Init(level.Index, level.Status, this);
				data.scrollItems.Enqueue(item);
			}

			UpdateItems();
		}


		void UpdateItems()
		{
		}

	#endregion


	#region Scroll update


		void Update()
		{
			
			Debug.Log("dir scroll: " + GetScrollDirection());

			UpdateContent();
		}


		void UpdateContent()
		{
			if (!IsScrolling) return;

			

			

		}
		

		ScrollDirection GetScrollDirection()
		{
			var vel = scrollRect.velocity;
			ScrollDirection dir;

			if (vel.y == 0)
				dir = ScrollDirection.ZERO;
			else if (vel.y > 0)
				dir = ScrollDirection.UP;
			else
				dir = ScrollDirection.DOWN;

			return dir;
		}


		public void OnScrolling(bool _isScroll)
		{
			IsScrolling = _isScroll;
		}

	#endregion

	}

	public enum ScrollDirection
	{
		UP, DOWN, ZERO
	}
}

