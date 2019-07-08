using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mosframe;

namespace BrakeBricks 
{    
    [RequireComponent(typeof(ScrollRect))]
    public abstract class BaseDynamicScrollViev : UIBehaviour    
    {
        /// <summary> Scroll Direction </summary>
	    public enum Direction 
        {
            Vertical, Horizontal,
        }

    #region Var        
    
        public RectTransform itemPrototype;

	    protected int totalItemCount; // = 3000;	    

        protected abstract float contentAnchoredPosition { get; set; }
	    protected abstract float contentSize { get; }
	    protected abstract float viewportSize { get; }       

        protected Direction _direction = Direction.Vertical;
        protected LinkedList<RectTransform> _containers = new LinkedList<RectTransform>();
        protected float _prevAnchoredPosition = 0;
	    protected int _nextInsertItemNo = 0; //Next item index to be inserted (top / left of viewport)
        protected float _itemSize = -1;
	    protected int _prevTotalItemCount = 103;
        protected RectTransform _viewportRect = null;
        protected RectTransform _contentRect = null;
        
        protected abstract GameLevel[] Levels {get;}
        protected int countItems;
        protected DataMainMenu data;


        bool isLoadContentData;

    #endregion


        // Awake
        protected override void Awake() 
        {
            data = Resources.Load<DataMainMenu>("Data/DataMainMenu");  
            this.itemPrototype = data.сontainerLevelsMenu;

            if( this.itemPrototype == null )
            {
                Debug.LogError( RichText.red(new{this.name,this.itemPrototype}) );
                return;
            }

            base.Awake();

            var scrollRect = this.GetComponent<ScrollRect>();
            this._viewportRect = scrollRect.viewport;
            this._contentRect = scrollRect.content;            
        }


        void LoadContentAnchorPosition()
        {
            contentAnchoredPosition = GameManager.CurrentGame.ContentAnchorPosition_Y;//data.contentAnchoredPosition;
            refresh();

            isLoadContentData = true;
        }


        void SaveContentAnchorPosition()
        {
            GameManager.CurrentGame.ContentAnchorPosition_Y = contentAnchoredPosition;            
        }


        // Start
        protected override void Start ()
	    {            
            this._prevTotalItemCount = this.totalItemCount;

            // instantiate items
            var itemCount = (int)(this.viewportSize / this._itemSize + 2);            
           
            
            for( var i = 0; i < itemCount; ++i ) 
            {
			    var itemRect = Instantiate( this.itemPrototype );
			    itemRect.SetParent( this._contentRect, false );
			    itemRect.name = i.ToString();
			    itemRect.anchoredPosition = (this._direction == Direction.Vertical) ? new Vector2(0, -this._itemSize * i) : 
                                                                                      new Vector2( this._itemSize * i, 0);
                this._containers.AddLast( itemRect );

			    itemRect.gameObject.SetActive( true );

				this.updateItem( i, itemRect.gameObject );                
		    }

            // resize content
			this.resizeContent();    
	    }


        // update
	    private void Update()
	    {
            if (!isLoadContentData)
                LoadContentAnchorPosition();


            if( this.totalItemCount != this._prevTotalItemCount ) {

                this._prevTotalItemCount = this.totalItemCount;

                // check scroll bottom
                var isBottom = false;
                if( this.viewportSize-this.contentAnchoredPosition >= this.contentSize-this._itemSize * 0.1f ) {
                    isBottom = true;
                }

                this.resizeContent();

                // move scroll to bottom
                if( isBottom ) {
                    this.contentAnchoredPosition = this.viewportSize - this.contentSize;
                }

                refresh();                
            }           


            // [ Scroll up ]
		    while( this.contentAnchoredPosition - this._prevAnchoredPosition  < -this._itemSize * 0.5 ) {

                this._prevAnchoredPosition -= this._itemSize;

                // move a first item to last
                var item = this._containers.First.Value;
                this._containers.RemoveFirst();
                this._containers.AddLast(item);

                // set item position
                var pos = this._itemSize * ( this._containers.Count + this._nextInsertItemNo );
			    item.anchoredPosition = (this._direction == Direction.Vertical) ? new Vector2(0, -pos) : new Vector2(pos, 0);

                // update item
                this.updateItem( this._containers.Count+this._nextInsertItemNo, item.gameObject );

			    this._nextInsertItemNo++;                         
                
		    }

            // [ Scroll down ]
            while ( this.contentAnchoredPosition - this._prevAnchoredPosition > 0 ) {

                this._prevAnchoredPosition += this._itemSize;

                // move a last item to first
			    var item = this._containers.Last.Value;
                this._containers.RemoveLast();
                this._containers.AddFirst(item);

                this._nextInsertItemNo--;                             

                // set item position
			    var pos = this._itemSize * this._nextInsertItemNo;
			    item.anchoredPosition = (this._direction == Direction.Vertical) ? new Vector2(0,-pos): new Vector2(pos,0);

                // update item
                this.updateItem( this._nextInsertItemNo, item.gameObject );
		    }

            //Debug.Log("this.contentAnchoredPosition: " + this.contentAnchoredPosition);

            SaveContentAnchorPosition();
	    }
    
        
        // refresh
        private void refresh() 
        {
            var index = 0;
            if( this.contentAnchoredPosition != 0 ) 
            {
                index = (int)(-this.contentAnchoredPosition / this._itemSize);
            }

            foreach( var itemRect in  this._containers ) 
            {
                // set item position
                var pos = this._itemSize * index;
			    itemRect.anchoredPosition = (this._direction == Direction.Vertical) ? new Vector2(0, -pos) : new Vector2(pos, 0);

                this.updateItem( index, itemRect.gameObject );

                ++index;
            }

            this._nextInsertItemNo = index - this._containers.Count;
            this._prevAnchoredPosition = (int)(this.contentAnchoredPosition / this._itemSize) * this._itemSize;
        }

        // resize content

        private void resizeContent() 
        {

            var size = this._contentRect.getSize();
            if( this._direction == Direction.Vertical ) size.y = this._itemSize * this.totalItemCount;
            else                                        size.x = this._itemSize * this.totalItemCount;
            this._contentRect.setSize( size );
        }

        // update item

	    private void updateItem( int index, GameObject itemObj )
	    {
		    if( index < 0 || index >= this.totalItemCount ) 
            {
			    itemObj.SetActive(false);
		    }
		    else 
            {
                itemObj.SetActive(true);		
                
			    var item = itemObj.GetComponent<ItemController>();                
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
		    }
	    }
    }
}