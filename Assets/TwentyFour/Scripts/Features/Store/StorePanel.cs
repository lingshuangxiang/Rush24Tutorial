using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using Economy;
using TMPro;
using TwentyFour.Scripts.Category;
using TwentyFour.Scripts.Common;
using TwentyFour.Scripts.Inventory;
using TwentyFour.Scripts.Metrics;
using TwentyFour.Scripts.Purchase;
using TwentyFour.Scripts.RemoteConfig;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#endif
using Logger = Unity.UOS.TwentyFour.Common.Logger;


public class StorePanel : MonoBehaviour
{
    public string DefaultSlugName;
    public string StoreSlugName;
    public GameObject StoreItemPrefab;
    public RectTransform StoreItemContainer;
    public GameObject CategoryItemPrefab;
    public RectTransform CategoriesContainer;
    SearchProductsSDKResponse currentStoreProductList;
    ExpandedProductSDK currentExpandedProduct;
    public Text StoreProductName;
    public Text StoreProductDescription;
    public Text StoreProductRemainCountText;
    public Text StoreExpiredTimeText;
    public Button PurchaseButton;
    public Text CostText;
    public Text PurchaseText;

    public ScrollRect CategoryScrollRect;
    public ScrollRect ItemScrollRect;
    public Image CoinIcon;

    public GameObject EmptyHint;

    public GameObject DefaultStoreIcon;
    public GameObject TournamentStoreIcon;
    public TextMeshProUGUI CoinsText;
    public GameObject StoreCoinIcon;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void LockScroll()
    {
        StopCoroutine(DisableScrollRect());
        StartCoroutine(DisableScrollRect());
    }
    private void OnEnable()
    {
        //StartCoroutine(DisableScrollRect());
        StoreSlugName = DefaultSlugName;
        RefreshCategoryInfos();
        ResetDetail();
        RefreshItemList();
        RefreshCategoryItemList();
        InventoryHelper.OnInventoryUpdated += RefreshCategoryInfos;

#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        WX.OnShow(StartCheckInventoryData);
#endif
    }

    IEnumerator DisableScrollRect()
    {
        CategoryScrollRect.vertical = false;
        ItemScrollRect.vertical = false;
        yield return new WaitForSeconds(0.5f);
        CategoryScrollRect.vertical = true;
        ItemScrollRect.vertical = true;
    }
    private void OnDisable()
    {
        currentExpandedProduct = null;
        InventoryHelper.OnInventoryUpdated -= RefreshCategoryInfos;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        WX.OffShow(StartCheckInventoryData);
#endif
    }

    public void ShowDefault()
    {
        Show(CategoryHelper.DefaultGOLDCategory);
    }
    public void Show(string storeSlugName)
    {
        DefaultSlugName = storeSlugName;
        gameObject.SetActive(true);
    }

