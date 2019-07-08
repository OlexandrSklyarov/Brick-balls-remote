using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks
{
    public abstract class AdaptorScrollView : MonoBehaviour
    {
    #region var

        protected RectTransform prefab;
        protected RectTransform content;
        protected DataShopItem data;
        protected PurchaseManager purchaseManager;

    #endregion 


    #region Init
        

        protected void Awake()
        {
            purchaseManager = GameObject.FindObjectOfType<PurchaseManager>();
            data = Resources.Load<DataShopItem>("Data/DataShopItem");
            GetData();            
        }


        //получение данных для заполнения (каждый наследник тянет свои данные: prefab и content)
        protected abstract void GetData();

        protected abstract void UpdateItems();

    #endregion 

    #region Update

        //обновляем данные при активации объекта со списком
        protected void OnEnable()
        {
            UpdateItems();
        }

    #endregion

        
        

    }   
}