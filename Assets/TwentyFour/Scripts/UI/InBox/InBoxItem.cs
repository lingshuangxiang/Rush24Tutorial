using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DG.Tweening;
using Inbox;
using TwentyFour.Scripts.Common;
using TwentyFour.Scripts.Metrics;
using TwentyFour.Scripts.Tournament;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

public class InBoxItem : MonoBehaviour
{
    InboxMessage CurrentMessage;
    public GameObject SelectHint;

    public GameObject ReadedHint;
    public Image ShadowImage;

    public RectTransform MailInfo;
    
    public Text TitleText;
    public Text ContentText;
    public Image RewardImage;
    public Text RewardCountText;

    public RectTransform MailInfoEnd;
    public Tweener MailInfoTweener;
    
    public InBoxPanel ParentInBoxPanel => UIManager.Instance.InBoxPanelInstance;
    int Index;
    // Start is called before the first frame update
    void Start()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }
    private void OnClick()
    {
        ParentInBoxPanel.SelectedIndex = Index;
        ParentInBoxPanel.MailContentText.text = CurrentMessage.Body;
        var sb = new StringBuilder();
        sb.AppendLine(CurrentMessage.Source);
        sb.AppendLine(CurrentMessage.SendAt.ToDateTime().ToLocalTime().ToString("f",new CultureInfo("zh-CN")));
        sb.AppendLine(TimeConverter.GetTimeAgoOrAfter(CurrentMessage.ExpiredAt.Seconds)+"过期");
        ParentInBoxPanel.FromText.text = sb.ToString();
        var hasReward = CurrentMessage.Attachment.Count > 0;
        ParentInBoxPanel.RedeemButton.gameObject.SetActive(hasReward);
        if (hasReward)
        {
            ParentInBoxPanel.RedeemButton.onClick.RemoveAllListeners();
            var unredeemed = CurrentMessage.Status == MessageStatusType.Unconsumed || 
                             CurrentMessage.Status == MessageStatusType.Unread;
            if (unredeemed)
            {
                ParentInBoxPanel.RedeemButton.onClick.AddListener(OnRedeem);
                ParentInBoxPanel.RedeemButton.interactable = true;
                ParentInBoxPanel.RedeemButtonText.text = "领取奖励";
            }
            else
            {
                ParentInBoxPanel.RedeemButton.interactable = false;
                ParentInBoxPanel.RedeemButtonText.text = "已领取";
            }
        }
        ResetOthers();
        ParentInBoxPanel.MailDetail.gameObject.SetActive(true);
        SelectHint.SetActive(true);
        ShadowImage.enabled = true;
        MailInfoTweener = MailInfo.DOMove(MailInfoEnd.position, 0.2f).From(transform.position);
        if (CurrentMessage.Status == MessageStatusType.Unread)
        {
            StartCoroutine(ReadMail());
        }
        else if( CurrentMessage.Status == MessageStatusType.Unconsumed)
        {
            ReadedHint.SetActive(true);
        }
        else
        {
            ReadedHint.SetActive(false);

        }
    }

    IEnumerator ReadMail()
    {
        UIManager.Instance.ShowCommonLoading("正在读取中...");
        var read = InBoxHelper.ReadMessage(CurrentMessage.Id);
        yield return new WaitUntil(() => read.IsCompleted);
        CurrentMessage.Status = read.Result.Message.Status;
        UIManager.Instance.HideCommonLoading();
        if (read.Result.Message.Status != MessageStatusType.Completed)
        {
            ReadedHint.SetActive(true);
        }
        else
        {
            ReadedHint.SetActive(false);
        }
    }

    bool canPlayGetItemEffect;
    IEnumerator Redeem()
    {
        canPlayGetItemEffect = true;
        var redeem = InBoxHelper.ConsumeMessage(CurrentMessage.Id,RedeemFailed);
        yield return new WaitUntil(() => redeem.IsCompleted);
        CurrentMessage.Status = redeem.Result.Message.Status;
        if (redeem.Result.Message.Status != MessageStatusType.Completed)
        {
            ReadedHint.SetActive(true);
        }
        else
        {
            ReadedHint.SetActive(false);
        }
        
        
        if (canPlayGetItemEffect)
        {
            ParentInBoxPanel.RedeemButton.interactable = false;
            ParentInBoxPanel.RedeemButtonText.text = "已领取";
            var updateInventory = InventoryHelper.ListPersonaInventory();
            yield return TournamentDataHelper.FetchTournamentData();
            yield return new WaitUntil(() => updateInventory.IsCompleted);
            var itemList = new List<GetItemData>();
            foreach (var reward in redeem.Result.Message.Attachment)
            {
                var data = new GetItemData();
                data.DisplayName = reward.Resource.DisplayName;
                data.Slug = reward.ItemSlug;
                data.Count = reward.Quantity;
                data.Namespace = reward.Resource.Namespace;
                itemList.Add(data);
                MetricsHelper.TrackEvent(MetricsKeys.EVENT_REDEEM_ITEM,new Dictionary<string, object>()
                {
                    {MetricsKeys.PARAM_REWARD_TYPE,reward.ItemSlug},
                    {MetricsKeys.PARAM_FROM,CurrentMessage.Id},
                    {MetricsKeys.PARAM_REDEEM_TYPE,"inbox"}
                });
            }
            UIManager.Instance.ShowGetItemPanel(itemList);
            
        }
        UIManager.Instance.HideCommonLoading();

    }

    void RedeemFailed(Exception e)
    {
        canPlayGetItemEffect = false;
        PassportException passportException = e as PassportException;
        UIMessage.Show(passportException?.ErrorMessage);
        UIManager.Instance.HideCommonLoading();
    }

    private void OnRedeem()
    {
        UIManager.Instance.ShowCommonLoading("正在兑换");
        StartCoroutine(Redeem());
    }

    void ResetOthers()
    {
        foreach (var item in transform.parent.GetComponentsInChildren<InBoxItem>())
        {
            item.Reset();
        }
    }

    private void OnDisable()
    {
        //Reset();
    }

    public void Reset()
    {
        var active = UIManager.Instance.InBoxPanelInstance.SelectedIndex == Index;
        SelectHint.SetActive(active);
        ShadowImage.enabled = active;
        MailInfoTweener.Kill();
        if (active)
        {
            MailInfo.position = MailInfoEnd.position;

        }
        else
        {
            MailInfo.DOLocalMove( Vector3.zero, 0);

        }
    }

    void ScrollCellIndex (int index)
    {
        Index = index;
        Reset();
        var data = InBoxHelper.InBoxMessages[index];
        CurrentMessage = data;
        TitleText.text = data.Title;
        ContentText.text = $"{TimeConverter.GetTimeAgoOrAfter(data.SendAt.Seconds)}";
        ReadedHint.SetActive(data.Status == MessageStatusType.Unread || data.Status == MessageStatusType.Unconsumed);
        if (data.Attachment.Count > 0)
        {
            RewardImage.gameObject.SetActive(true);
            RewardCountText.gameObject.SetActive(true);
            var item = data.Attachment[0];
            RewardImage.sprite = InventoryHelper.GetItemIcon(item.Resource.Namespace, item.ItemSlug);
            RewardCountText.text = item.Quantity.ToString();
        }
        else
        {
            RewardImage.gameObject.SetActive(false);
            RewardCountText.gameObject.SetActive(false);
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
