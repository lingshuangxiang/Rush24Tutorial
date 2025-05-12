using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Achievement;
using DG.Tweening;
using TwentyFour.Scripts.Achievement;
using TwentyFour.Scripts.PersonaProperty;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class KapibalaEventItem : MonoBehaviour
{
    public List<Sprite> AllIcons = new List<Sprite>();
    [SerializeField] private string currentAchievement;

    public Text Title;
    public Text Reward;
    public Image Icon;
    public Text AchievementProgress;
    public Button ActionButton;
    public Text ActionButtonText;

    public GameObject CompletedHint;
    
    private string currentPersonaKey;
    private string kapibaraKey = "CharKapibara";
    

    AchievementInfoExpanded achievementExpanded;
    public UnityEvent OnTodoEvent = new UnityEvent();
    public void Init(AchievementInfoExpanded info)
    {
        achievementExpanded = info;
        currentAchievement = info.SlugName;
        string iconHint = String.Empty;
        switch (currentAchievement)
        {
            case AchievementKeys.CLEAR_STAGES:
                iconHint = "Eye";
                currentPersonaKey = PersonaPropertyKeys.ActiveAvatarEyeKey;
                break;
            case AchievementKeys.WIN_BATTLE_RANK_3:
                iconHint = "Mouth";
                currentPersonaKey = PersonaPropertyKeys.ActiveAvatarMouthKey;
                break;
            case AchievementKeys.JOIN_BATTLE_CUSTOM:
                iconHint = "Head";
                currentPersonaKey = PersonaPropertyKeys.ActiveAvatarHeadKey;
                break;
        }
        foreach (var icon in AllIcons.Where(icon => icon.name.Contains(iconHint)))
        {
            Icon.sprite = icon;
            break;
        }
        Title.text = info.DisplayName;
        if (info.Rewards.Count > 0)
            Reward.text = $"可获得 {info.Rewards[0].DisplayName}";
        else
            Reward.text = string.Empty;
        AchievementProgress.text = $"{info.AchievedValue}/{info.ThresholdValue}";
        ActionButton.onClick.RemoveAllListeners();
        CompletedHint.SetActive(false);
        ActionButton.GetComponent<Image>().DOFade(info.Completed ? 1 : 0.6f, 0);
        if (!info.Completed)
        {
            ActionButton.onClick.AddListener(OnClickToDO);

        }
        else
        {
            if (info.Redeemed)
            {
                ActionButton.interactable = false;
                ActionButton.gameObject.SetActive(false);
                CompletedHint.SetActive(true);
            }
            else
            {
                ActionButton.interactable = true;
                ActionButtonText.text = "领取！";
                ActionButton.onClick.AddListener(OnActionButtonClick);
            }
            // if (PersonaPropertiesHelper.GetLocalProperties().TryGetValue(currentPersonaKey, out var currentChar) &&
            //     currentChar == kapibaraKey)
            // {
            //     ActionButton.interactable = false;
            //     ActionButton.gameObject.SetActive(false);
            //     CompletedHint.SetActive(true);
            // }
            // else
            // {
            //     ActionButton.interactable = true;
            //     ActionButtonText.text = "变身！";
            //     ActionButton.onClick.AddListener(OnActionButtonClick);
            //
            // }
        }
    }

    public void OnClickToDO()
    {
        OnTodoEvent?.Invoke();   
    }
    public void OnActionButtonClick()
    {
        StartCoroutine(GetReward());
    }

    private bool canPlayGetItemEffect;
    IEnumerator GetReward()
    {
        canPlayGetItemEffect = true;
        UIManager.Instance.ShowCommonLoading("正在领取奖励");
        var rewardAwaiter = AchievementManager.RedeemAchievementRewards(currentAchievement,RedeemFailed);
        yield return new WaitUntil(()=>rewardAwaiter.IsCompleted);
        var inventory = InventoryHelper.ListPersonaInventory();
        yield return new WaitUntil(()=>inventory.IsCompleted);
        UIManager.Instance.HideCommonLoading();
        if (canPlayGetItemEffect)
        {
            var reward = achievementExpanded.Rewards[0];
            var rewardList = new List<GetItemData>();
            GetItemData data = new GetItemData();
            data.DisplayName = reward.DisplayName;
            data.Slug = reward.SlugName;
            data.Count = 1;
            data.Namespace = reward.Namespace;
            rewardList.Add(data);
            UIManager.Instance.ShowGetItemPanel(rewardList);
            ActionButton.interactable = false;
            ActionButton.gameObject.SetActive(false);
            CompletedHint.SetActive(true);
        }
        
    }

    void RedeemFailed(Exception e)
    {
        PassportException passportException = e as PassportException;
        canPlayGetItemEffect = false;
        UIMessage.Show(passportException?.ErrorMessage);
    }
    
}
