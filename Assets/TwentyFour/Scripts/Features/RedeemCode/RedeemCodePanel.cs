using System;
using System.Collections;
using System.Collections.Generic;
using TwentyFour.Scripts.Metrics;
using TwentyFour.Scripts.Quest;
using TwentyFour.Scripts.RemoteConfig;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.UI;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.UI;

public class RedeemCodePanel : MonoBehaviour
{
    public Button PasteButton;
    public InputField InputCodeField;
    // Start is called before the first frame update
    void Start()
    {
        PasteButton.onClick.AddListener(OnPaste);
    }

    void OnPaste()
    {
        CopyPasteUtil.Paste(InputCodeField);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void Redeem()
    {
        if (string.IsNullOrEmpty(InputCodeField.text))
        {
            UIMessage.Show("兑换码为空！");
        }
        else
        {
            StartCoroutine(RedeemItem());
        }
    }
    private bool canPlayGetItemEffect;

    IEnumerator RedeemItem()
    {
        canPlayGetItemEffect = true;
        UIManager.Instance.ShowCommonLoading("正在兑换");
        var redeem = InventoryHelper.RedeemToken(InputCodeField.text, "", RedeemFailed);
        yield return new WaitUntil(() => redeem.IsCompleted);
        var updateInventory = InventoryHelper.ListPersonaInventory();
        yield return new WaitUntil(() => updateInventory.IsCompleted);
        UIManager.Instance.HideCommonLoading();
        if (redeem.Result.TokenInstance.HasInboxMessageId)
        {
            var fetch = InBoxHelper.ReceiveMessages().GetAwaiter();
            yield return new WaitUntil(() => fetch.IsCompleted);
            var data = InBoxHelper.ViewInbox().GetAwaiter();
            yield return new WaitUntil(() => data.IsCompleted);
            UIMessage.Show("兑换成功！已发送到邮箱中！");
        }
        else
        {
            if (canPlayGetItemEffect)
            {
                InputCodeField.text = string.Empty;
                var itemList = new List<GetItemData>();

                foreach (var reward in redeem.Result.TokenInstance.Gifts)
                {
                    var data = new GetItemData();
                    data.DisplayName = reward.Detail.DisplayName;
                    data.Slug = reward.Slug;
                    data.Count = reward.Quantity;
                    data.Namespace = reward.Detail.Namespace;
                    itemList.Add(data);
                    MetricsHelper.TrackEvent(MetricsKeys.EVENT_REDEEM_ITEM,new Dictionary<string, object>()
                    {
                        {MetricsKeys.PARAM_REWARD_TYPE,reward.Slug},
                        {MetricsKeys.PARAM_FROM,redeem.Result.TokenInstance.Id},
                        {MetricsKeys.PARAM_REDEEM_TYPE,"redeem_code"}
                    });
                }
                UIManager.Instance.ShowGetItemPanel(itemList);
            }
        }
        
        
    }
    
    void RedeemFailed(Exception e)
    {
        PassportException passportException = e as PassportException;
        UIMessage.Show(passportException?.ErrorMessage);
        canPlayGetItemEffect = false;
    }
}
