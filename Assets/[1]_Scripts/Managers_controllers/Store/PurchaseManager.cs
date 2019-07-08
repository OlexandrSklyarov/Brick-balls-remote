using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

 
public class PurchaseManager : MonoBehaviour, IStoreListener
{
#region Var

    private static IStoreController m_StoreController;
    private static IExtensionProvider m_StoreExtensionProvider;
    private string nc_currentProductName;
    private string c_currentProductName;
 
    [Tooltip("Не многоразовые товары. Больше подходит для отключения рекламы и т.п.")]
    [SerializeField]    
    Dictionary<string, int> NC_PRODUCTS = new Dictionary<string, int>();

    [Tooltip("Многоразовые товары. Больше подходит для покупки игровой валюты и т.п.")]
    [SerializeField]
    Dictionary<string, int> C_PRODUCTS = new Dictionary<string, int>();

#endregion

 
#region  Event

    /// <summary>
    /// Событие, которое запускается при удачной покупке многоразового товара.
    /// </summary>
    public static event OnSuccessConsumable OnPurchaseConsumable;
    /// <summary>
    /// Событие, которое запускается при удачной покупке не многоразового товара.
    /// </summary>
    public static event OnSuccessNonConsumable OnPurchaseNonConsumable;
    /// <summary>
    /// Событие, которое запускается при неудачной покупке какого-либо товара.
    /// </summary>
    public static event OnFailedPurchase PurchaseFailed;

#endregion

    public void Init(Dictionary<string, int> nc_products, Dictionary<string, int> c_products)
    {
        SetProductByCategory(nc_products, c_products);
        InitializePurchasing();
    }


    //заполняет категории продуктов
    void SetProductByCategory(Dictionary<string, int> nc_products, Dictionary<string, int> c_products)
    {
        NC_PRODUCTS = nc_products;
        C_PRODUCTS = c_products;
    }
 


    /// <summary>
    /// Проверить, куплен ли товар.
    /// </summary>
    /// <param name="id">Индекс товара в списке.</param>
    /// <returns></returns>
    public static bool CheckBuyState(string id)
    {
        Product product = m_StoreController.products.WithID(id);
        if (product.hasReceipt) { return true; }
        else { return false; }
    }

 
    public void InitializePurchasing()
    {
        
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        //добавляем многоразовые товары
        foreach (var s in C_PRODUCTS) 
        {
            builder.AddProduct(s.Key, ProductType.Consumable);
        }
        
        //добавляем товар с одной возможностью купить
        foreach (var s in NC_PRODUCTS) 
        {
            builder.AddProduct(s.Key, ProductType.NonConsumable);
        }        
        
        UnityPurchasing.Initialize(this, builder);
        
    }

 
    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

 
    //покупка многоразового товара
    public void BuyConsumable(string productName)
    {
        if (C_PRODUCTS.ContainsKey(productName))
        {
            c_currentProductName = productName;
            BuyProductID(productName);              
        }       
    }

 
    //покупка одноразового товара
    public void BuyNonConsumable(string productName)
    {
        if (NC_PRODUCTS.ContainsKey(productName))
        {
            nc_currentProductName = productName;
            BuyProductID(productName);                      
        }         
    }

 
    void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(productId);
 
            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                OnPurchaseFailed(product, PurchaseFailureReason.ProductUnavailable);
            }
        }
    }

 
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");
 
        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;
    }

 
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

 
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {    
         if (C_PRODUCTS.Count > 0 && String.Equals(args.purchasedProduct.definition.id, c_currentProductName, StringComparison.Ordinal))
            OnSuccess_Consumable(args);
        else if (NC_PRODUCTS.Count > 0 && String.Equals(args.purchasedProduct.definition.id, nc_currentProductName, StringComparison.Ordinal))
            OnSuccess_NonConsumable(args);
        else Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));

        return PurchaseProcessingResult.Complete;
    }

 
    public delegate void OnSuccessConsumable(PurchaseEventArgs args);
    protected virtual void OnSuccess_Consumable(PurchaseEventArgs args)
    {
        OnPurchaseConsumable?.Invoke(args);
        Debug.Log(C_PRODUCTS[c_currentProductName] + " Buyed!");
    }


    public delegate void OnSuccessNonConsumable(PurchaseEventArgs args);
    protected virtual void OnSuccess_NonConsumable(PurchaseEventArgs args)
    {
        OnPurchaseNonConsumable?.Invoke(args);
        Debug.Log(NC_PRODUCTS[nc_currentProductName] + " Buyed!");
    }


    public delegate void OnFailedPurchase(Product product, PurchaseFailureReason failureReason);
    protected virtual void OnFailedP(Product product, PurchaseFailureReason failureReason)
    {
        PurchaseFailed?.Invoke(product, failureReason);
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }

 
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        OnFailedP(product, failureReason);
    }
}