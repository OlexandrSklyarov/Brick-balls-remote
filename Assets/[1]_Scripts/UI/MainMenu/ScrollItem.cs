using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks
{
	public class ScrollItem : MonoBehaviour 
	{
	
	#region Var
		
		DataScrollElement data; 
		Image myBackgraund;
		Image lockLevelImage; //пиктограма статуса уровня (закрыт, открыт, пройден)
		Image starRatingImage_0, starRatingImage_1, starRatingImage_2; //картинки рейтинга (звёзды)		
		Text textNumberLevel; //текст индекса		
		StatusLevel statusLevel; //текущий статус уровня
		DynamicGridScrollView scrollView;
		int indexLevel; //индекс панели
		int rating; //рейтинг уровня
		public int IndexLevel {get{return indexLevel;}}

		
		
	#endregion


	#region Init

		//настраивает панельку при создании
		public void Init(int _indexLevel, int _rating, StatusLevel _status)
		{					
			data = Resources.Load<DataScrollElement>("Data/DataScrollElement");
			scrollView = GameObject.FindObjectOfType<DynamicGridScrollView>();
			
			FindUI();
			UpdateInfo(_indexLevel, _rating, _status);
			
		}


		//ищет UI компоненты на панельке
		void FindUI()
		{
			myBackgraund = GetComponent<Image>();

			textNumberLevel = transform.Find("TextNumberLevel").GetComponent<Text>();
			lockLevelImage = transform.Find("ImageLockLevel").GetComponent<Image>();

			starRatingImage_0 = transform.Find("ImageStar_0").GetComponent<Image>();
			starRatingImage_1 = transform.Find("ImageStar_1").GetComponent<Image>();
			starRatingImage_2 = transform.Find("ImageStar_2").GetComponent<Image>();
		}
		

	#endregion


	#region Update

		//обновляет информацию о текущей панельке
		public void UpdateInfo(int _indexLevel, int _rating, StatusLevel _status)
		{
			indexLevel = _indexLevel;
			rating = _rating;
			statusLevel = _status;
			
			textNumberLevel.text = indexLevel.ToString();
			SetImageStatus();	
			//Debug.Log("Scroll El # " + indexLevel + " upadate");		
		}
		

		//устанавливает пиктограму на панельке в зависимости от еестатуса
		void SetImageStatus()
		{
			switch(statusLevel)
			{				
				case StatusLevel.OPEN:
					lockLevelImage.gameObject.SetActive(false);
					RatingImageActive();
					myBackgraund.color = data.OpenColor;
				break;
				case StatusLevel.COMPLETED:
					lockLevelImage.gameObject.SetActive(false);
					RatingImageActive();
					myBackgraund.color = data.ComplitedColor;
				break;
				default:
					lockLevelImage.gameObject.SetActive(true);
					RatingImageHide();
					myBackgraund.color = data.CloseColor;
				break;
			}
		}


		//скрыть картинки рейтинга
		void RatingImageHide()
		{
			starRatingImage_0.gameObject.SetActive(false);
			starRatingImage_1.gameObject.SetActive(false);
			starRatingImage_2.gameObject.SetActive(false);
		}


		//отобразить картинки рейтинга
		void RatingImageActive()
		{
			starRatingImage_0.gameObject.SetActive(true);
			starRatingImage_1.gameObject.SetActive(true);
			starRatingImage_2.gameObject.SetActive(true);

			switch(rating)
			{
				case 1:
					starRatingImage_0.sprite = data.whiteStarImage;
					starRatingImage_1.sprite = data.blackStarImage;
					starRatingImage_2.sprite = data.blackStarImage;
				break;
				case 2:
					starRatingImage_0.sprite = data.whiteStarImage;
					starRatingImage_1.sprite = data.whiteStarImage;
					starRatingImage_2.sprite = data.blackStarImage;
				break;
				case 3:
					starRatingImage_0.sprite = data.whiteStarImage;
					starRatingImage_1.sprite = data.whiteStarImage;
					starRatingImage_2.sprite = data.whiteStarImage;
				break;
				default:
					starRatingImage_0.sprite = data.blackStarImage;
					starRatingImage_1.sprite = data.blackStarImage;
					starRatingImage_2.sprite = data.blackStarImage;
				break;
			}

		}


		public void ClickPanel() 
		{
			if (scrollView.IsScrolling)
				return;
		
			if (statusLevel != StatusLevel.CLOSE)
				InputManager.Instance.PressButton_ChangeLevel(indexLevel);
		}


	#endregion

	}


	
}