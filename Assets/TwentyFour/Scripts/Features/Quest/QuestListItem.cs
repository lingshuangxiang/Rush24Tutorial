using System;
using System.Collections;
using System.Collections.Generic;
using Quest;
using TwentyFour.Scripts.Metrics;
using TwentyFour.Scripts.Quest;
using TwentyFour.Scripts.Wechat;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.UI;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.UI;

public class QuestListItem : MonoBehaviour
{
    PersonaQuestItem questItem;
    public Text QuestName;
    public Text QuestProcess;
    public Text Reward;
    public Image Icon;
    public Image ButtonIcon;
    public Button ActionButton;
    public Text ActionButtonText;

    public GameObject CompletedHint;
    Action RedeemedCallback;
    Action ADToDO;
    
    public Color DefaultColor;
    public Color RedeemedColor;
    public Image ButtonBackground;
    void SetParams(PersonaQuestItem quest)
    {
        questItem = quest;
        QuestName.text = quest.DisplayName;
        QuestProcess.text = $"{quest.AchievedValue}/{quest.ThresholdValue}";
        if (quest.Rewards.Count > 0)
        {
            Reward.text = $"x{quest.Rewards[0].Quantity}";
        }
        
        ActionButton.interactable = false;
        ActionButton.onClick.RemoveAllListeners();
    }
    public void Init(PersonaQuestItem quest,Action redeemedCallback = null,Action todo= null)
    {
        SetParams(quest);
        RedeemedCallback = redeemedCallback;
        ButtonBackground.color = DefaultColor;
        if (quest.Completed)
        {
            if (quest.Redeemed)
            {
                ActionButton.gameObject.SetActive(false);
                CompletedHint.SetActive(true);
            }
            else
            {
                ButtonBackground.color = RedeemedColor;
                ActionButton.gameObject.SetActive(true);
                CompletedHint.SetActive(false);
                ActionButtonText.text = "领取";
                ActionButton.interactable = true;
                ActionButton.onClick.AddListener(OnActionButtonClick);

            }
        }
        else
        {
            if (todo != null)
            {
                ActionButton.interactable = true;
                ActionButtonText.text = "去完成";
                ActionButton.onClick.AddListener(() =>
                {
                    todo?.Invoke();
                });
            }
        }
    }

    public void InitAD(PersonaQuestItem quest,Action redeemedCallback = null,Action todo= null)
    {
        SetParams(quest);
        RedeemedCallback = redeemedCallback;
        ADToDO = todo;
        ButtonBackground.color = DefaultColor;
        ActionButton.interactable = true;
        if (quest.Completed)
        {
            if (quest.Redeemed)
            {
                ActionButton.gameObject.SetActive(false);
                CompletedHint.SetActive(true);
            }
            else
            {
                ButtonBackground.color = RedeemedColor;
                ActionButton.gameObject.SetActive(true);
                CompletedHint.SetActive(false);
                ActionButtonText.text = "领取";
                ActionButton.interactable = true;
                ActionButton.onClick.AddListener(OnADButtonClick);

            }
        }
        else
        {
            ActionButtonText.text = "去完成";
            ActionButton.gameObject.SetActive(true);
            CompletedHint.SetActive(false);
            ActionButton.interactable = true;

            ActionButton.onClick.AddListener(OnClickADButton);
            
        }
    }

    private void OnADButtonClick()
    {

    }


    private void OnClickADButton()
    {
        WXAdManager.OnRewardedADSucceed -= OnRewardedADSucceed;
        WXAdManager.OnRewardedADSucceed += OnRewardedADSucceed;
        WXAdManager.ShowRewardAd();
    }

    private void OnRewardedADSucceed()
    {
        var dic = new Dictionary<string, object>();
        if (questItem.Properties.TryGetValue(MetricsKeys.PARAM_REWARD_TYPE, out string rewardType))
        {
            dic[MetricsKeys.PARAM_REWARD_TYPE] = rewardType;
        }

        MetricsHelper.TrackEvent(MetricsKeys.EVENT_WATCH_ADV, dic);
        UIManager.Instance.StartCoroutine(UploadAD());
    }

    private bool canPlayGetItemEffect;

    public void OnActionButtonClick()
    {
        UIManager.Instance.StartCoroutine(GetReward());
    }

    IEnumerator UploadAD()
    {
        UIManager.Instance.ShowCommonLoading("正在上传");
        var ad = QuestHelper.UpdatePersonaQuestItem(
            questItem.SlugName, QuestItemUpdateAction.Increase,
            1);
        yield return new WaitUntil(() => ad.IsCompleted);
        var search = QuestHelper.SearchPersonaQuests();
        yield return new WaitUntil(() => search.IsCompleted);
        
        UIManager.Instance.HideCommonLoading();
        ADToDO?.Invoke();
    }

    IEnumerator GetReward(Action onComplete = null)
    {
        canPlayGetItemEffect = true;
        UIManager.Instance.ShowCommonLoading("正在领取奖励");
        var rewardAwaiter = QuestHelper.RedeemQuestItemRewards(questItem.SlugName, RedeemFailed);
        yield return new WaitUntil(()=>rewardAwaiter.IsCompleted);
        UIManager.Instance.HideCommonLoading();
        if (canPlayGetItemEffect)
        {
            var search = QuestHelper.SearchPersonaQuests();
            yield return new WaitUntil(() => search.IsCompleted);
            ActionButton.interactable = false;
            ActionButton.gameObject.SetActive(false);
            CompletedHint.SetActive(true);
            var itemList = new List<GetItemData>();
            foreach (var reward in questItem.Rewards)
            {
                var data = new GetItemData();
                data.DisplayName = reward.Resource.DisplayName;
                data.Slug = reward.SlugName;
                data.Count = reward.Quantity;
                data.Namespace = reward.Resource.Namespace;
                itemList.Add(data);
                MetricsHelper.TrackEvent(MetricsKeys.EVENT_REDEEM_ITEM,new Dictionary<string, object>()
                {
                    {MetricsKeys.PARAM_REWARD_TYPE,reward.SlugName},
                    {MetricsKeys.PARAM_FROM,questItem.SlugName},
                    {MetricsKeys.PARAM_REDEEM_TYPE,"quest"}
                });
            }
            UIManager.Instance.ShowGetItemPanel(itemList);
            onComplete?.Invoke();
        }
        RedeemedCallback?.Invoke();
        
    }

    void RedeemFailed(Exception e)
    {
        canPlayGetItemEffect = false;
        PassportException passportException = e as PassportException;
        UIMessage.Show(passportException?.ErrorMessage);
    }
}
