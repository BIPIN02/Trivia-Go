using UnityEngine;
using System.Collections;
using UnityEngine.Purchasing;
using System;

public class IAPController : MonoBehaviour, IStoreListener
{
    public string Tweentychecks = "com.check.tweentychecks";
    public string FortyFivechecks = "com.check.fortyfivechecks";
    public string Hundredchecks = "com.check.hundredchecks";
    public string Tweentybomb = "com.bomb.tweentybomb";
    public string FortyFivebomb = "com.bomb.fortyfivebomb";
    public string Hundredbomb = "com.bomb.hundredbomb";
    public IStoreController controller;
    private IExtensionProvider extensions;

    // Use this for initialization
    void Start()
    {
        //DontDestroyOnLoad(transform.gameObject);
        GameManager.Instance.IAPControl = this;
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(Tweentychecks, ProductType.Consumable);
        builder.AddProduct(FortyFivechecks, ProductType.Consumable);
        builder.AddProduct(Hundredchecks, ProductType.Consumable);
        builder.AddProduct(Tweentybomb, ProductType.Consumable);
        builder.AddProduct(FortyFivebomb, ProductType.Consumable);
        builder.AddProduct(Hundredbomb, ProductType.Consumable);
        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("IAP Initizalization complete!!");
        this.controller = controller;
        this.extensions = extensions;
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        Debug.Log("IAP purchase FAILED!! " + p.ToString());
    }


    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log("IAP Initizalization FAILED!! " + error.ToString());
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("IAP Initizalization FAILED!! " + error.ToString());

    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        Debug.Log("purchase id" + e.purchasedProduct.definition.id);
        if (e.purchasedProduct.definition.id == Tweentychecks)
        {
            UIManager.Instance.userDetail. checkMark +=20;
        }
        else if (e.purchasedProduct.definition.id == FortyFivechecks)
        {
            UIManager.Instance.userDetail.checkMark += 45;
        }
        else if (e.purchasedProduct.definition.id == Hundredchecks)
        {
            UIManager.Instance.userDetail.checkMark += 100;
        }
        else if (e.purchasedProduct.definition.id == Tweentybomb)
        {
            UIManager.Instance.userDetail.bomb += 20;
        }
        else if (e.purchasedProduct.definition.id == FortyFivebomb)
        {
            UIManager.Instance.userDetail.bomb += 45;
        }
        else if (e.purchasedProduct.definition.id == Hundredbomb)
        {
            UIManager.Instance.userDetail.bomb += 100;
        }
        GameManager.Instance.SendDataToServer();
        return PurchaseProcessingResult.Complete;
    }
    public void OnPurchaseClicked(int productId)
    {
        if (controller != null)
        {
            if (productId == 1)
            {
                controller.InitiatePurchase(Tweentychecks);
            }
            else if (productId == 2)
            {
                controller.InitiatePurchase(FortyFivechecks);
            }
            else if (productId == 3)
            {
                controller.InitiatePurchase(Hundredchecks);
            }
            else if (productId == 4)
            {
                controller.InitiatePurchase(Tweentybomb);
            }
            else if (productId == 5)
            {
                controller.InitiatePurchase(FortyFivebomb);
            }
            else if (productId == 6)
            {
                controller.InitiatePurchase(Hundredbomb);
            }
        }
    }
}

