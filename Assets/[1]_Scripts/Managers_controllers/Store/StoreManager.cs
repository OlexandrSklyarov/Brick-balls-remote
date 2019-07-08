using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

namespace BrakeBricks
{
    public class StoreManager : MonoBehaviour
    {
        
    #region Var

        PurchaseManager purchaseManager; //менеджер покупок
        DataShopItem data;//данные о товарах


    #endregion    


    #region Event

        public event Action<int, int> BuyNewBallEvent; //покупка нового мяча
        public event Action<int> ChangePurchasedBallEvent; //событие на выбор уже купленого товара
        public event Action<int> BuyCoinsEvent; //больше валюты
        public event Action<int, int, int> BuyPackageEvent; //новый пакет 
        public event Action<double> SpendMoneyEvent; //событие совершения покупки на сумму
        public event Action<GameActionType, int, int> BuyGameActionEvent; //покупка Action
        public event Action BuyGameContinueEvent; //покупка game continue

    #endregion


    #region Init

        void Awake()
        {
            GetData();
            AddProductPurchaseManager();
            Subscription();
        }


        void GetData()
        {
            purchaseManager = GetComponent<PurchaseManager>();            
            data = Resources.Load<DataShopItem>("Data/DataShopItem");            
        }


        void AddProductPurchaseManager()
        {           
            Dictionary<string, int> nc_products = new Dictionary<string, int>(); //не многоразовые
            Dictionary<string, int> c_products = new Dictionary<string, int>(); //многоразовые        
          

            //остальные (gams & pack) добавляем в многоразовые товары       
            for (int i = 0 ; i < data.itemGams.Length; i++)
            {
                //вторым параметром записываем порядковый номер предмета
                c_products.Add(data.itemGams[i].name, i);                
            }

            for (int i = 0 ; i < data.itemPacks.Length; i++)
            {
                //вторым параметром записываем порядковый номер предмета
                c_products.Add(data.itemPacks[i].name, i);                
            }            

            //предаём словари с товарами в менеджер покупок
            purchaseManager.Init(nc_products, c_products);

        }


        void Subscription()
        {
            PurchaseManager.OnPurchaseConsumable += Purchase_ConsumableProduct; //покупка многоразового товара
            PurchaseManager.OnPurchaseNonConsumable += Purchase_NonConsumableProduct; //покупка одноразового товара

            //покупка Ice Action
            InputManager.Instance.PressButton_Action1_Event += (ActionButton actButton) => 
            {
                if (actButton == ActionButton.BUY)
                {
                    PurchaseAction(GameActionType.ICE_FORCE);
                }
            };

            //покупка Bomb Action
            InputManager.Instance.PressButton_Action2_Event += (ActionButton actButton) => 
            {
                if (actButton == ActionButton.BUY)
                {
                    PurchaseAction(GameActionType.TIME_BOMB);
                }
            };

            //покупка продолжения игры
            InputManager.Instance.ExLifePanel_PressButtonBuyContinueEvent += PurchaseGameContinue;
        }


    #endregion


    #region Purchase
        

        //обработка покупки многоразовых товаров
        void Purchase_ConsumableProduct(PurchaseEventArgs args)
        {
            var idProduct = args.purchasedProduct.definition.id;

            //проверка на то что продукт GEMS
            bool isGems = false;
            foreach(var g in data.itemGams) 
                if (g.name == idProduct) isGems = true;

            //проверка на то что продукт Pack
            bool isPack = false;
            foreach(var p in data.itemPacks) 
                if (p.name == idProduct) isPack = true;

            if (isGems) //GEMS
            {
                var money = data.itemGams.Single(coins => coins.name == idProduct);  
                if (money != null)  
                {                    
                    BuyCoinsEvent?.Invoke(money.countItem); // событие добавления валюты
                    SpendMoneyEvent?.Invoke(money.costItem); //событие о совершение покупки на сумму
                    AudioManager.Instance.Play(StaticPrm.SOUND_PURCHASE);
                }                               
            }
            else if (isPack) //PACK
            {
                var package = data.itemPacks.Single(p => p.name == idProduct);
                if (package != null)    
                {
                    // событие добавления пака с предметами
                    BuyPackageEvent?.Invoke(package.countItem_0, package.countItem_1, package.countItem_2);
                    SpendMoneyEvent?.Invoke(package.costItem); //событие о совершение покупки на сумму
                    AudioManager.Instance.Play(StaticPrm.SOUND_PURCHASE); 
                }                
            }            
        }       

        

        //обработка покупки одноразовых товаров
         void Purchase_NonConsumableProduct(PurchaseEventArgs args)
        {
            var idProduct = args.purchasedProduct.definition.id;            
            
            Debug.Log(this.name +": Purchase_NonConsumableProduct - NOT IMPLEMENT!!!");           
        } 


        //Покупка 
        public void PurchaseInGameProduct(int indexProduct, bool purchaseStatus)
        {
            var item = data.itemBalls[indexProduct]; 

            if (item == null)
            {
                Debug.Log(string.Format("There is no such product {0}", indexProduct));
                return;
            }

            //если товар куплен - то просто выбираем его
            if (purchaseStatus)
            {
                ChangePurchasedBallEvent?.Invoke(indexProduct);
                ManagerUI.ShowMsg("^_^ Item is buyed ^_^");
                AudioManager.Instance.Play(StaticPrm.SOUND_CLICK_BUTTON);
                
                return;
            }
            else //товар не куплен - пробуем купить, если хватит средств
            {
                var cost = data.itemBalls[indexProduct].costItem; 
                var myMoney = GameManager.CurrentGame.MoneyCount;

                if (myMoney >= cost)
                {
                    var ball = data.itemBalls[indexProduct];                  
                    ball.isBuyed = true; //отмечаем, что товар купили                    
                    BuyNewBallEvent?.Invoke(indexProduct, cost);                    
                    AudioManager.Instance.Play(StaticPrm.SOUND_PURCHASE);
                }             
                else
                {                    
                    ManagerUI.ShowMsg("No gems :(((");
                    AudioManager.Instance.Play(StaticPrm.SOUND_NO_MONEY);
                }       
            }
        }


        //покупка Actions (Ice, Bomb)
        void PurchaseAction(GameActionType type)
        {
            var myMoney = GameManager.CurrentGame.MoneyCount;
            var costAct = GameManager.Config.costAction;

            if (myMoney >= costAct)
            {
                BuyGameActionEvent?.Invoke(type, 1, costAct);                
            }
        }


        //купить continue game
        void PurchaseGameContinue()
        {
            var myMoney = GameManager.CurrentGame.MoneyCount;
            var costContinue = GameManager.Config.costContinueGame;

            if (myMoney >= costContinue)
            {
                BuyGameContinueEvent?.Invoke();                
            }
        }

    #endregion

    }
}