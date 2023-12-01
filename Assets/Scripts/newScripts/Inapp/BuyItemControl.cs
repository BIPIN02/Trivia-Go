using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BuyItemControl : MonoBehaviour {

    public int index = 1;
    public GameObject priceText;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start() {
        if (GameManager.Instance.IAPControl.controller != null)
        {
            if (this.index == 1)
            {
                priceText.GetComponent<Text>().text = GameManager.Instance.IAPControl.controller.products.WithID(GameManager.Instance.IAPControl.Tweentychecks).metadata.localizedPriceString;
            }
            else if (this.index == 2)
            {
                priceText.GetComponent<Text>().text = GameManager.Instance.IAPControl.controller.products.WithID(GameManager.Instance.IAPControl.FortyFivechecks).metadata.localizedPriceString;
            }
            else if (this.index == 3)
            {
                priceText.GetComponent<Text>().text = GameManager.Instance.IAPControl.controller.products.WithID(GameManager.Instance.IAPControl.Hundredchecks).metadata.localizedPriceString;
            }
            else if (this.index == 4)
            {
                priceText.GetComponent<Text>().text = GameManager.Instance.IAPControl.controller.products.WithID(GameManager.Instance.IAPControl.Tweentybomb).metadata.localizedPriceString;
            }
            else if (this.index == 5)
            {
                priceText.GetComponent<Text>().text = GameManager.Instance.IAPControl.controller.products.WithID(GameManager.Instance.IAPControl.FortyFivebomb).metadata.localizedPriceString;
            }
            else if (this.index == 6)
            {
                priceText.GetComponent<Text>().text = GameManager.Instance.IAPControl.controller.products.WithID(GameManager.Instance.IAPControl.Hundredbomb).metadata.localizedPriceString;
            }
        }

    }

    public void buyItem() {
        GameManager.Instance.IAPControl.OnPurchaseClicked(index);
    }
}
