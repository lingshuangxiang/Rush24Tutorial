using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Economy;
using TwentyFour.Scripts.Common;
using TwentyFour.Scripts.Purchase;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

public class StoreProductItem : MonoBehaviour
{
    public Text ProductName;
    public Image Icon;
    public GameObject PurchasableHint;

    public RectTransform SelectedHint;
    public ExpandedProductSDK ProductSDK;
    
    public GameObject ExpriedHint;
    public Text ExpriedHintText;
    
    public GameObject DiscountHint;
    public Text DiscountHintText;
    // Start is called before the first frame update
    public void Init(ExpandedProductSDK itemData)
    {
        ProductSDK = itemData;
        Icon.sprite = InventoryHelper.GetItemIcon(itemData.Rewards[0].Namespace, itemData.Rewards[0].SlugName);
        PurchasableHint.SetActive(!itemData.Purchasable);
        if (itemData.Rewards.Count == 1 && itemData.CustomData.TryGetValue("type",out var type) && type == InventoryHelper.AvatarPartsKey)
        {
            if (InventoryHelper.ExpandedInventoryItemDic.ContainsKey(itemData.Rewards[0].SlugName))
            {
                PurchasableHint.SetActive(true);
            }
        }
        var hasExpiredTime =itemData.EndTime != null;
        ExpriedHint.SetActive(hasExpiredTime);
        if (hasExpiredTime)
        {
            ExpriedHintText.text = TimeConverter.GetTimeAgoOrAfterFormat(itemData.EndTime.Seconds,false);
        }

        if (itemData.CustomData.TryGetValue("discount", out var discount))
        {
            DiscountHintText.text = discount;
            DiscountHint.SetActive(true);
        }
        GetComponent<Button>().onClick.AddListener(ClickEffect);
        

    }

    void ClickEffect()
    {
        BGMManager.Instance.PlayAFX(AFXMusic.ButtonEffectClick);
        foreach (var item in transform.parent.GetComponentsInChildren<StoreProductItem>())
        {
            item.SelectedHint.gameObject.SetActive(false);
        }

        //随机旋转一下z轴
        SelectedHint.transform.DOScale(1.2f, 0.2f).From(1f);
        SelectedHint.transform.DORotate(new Vector3(0, 0, UnityEngine.Random.Range(-10, 10)), 0.2f)
            .From(new Vector3(0, 0, 0));
        SelectedHint.gameObject.SetActive(true);
    }

    void Start()
    {
        //GetComponent<Button>().onClick.AddListener(Purchase);
    }

   

    // Update is called once per frame
    void Update()
    {
        
    }
}