    public void RefreshCategoryInfos()
    {
        if (CategoryHelper.LocalCategories.TryGetValue(StoreSlugName, out var category))
        {
            DefaultStoreIcon.SetActive(false);
            TournamentStoreIcon.SetActive(false);
            if (category.CustomData.TryGetValue("coin", out var coin))
            {
                StoreCoinIcon.SetActive(true);
                if (coin == InventoryKeys.Default_Currency)
                {
                    DefaultStoreIcon.SetActive(true);
                }
                else if (coin == InventoryKeys.Tournament_Currency)
                {
                    TournamentStoreIcon.SetActive(true);
                }
                uint currencyCount = 0;
                CoinsText.text = "0";
                foreach (var inventory in InventoryHelper.ExpandedInventoryItems)
                {
                    if (inventory.Resource.ResourceSlug == coin)
                    {
                        currencyCount = inventory.Quantity;
                    }

                }
                CoinsText.text = currencyCount.ToString();
            }
            else
            {
                Logger.LogError("No coin key in category custom data");
                StoreCoinIcon.SetActive(false);
            }
        }
        else
        {
            StoreCoinIcon.SetActive(false);
        }
    }
    void RefreshCategoryItemList()
    {
        foreach (var child in CategoriesContainer.transform.GetComponentsInChildren<StoreCategoryItem>())
        {
            Destroy(child.gameObject);
        }

        var categoryItems = new List<StoreCategoryItem>();
        foreach (var product in CategoryHelper.LocalCategories)
        {
            if (CategoryHelper.LocalProducts.ContainsKey(product.Key) && !CategoryHelper.IgnoredCategories.Contains(product.Key))
            {
                if(CategoryHelper.LocalProducts[product.Key].Products.Count > 0)
                {
                    var item = Instantiate(CategoryItemPrefab, CategoriesContainer).GetComponent<StoreCategoryItem>();
                    item.Init(product.Value,this);
                    categoryItems.Add(item);
                }
            }
        }

        if(EmptyHint)
            EmptyHint.SetActive(categoryItems.Count == 0);
        if (categoryItems.Count > 0)
        {
            bool found = false;
            foreach (var category in categoryItems)
            {
                if (category.CategoryData.SlugName == DefaultSlugName)
                {
                    category.CategoryButton.onClick.Invoke();
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                categoryItems[0].CategoryButton.onClick.Invoke();
            }
        }
    }
    void Purchase()
    {
        if (currentExpandedProduct == null) return;
        var cost = currentExpandedProduct.Costs[0];

        UIManager.Instance.ShowPopUp("确认兑换",$"确认花费【{cost.DisplayName}】x{cost.Quantity} 兑换【{currentExpandedProduct.DisplayName}】吗？",(
            () =>
            {
                StartCoroutine(StartPurchase());
            }));
        
    }
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
    void StartCheckInventoryData(WeChatWASM.OnShowListenerResult result)
    {
        UIManager.Instance.ShowCommonLoading("正在检查数据");
        StartCoroutine(CheckInventoryData());
    }
#endif
    public void ResetDetail()
    {
        currentExpandedProduct = null;
        StoreProductName.text = string.Empty;
        StoreProductDescription.text = string.Empty;
        StoreProductRemainCountText.text = string.Empty;
        StoreExpiredTimeText.text = string.Empty;
        PurchaseButton.gameObject.SetActive(false);
        PurchaseButton.onClick.RemoveAllListeners();
        PurchaseButton.onClick.AddListener(Purchase);
    }
    IEnumerator CheckInventoryData()
    {
        var updateInventory = InventoryHelper.ListPersonaInventory();
        yield return new WaitUntil(() => updateInventory.IsCompleted);
        var defalut = CategoryHelper.ListProducts(CategoryHelper.DefaultGOLDCategory);
        currentStoreProductList = CategoryHelper.LocalProducts[StoreSlugName];
        var tempCurrent = currentExpandedProduct;
        currentExpandedProduct = null;
        foreach (var product in currentStoreProductList.Products)
        {
            if (tempCurrent.SlugName == product.SlugName)
            {
                currentExpandedProduct = product;
                break;
            }
        }
        RefreshInfo(currentExpandedProduct);
        RefreshItemList();
        UIManager.Instance.HideCommonLoading();
    }
    private bool canPlayEffect;

    public StorePanel(TextMeshProUGUI coinsText)
    {
        CoinsText = coinsText;
    }

    IEnumerator StartPurchase()
    {
        canPlayEffect = true;
        UIManager.Instance.ShowCommonLoading("正在结算");
        var purchaseTask = PurchaseHelper.VirtualPurchase(currentExpandedProduct.SlugName, 1,failedAction: PurchaseFailedAction);
        yield return new WaitUntil(() => purchaseTask.IsCompleted);
        MetricsHelper.TrackEvent(MetricsKeys.EVENT_PURCHASE_PRODUCT, new Dictionary<string, object>()
        {
            {MetricsKeys.PARAM_CATEGORY_SLUG,StoreSlugName},
            {MetricsKeys.PARAM_PRODUCT_SLUG,currentExpandedProduct.SlugName},
        });
        var updateInventory = InventoryHelper.ListPersonaInventory(UpdateInventoryFailedAction);
        yield return new WaitUntil(() => updateInventory.IsCompleted);
        var defalut = CategoryHelper.ListProducts(CategoryHelper.DefaultGOLDCategory);

        yield return new WaitUntil(()=>defalut.IsCompleted);
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
        currentStoreProductList = CategoryHelper.LocalProducts[StoreSlugName];
        var tempCurrent = currentExpandedProduct;
        currentExpandedProduct = null;
        foreach (var product in currentStoreProductList.Products)
        {
            if (tempCurrent.SlugName == product.SlugName)
            {
                currentExpandedProduct = product;
                break;
            }
        }
        RefreshInfo(currentExpandedProduct);
        RefreshItemList();
        
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
    public void RefreshItemList()
    {
        
        foreach (var child in StoreItemContainer.transform.GetComponentsInChildren<StoreProductItem>())
        {
            Destroy(child.gameObject);
        }

        if (CategoryHelper.LocalProducts.TryGetValue(StoreSlugName, out var StoreProductList))
        {
            currentStoreProductList = StoreProductList;
            foreach (var product in currentStoreProductList.Products)
            {
                var item = Instantiate(StoreItemPrefab, StoreItemContainer).GetComponent<StoreProductItem>();
                item.Init(product);
                if (currentExpandedProduct != null && product.SlugName == currentExpandedProduct.SlugName)
                {
                    item.SelectedHint.transform.DOScale(1.2f, 0f).From(1f);
                    item.SelectedHint.transform.DORotate(new Vector3(0, 0, UnityEngine.Random.Range(-10, 10)), 0f)
                        .From(new Vector3(0, 0, 0));
                    item.SelectedHint.gameObject.SetActive(true);
                }
                // item.ProductName.text = $"{product.DisplayName}:{product.ConsumeCount}/{product.LimitCount}"; 
                // item.ProductSDK = product;
                item.GetComponent<Button>().onClick.AddListener(() =>
                {
                    currentExpandedProduct = product;
                    RefreshInfo(currentExpandedProduct);
                });
            }
        }
        
    }

    void RefreshInfo(ExpandedProductSDK product)
    {
        if (product == null)
        {
            ResetDetail();
            return;
        }
        PurchaseButton.gameObject.SetActive(true);
        StoreProductName.text = $"{product.DisplayName}";
        string desc = string.Empty;
        if (product.CustomData.TryGetValue("desc", out var descValue))
        {
            desc = descValue;
        }

        if (product.LimitCount > 0)
        {
            StoreProductRemainCountText.gameObject.SetActive(true);
            StoreProductRemainCountText.text = $"剩余兑换次数：{product.LimitCount - product.ConsumeCount}";
        }
        else
        {
            StoreProductRemainCountText.gameObject.SetActive(false);
            StoreProductRemainCountText.text = string.Empty;
        }

        if (product.EndTime != null)
        {
            StoreExpiredTimeText.gameObject.SetActive(true);
            StoreExpiredTimeText.text = $"{TimeConverter.GetTimeAgoOrAfterFormat(product.EndTime.Seconds,false)}后过期";
        }
        else
        {
            StoreExpiredTimeText.gameObject.SetActive(false);
            StoreExpiredTimeText.text = string.Empty;
        }

        bool canBuy = product.Purchasable;
        PurchaseButton.interactable = canBuy;

        if (product.Costs?.Count > 0)
        {
            CoinIcon.sprite = InventoryHelper.GetItemSmallIcon(product.Costs[0].Namespace, product.Costs[0].SlugName);
            CostText.text = $"x{product.Costs[0].Quantity}";
            StoreProductDescription.text = desc;
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
                    PurchaseButton.interactable = false;
                    PurchaseText.text = "代币不足";
                }
                else
                {
                    PurchaseButton.interactable = true;
                    PurchaseText.text = "兑换";
                }
                if (product.Rewards.Count == 1 && product.CustomData.TryGetValue("type",out var type) && type == InventoryHelper.AvatarPartsKey)
                {
                    if (InventoryHelper.ExpandedInventoryItemDic.ContainsKey(product.Rewards[0].SlugName))
                    {
                        PurchaseButton.interactable = false;
                        PurchaseText.text = "已拥有";
                    }
                }
            }
            else
            {
                PurchaseButton.interactable = false;
                PurchaseText.text = "已兑换";
            }
            
        }
        else
        {
            StoreProductDescription.text = string.Empty;
            PurchaseButton.gameObject.SetActive(false);

        }
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
