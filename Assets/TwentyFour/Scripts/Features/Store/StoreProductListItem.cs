using System;
using System.Collections;
using System.Collections.Generic;
using Economy;
using TwentyFour.Scripts.Category;
using TwentyFour.Scripts.Purchase;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.UI;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFour.Scripts.UI.Store
{
    public class StoreProductListItem : MonoBehaviour
    {
        ExpandedProductSDK ProductSDK;
        public Text ProductName;
        public Text ProductProcess;
        public Text Reward;
        public Image Icon;
        public Image ButtonIcon;
        public Button ActionButton;
        public Text ActionButtonText;

        public GameObject CompletedHint;

        
        void SetParams(ExpandedProductSDK product)
        {
            ProductSDK = product;
            ProductName.text = product.DisplayName;
            ProductProcess.text = $"{product.ConsumeCount}/{product.LimitCount}";
            if (product.Rewards.Count > 0)
            {
                Reward.text = $"x{product.Rewards[0].Quantity}";
                Icon.sprite = InventoryHelper.GetItemIcon(product.Rewards[0].Namespace, product.Rewards[0].SlugName);
            }
        
            ActionButton.interactable = false;
            ActionButton.onClick.RemoveAllListeners();
        }

        public void Init(ExpandedProductSDK product)
        {
            SetParams(product);
            ActionButtonText.text = $"x{product.Costs[0].Quantity}";
            bool canBuy = product.Purchasable;
            ActionButton.interactable = canBuy;
            ActionButtonText.color = Color.white;

            if (canBuy)
            {
                List<string> costList = new List<string>();
                uint currentCount = 0;
                foreach (var cost in product.Costs)
                {
                    if (!InventoryHelper.IsSufficientQuantity(cost.SlugName, cost.Quantity))
                    {
                        costList.Add(cost.SlugName);
                    }
                }

                if (costList.Count > 0)
                {
                    ActionButton.interactable = false;
                    ActionButtonText.color = Color.red;
                }
                
            }
            else
            {
                ActionButton.interactable = false;
                ActionButtonText.text = "已兑换";
            }
            ActionButton.onClick.AddListener(OnActionButtonClick);
        }

        private void OnActionButtonClick()
        {
            if (ProductSDK == null) return;
            var productSDKCost = ProductSDK.Costs[0];
            List<string> costList = new List<string>();
            uint currentCount = 0;
            foreach (var cost in ProductSDK.Costs)
            {
                if (!InventoryHelper.IsSufficientQuantity(cost.SlugName, cost.Quantity))
                {
                    costList.Add(cost.SlugName);
                }
            }

            if (costList.Count > 0)
            {
                UIMessage.Show("钻石不足");
            }
            else
            {
                UIManager.Instance.ShowPopUp("确认兑换",$"确认花费【{productSDKCost.DisplayName}】x{productSDKCost.Quantity} 兑换【{ProductSDK.DisplayName}】吗？",(
                    () =>
                    {
                        StartCoroutine(StartPurchase());
                    }));
            }

            
        }
        private bool canPlayEffect;

        IEnumerator StartPurchase()
        {
            canPlayEffect = true;
            UIManager.Instance.ShowCommonLoading("正在结算");
            var purchaseTask = PurchaseHelper.VirtualPurchase(ProductSDK.SlugName, 1,failedAction: PurchaseFailedAction);
            yield return new WaitUntil(() => purchaseTask.IsCompleted);
            var updateInventory = InventoryHelper.ListPersonaInventory(UpdateInventoryFailedAction);
            yield return new WaitUntil(() => updateInventory.IsCompleted);
            var defalut = CategoryHelper.ListProducts(CategoryHelper.DefaultVITCategory);
            yield return new WaitUntil(()=> defalut.IsCompleted);
            
            Init(CategoryHelper.LocalProducts[CategoryHelper.DefaultVITCategory].Products[0]);

            if (canPlayEffect)
            {
                var itemList = new List<GetItemData>();
                foreach (var reward in purchaseTask.Result.Product.Rewards)
                {
                    var data = new GetItemData();
                    data.DisplayName = reward.DisplayName;
                    data.Slug = reward.SlugName;
                    data.Count = reward.Quantity;
                    data.Namespace = reward.Namespace;
                    itemList.Add(data);
                }
                UIManager.Instance.ShowGetItemPanel(itemList);

            }
        
            UIManager.Instance.HideCommonLoading();
        }
        
        void UpdateInventoryFailedAction(Exception e)
        {
            PassportException passportException = e as PassportException;
            UIMessage.Show(passportException?.ErrorMessage);
        }
        private void PurchaseFailedAction(Exception e)
        {
            canPlayEffect = false;
            UIManager.Instance.HideCommonLoading();
            PassportException passportException = e as PassportException;
            UIMessage.Show(passportException?.ErrorMessage);
        }
    }
}