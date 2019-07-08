using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks
{
	[System.Serializable]
	public class TestScrollItem : MonoBehaviour
	{

	#region Var
		
		DataScrollElement data; 
		Image statusImage; //пиктограма статуса уровня (закрыт, открыт, пройден)
		TextMeshProUGUI textNumberLevel; //текст индекса		
		StatusLevel statusLevel; //текущий статус уровня
		TestItemController content;
		int indexLevel; //индекс панели
		
	#endregion


	#region Init

		//настраивает панельку при создании
		public void Init(int _indexLevel, StatusLevel _status, TestItemController _content)
		{					
			data = Resources.Load<DataScrollElement>("Data/DataScrollElement");
			content = _content;

			FindUI();
			UpdateInfo(_indexLevel, _status);
		}


		//ищет UI компоненты на панельке
		void FindUI()
		{
			textNumberLevel = transform.Find("TextMeshPro Text").GetComponent<TextMeshProUGUI>();
			statusImage = transform.Find("ImageStatusLevel").GetComponent<Image>();
		}
		

	#endregion


	#region Update

		//обновляет информацию о текущей панельке
		public void UpdateInfo(int _indexLevel, StatusLevel _status)
		{
			indexLevel = _indexLevel;
			statusLevel = _status;
			
			textNumberLevel.text = indexLevel.ToString();
			SetImageStatus();	
			//Debug.Log("Scroll El # " + indexLevel + " upadate");		
		}


		//устанавливает пиктограму на панельке в зависимости от еестатуса
		void SetImageStatus()
		{
			Sprite img;

			switch(statusLevel)
			{				
				case StatusLevel.OPEN:
					//img = data.openImage;
				break;
				case StatusLevel.COMPLETED:
					//img = data.completedLevelImage;
				break;
				default:
					img = data.closeImage;
				break;
			}

			//statusImage.sprite = img;
		}


		private void OnMouseUp() 
		{
			if (content.IsScrolling)
				return;
		
			if (statusLevel != StatusLevel.CLOSE)
				InputManager.Instance.PressButton_ChangeLevel(indexLevel);
		}


	#endregion

	}
}
