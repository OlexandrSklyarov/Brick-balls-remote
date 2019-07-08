using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
	public class ContentScrolling : MonoBehaviour 
	{

	#region Var

		public bool IsScrolling {get; private set;} //производиться ли сейчас скролинг (прокрутка меню)

		DataContentScrolling data;		
		ScrollItem[] scrollItems;
		Vector2[] itemsPos;
		Vector3 centrScrollView; //центр меню прокрутки

		RectTransform contentRect;

		int maxCountLevels; //количество уровней в игре

		int currentMinIndex;
		int currentMaxIndex;
		int countPanels; 		
		const int  OFFSET_INDEX = 4;
		bool isSetAnchorPos; //установлена ли позиция контента
		

		
	#endregion


	#region Init

		public void Init()
		{
			data = Resources.Load<DataContentScrolling>("Data/DataContentScrolling");	
				
			maxCountLevels = GameManager.CurrentGame.Levels.Length;			
			
			centrScrollView = GameObject.Find("Scroll_View").gameObject.transform.localPosition;

			currentMinIndex = 0;
			currentMaxIndex = 100;
			countPanels = currentMaxIndex - currentMinIndex;

			contentRect = GetComponent<RectTransform>();
									
			CreateContent();
			StartingGetContentAnchorPosition();
			UdateInfo();
			
		}			


		void CreateContent()
		{
			var levels = GameManager.CurrentGame.Levels;

			scrollItems = new ScrollItem[levels.Length];
			itemsPos = new Vector2[levels.Length];			
			
			for (int i = 0; i < scrollItems.Length; i++)
			{			

				var panel = Instantiate(data.prefamPanelLevel, transform, false) as GameObject;
				
				var element = panel.GetComponent<ScrollItem>();
				element.Init(levels[i].Index, levels[i].Rating, levels[i].Status);
				scrollItems[i] = element;	
				itemsPos[i] = element.transform.localPosition;														
			}
		}		
	

	#endregion


	#region SET / Get Anchor position

		void GetContentAnchorPosition()
		{
			GameManager.CurrentGame.ContentAnchorPosition_Y = contentRect.anchoredPosition.y;
		}


		public void SetContentAnchorPosition()
		{
			var y = GameManager.CurrentGame.ContentAnchorPosition_Y;;
			contentRect.anchoredPosition = new Vector3(0f, y, 0f);
		}

		IEnumerator StartingGetContentAnchorPosition()
		{		
			yield return new WaitForSeconds(0.01f);	

			if (GameManager.CurrentGame.ContentAnchorPosition_Y == 0)
			{
				var y = contentRect.sizeDelta.y / 2f;
				Debug.Log("contentRect.sizeDelta - " + contentRect.sizeDelta);
				GameManager.CurrentGame.ContentAnchorPosition_Y  = y;

				SetContentAnchorPosition();
			}
		}

	#endregion


	#region Update

		void Update() 
		{
			if (!isSetAnchorPos)
			{
				isSetAnchorPos = !isSetAnchorPos;
				StartCoroutine( StartingGetContentAnchorPosition() );
			}
			
			
			
			if (!IsScrolling)
				return;

			//SetActivePanels();	
					
		}


		public void UdateInfo()
		{
			var levels = GameManager.CurrentGame.Levels;
			int num = 0;
			for(int i = currentMinIndex; i < currentMaxIndex; i++)
			{
				scrollItems[num].UpdateInfo(levels[i].Index, levels[i].Rating, levels[i].Status);
				num++;
			}		

			SetContentAnchorPosition();	
		}


		public void Scrolling(bool _isScrolling)
		{
			IsScrolling = _isScrolling;	
			GetContentAnchorPosition();		
		}


	#endregion


	#region Scrolling_1

		void SetActivePanels()
		{			
			var curPosY = transform.position.y;
			var dist = Vector3.Distance(centrScrollView, transform.localPosition);

			if (dist > data.maxDist)
			{
				if (curPosY < 0)
				{
					ShiftIndicesUp();					
				}					
				else	
				{
					ShiftIndicesDown();					
				}

				SetActiveContent();
			}		
		}



		//перебирает весь список с панельками и включает только те, 
		//которые попадают в диапазон видимости
		void SetActiveContent()
		{	
			for (int i = 0; i < scrollItems.Length-1; i++)
			{	
				if (i >= currentMinIndex && i <= currentMaxIndex-1)
					scrollItems[i].gameObject.SetActive(true);
				else
					scrollItems[i].gameObject.SetActive(false);																				
			}
		}


		void ShiftIndicesUp()
		{			
			currentMinIndex += OFFSET_INDEX;
			currentMaxIndex += OFFSET_INDEX;

			ClampedIndex();
		}


		void ShiftIndicesDown()
		{			
			currentMinIndex -= OFFSET_INDEX;
			currentMaxIndex -= OFFSET_INDEX;

			ClampedIndex();			
		}


		void ClampedIndex()
		{
			currentMinIndex = Mathf.Clamp(currentMinIndex, 0, maxCountLevels- countPanels);
			currentMaxIndex = Mathf.Clamp(currentMaxIndex, countPanels, maxCountLevels);
		}

	#endregion


	#region Scrolling_2


	#endregion

	}
}