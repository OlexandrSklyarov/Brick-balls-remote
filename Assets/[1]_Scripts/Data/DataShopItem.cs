using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrakeBricks
{
    [CreateAssetMenu(menuName = "Data/DataShopItem", fileName = "DataShopItem")]
    public class DataShopItem : ScriptableObject
    {        

        [System.Serializable]
        public class ShopItem_Ball
        {
            public string name;
            public Sprite iconItem;
            public Sprite iconMoney;           
            public int costItem;
            public bool isBuyed;  // мяч куплен    
            public BallType ballType; //тип мяча (за просмотр рекламы, или за валюту)    
            
        }


        [System.Serializable]
        public class ShopItem_Gams
        {
            public string name;
            public Sprite iconMoney;                    
            public int countItem;
            public double costItem; //cтоимость в реальных деньгах
        }


        [System.Serializable]
        public class ShopItem_Pack
        {
            public string name;
            public Sprite icon_0;
            public Sprite icon_1;
            public Sprite icon_2;
            public int countItem_0;
            public int countItem_1;
            public int countItem_2;
            public double costItem; //cтоимость в реальных деньгах
        }


    #region var

        [Header("Color backgraund ball")]
        public Color backgraund_Purchase; //цвет фона купленого товара        
        public Color backgraund_NonPurchase; //цвет фона НЕ купленого товара
        public Color backgraund_SelectedBall; //цвет выбраного мяча

        
        [Space]
        [Header("Gems items")]
        public RectTransform gamsItemPrefab;
        public ShopItem_Gams[] itemGams;


        [Space]
        [Header("Ball items")]
        public RectTransform ballItemPrefab;
        public ShopItem_Ball[] itemBalls;
        

        [Space]
        [Header("Pack items")]
        public RectTransform packItemPrefab;
        public ShopItem_Pack[] itemPacks;

    #endregion
        
    }

    public enum BallType {FREE_WATCH_AD, PAID}
}