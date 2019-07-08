using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mosframe;

namespace BrakeBricks
{
    public class DynamicGridScrollView : BaseDynamicScrollViev
    {     
    
    #region Var
        
        protected override float contentAnchoredPosition    
		{ 
			get { return -this._contentRect.anchoredPosition.y; } 
			set { this._contentRect.anchoredPosition = new Vector2(this._contentRect.anchoredPosition.x, -value ); } 
		}

        public bool IsScrolling {get; private set;}
	    protected override float contentSize { get { return this._contentRect.rect.height; } }
	    protected override float viewportSize { get { return this._viewportRect.rect.height;} }        
        protected override GameLevel[] Levels {get {return GameManager.CurrentGame.Levels;}}
		
        //[SerializeField] int levelCount;
        //GameLevel[] levelsArray;
       
        const int MAX_ITEM_COUNT = 5;

    #endregion

        protected override void Awake() 
		{
            //InitLevels();
                       
            base.Awake();
            this._direction = Direction.Vertical;
            this._itemSize = this.itemPrototype.rect.height;
        }

        /* 
		void InitLevels()
		{
			levelsArray = new GameLevel[levelCount];	
			for (int i = 0; i < Levels.Length; i++)
			{
				Levels[i].Index = i+1;
				Levels[i].Status = StatusLevel.OPEN;
			}
		}  
        */

        protected override void Start()
        {
            SetItemCount();
            base.Start();
        }


        void SetItemCount()
        {
            //количество item в одной линии 
            countItems = MAX_ITEM_COUNT;
            totalItemCount = Levels.Length / countItems;
        }      


        //публичный метод для изменения флага, что в данный момент происходит прокрутка списка
        public void SetScrolling(bool _isScrolling)
        {
            IsScrolling =_isScrolling;
        }


        public void UpdateItemsOnMenuLoad()
        {
            int index = _nextInsertItemNo;

            foreach (var rect in _containers)
            {
                var item = rect.GetComponent<ItemController>(); 
                if( item != null )
                {
                    var startIndex = (index > 0)? index * countItems : 0;
                    GameLevel[] level = new GameLevel[countItems];

                    int curIndex = 0;
                    for (int i = startIndex; i < startIndex+countItems; i++)
                    {
                        level[curIndex] = Levels[i];
                        curIndex++;
                    }                        

                    item.UpdateItems(level);
                }

                index++; 
            }
        }
    }
    
}