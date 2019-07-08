using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks
{
    public class BallShopAdaptor : AdaptorScrollView
    {     
         //модель данных
        public class ItemModelBall
        {
            public int index;
            public string nameProduct;
            public Sprite iconItem;
            public Sprite iconMoney;           
            public int costMoney;              
            public DataShopItem.ShopItem_Ball item;
            
        }

        
        //отображение данных
        public class ItemViewBall
        {
            public Image iconItem;
            public Image iconMoney;           
            public Text costMoneyText;
            public Button clickButton;

            public ItemViewBall(Transform rootView)
            {
                iconItem = rootView.Find("Image").GetComponent<Image>();
                iconMoney = rootView.Find("Price/Image").GetComponent<Image>();               
                costMoneyText = rootView.Find("Price/CostText").GetComponent<Text>();
                clickButton = rootView.transform.Find("Button").GetComponent<Button>();                
            }
        }


    #region Init

        protected override void GetData()
        {
            if (data == null) return;

            prefab = data.ballItemPrefab;
            content = this.GetComponent<RectTransform>();
        }

    #endregion


    #region Event

        //событие нажатия кнопки на какой-то из товаров в магазине
        public event System.Action ShopClickButtonEvent;

    #endregion


     #region Update 

        //обновляет все данные в списке
        protected override void UpdateItems()
        {   
            int modelsCount = data.itemBalls.Length;

            //стартуем получени данных и передаём их в OnReceiveModels()
            StartCoroutine( GetItems(modelsCount, result => OnReceiveModels(result)) );
        }


        //обрабатывает полученные данные
        void OnReceiveModels(ItemModelBall[] modelsArray)
        {
            //чистим список от старых компонентов
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            //создаем и заполняем новые компоненты
            foreach (var model in modelsArray)
            {
                var instance = Instantiate(prefab.gameObject) as GameObject;
                instance.transform.SetParent(content, false);
                InitializeItemView(instance, model);
            }
        }


        //заполняет созданый визуальный элемент списка данными (иконки, названия)
        void InitializeItemView(GameObject viewGameObject, ItemModelBall model)
        {
            ItemViewBall view = new ItemViewBall(viewGameObject.transform);
            view.iconItem.sprite = model.iconItem;
            view.iconMoney.sprite = model.iconMoney;       

            //выставляем текст с ценой
            if (model.item.ballType == BallType.PAID)
            {
                view.costMoneyText.text = model.costMoney.ToString(); //ставим цену мячу
            }
            else if (model.item.ballType == BallType.FREE_WATCH_AD)
            {
                view.costMoneyText.text = "Watch AD";
            }                              

            //меняем цвет фона, если товар куплен
            if (model.item.isBuyed)
            {
                SetColorBackgraund(viewGameObject, data.backgraund_Purchase); 
                view.costMoneyText.text = "Open";
            }  
                        
            //подписываемся на событие нажатия на предмет (кнопку)
            view.clickButton.onClick.AddListener( () => 
            {                
                switch(model.item.ballType)
                {
                    //покупка за валюту
                    case BallType.PAID:   
                    var store = GameManager.Instance.StoreManager;
                    store.PurchaseInGameProduct(model.index, model.item.isBuyed); 
                    break;
                    
                    //за просмотр рекламы
                    case BallType.FREE_WATCH_AD:   
                    var adManager = AdMobManager.Instance;
                    adManager.RewardBallForViewingAd(model.index, model.item.isBuyed, () => 
                    {
                        model.item.isBuyed = true; //выставляем статус - кплен
                    });
                    break;
                }
                
                UpdateItems();             
            });

            //меняем надпись и цвет фона у мяча, который выбрал пользователь
            if (model.index == GameManager.CurrentGame.CurrentBallSpriteIndex)
            {
                SetColorBackgraund(viewGameObject, data.backgraund_SelectedBall); 
                view.costMoneyText.text = "My ball";
            }
        }


        //меняет цвет фона у товара
        void SetColorBackgraund(GameObject viewGameObject, Color color)
        {
            viewGameObject.GetComponent<Image>().color = color;
        }

        
        //действие по нажатию на на элемент в списке предмет
        void OnClickItem()
        {
            ShopClickButtonEvent?.Invoke();            
        }
        
        
        //вытягивает данные из базы
        IEnumerator GetItems(int count, System.Action<ItemModelBall[]> callback)
        {
            yield return new WaitForSeconds(1f);

            var result = new ItemModelBall[count];

            SetResult(ref result);

            //передаём массив данных в метод
            callback(result);
        }


        //заполняет модель данных        
        void SetResult(ref ItemModelBall[] itemModelArray)
        {
            for (int i = 0; i < itemModelArray.Length; i++)
            {
                itemModelArray[i] = new ItemModelBall();                
                data.itemBalls[i].name = "BALL_" + i; //меняем имя в данных
                itemModelArray[i].index = i; //запоминаем индекс мяча               
                itemModelArray[i].nameProduct = data.itemBalls[i].name;
                itemModelArray[i].iconItem = data.itemBalls[i].iconItem;
                itemModelArray[i].iconMoney = data.itemBalls[i].iconMoney;                
                itemModelArray[i].costMoney = data.itemBalls[i].costItem;  
                
                //если такой мяч есть в списке купленых, то отмечаем что мяч купили
                data.itemBalls[i].isBuyed = GameManager.CurrentGame.BoughtBalls.Contains(i); 
                
                itemModelArray[i].item = data.itemBalls[i];//сам объект    
            }
        }      


    #endregion   

    }

    
}