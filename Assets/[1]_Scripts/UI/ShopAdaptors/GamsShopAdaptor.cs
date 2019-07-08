using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks
{
    public class GamsShopAdaptor : AdaptorScrollView
    {
    
        //модель данных
        public class ItemModelGams
        {            
            public string nameProduct;
            public Sprite iconMoney;            
            public int countItem;
            public double cashValue; //cтоимость в реальных деньгах
        }

        
        //отображение данных
        public class ItemViewGams
        {
            public Image iconItem;
            public Text countItemText;
            public Text cashValueText;
            public Button clickButton;

            public ItemViewGams(Transform rootView)
            {
                iconItem = rootView.Find("Image").GetComponent<Image>();
                countItemText = rootView.Find("TextCount").GetComponent<Text>();
                cashValueText = rootView.Find("TextPrice").GetComponent<Text>();
                clickButton = rootView.transform.Find("Button").GetComponent<Button>();                
            }
        }


    #region Init

        protected override void GetData()
        {
            if (data == null) return;

            prefab = data.gamsItemPrefab;
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
            int modelsCount = data.itemGams.Length;

            //стартуем получени данных и передаём их в OnReceiveModels()
            StartCoroutine( GetItems(modelsCount, result => OnReceiveModels(result)) );
        }


        //обрабатывает полученные данные
        void OnReceiveModels(ItemModelGams[] modelsArray)
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
        void InitializeItemView(GameObject viewGameObject, ItemModelGams model)
        {
            ItemViewGams view = new ItemViewGams(viewGameObject.transform);
            view.iconItem.sprite = model.iconMoney;
            view.countItemText.text = model.countItem.ToString();
            view.cashValueText.text = model.cashValue.ToString() + "$";
            
            //подписываемся на событие нажатия на предмет (кнопку)
            view.clickButton.onClick.AddListener( () => purchaseManager.BuyConsumable(model.nameProduct) );
        }


        //действие по нажатию на на элемент в списке предмет
        void OnClickItem()
        {
            ShopClickButtonEvent?.Invoke();
        }

                
        
        //вытягивает данные из базы
        IEnumerator GetItems(int count, System.Action<ItemModelGams[]> callback)
        {
            yield return new WaitForSeconds(1f);

            var result = new ItemModelGams[count];

            SetResult(ref result);

            //передаём массив данных в метод
            callback(result);
        }


        //заполняет модкль данных        
        void SetResult(ref ItemModelGams[] itemModelArray)
        {
            for (int i = 0; i < itemModelArray.Length; i++)
            {
                itemModelArray[i] = new ItemModelGams();
                itemModelArray[i].nameProduct = data.itemGams[i].name;
                itemModelArray[i].iconMoney = data.itemGams[i].iconMoney;
                itemModelArray[i].countItem = data.itemGams[i].countItem;
                itemModelArray[i].cashValue = data.itemGams[i].costItem;
            }
        }      


    #endregion  

    }
}
