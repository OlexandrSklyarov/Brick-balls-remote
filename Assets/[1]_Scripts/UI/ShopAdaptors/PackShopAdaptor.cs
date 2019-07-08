using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrakeBricks
{
    public class PackShopAdaptor : AdaptorScrollView
    {
    
    //модель данных
        public class ItemModelPack
        {
            public string nameProduct;
            public Sprite icon_0;
            public Sprite icon_1;
            public Sprite icon_2;
            public int countItem_0;
            public int countItem_1;
            public int countItem_2;
            public double cashValue; //cтоимость в реальных деньгах
        }

        
        //отображение данных
        public class ItemViewPack
        {
            public Image iconItem_0;
            public Image iconItem_1;
            public Image iconItem_2;
            public Text countItemText_0;
            public Text countItemText_1;
            public Text countItemText_2;
            public Text cashValueText;
            public Button clickButton;

            public ItemViewPack(Transform rootView)
            {
                iconItem_0 = rootView.Find("Items/Image_0").GetComponent<Image>();
                iconItem_1 = rootView.Find("Items/Image_1").GetComponent<Image>();
                iconItem_2 = rootView.Find("Items/Image_2").GetComponent<Image>();
                countItemText_0 = rootView.Find("Items/Text_0").GetComponent<Text>();
                countItemText_1 = rootView.Find("Items/Text_1").GetComponent<Text>();
                countItemText_2 = rootView.Find("Items/Text_2").GetComponent<Text>();
                cashValueText = rootView.Find("Price/CostText").GetComponent<Text>();
                clickButton = rootView.transform.Find("Button").GetComponent<Button>();                 
            }
        }


    #region Init

        protected override void GetData()
        {
            if (data == null) return;

            prefab = data.packItemPrefab;
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
            int modelsCount = data.itemPacks.Length;

            //стартуем получени данных и передаём их в OnReceiveModels()
            StartCoroutine( GetItems(modelsCount, result => OnReceiveModels(result)) );
        }


        //обрабатывает полученные данные
        void OnReceiveModels(ItemModelPack[] modelsArray)
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
        void InitializeItemView(GameObject viewGameObject, ItemModelPack model)
        {
            ItemViewPack view = new ItemViewPack(viewGameObject.transform);
            view.iconItem_0.sprite = model.icon_0;
            view.iconItem_1.sprite = model.icon_1;
            view.iconItem_2.sprite = model.icon_2;
            view.countItemText_0.text = model.countItem_0.ToString();
            view.countItemText_1.text = model.countItem_1.ToString();
            view.countItemText_2.text = model.countItem_2.ToString();
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
        IEnumerator GetItems(int count, System.Action<ItemModelPack[]> callback)
        {
            yield return new WaitForSeconds(1f);

            var result = new ItemModelPack[count];

            SetResult(ref result);

            //передаём массив данных в метод
            callback(result);
        }


        //заполняет модкль данных        
        void SetResult(ref ItemModelPack[] itemModelArray)
        {
            for (int i = 0; i < itemModelArray.Length; i++)
            {
                itemModelArray[i] = new ItemModelPack();
                itemModelArray[i].nameProduct = data.itemPacks[i].name;
                itemModelArray[i].icon_0 = data.itemPacks[i].icon_0;
                itemModelArray[i].icon_1 = data.itemPacks[i].icon_1;
                itemModelArray[i].icon_2 = data.itemPacks[i].icon_2;
                itemModelArray[i].countItem_0 = data.itemPacks[i].countItem_0;
                itemModelArray[i].countItem_1 = data.itemPacks[i].countItem_1;
                itemModelArray[i].countItem_2 = data.itemPacks[i].countItem_2;
                itemModelArray[i].cashValue = data.itemPacks[i].costItem;
            }
        }      


    #endregion

    }
}

