using System;
using System.Collections;
using System.Collections.Generic;
using Economy;
using Quest;
using TwentyFour.Scripts.Category;
using TwentyFour.Scripts.Quest;
using TwentyFour.Scripts.UI.Store;
using UnityEngine;
using UnityEngine.UI;

public class DailyVitQuestPanel : MonoBehaviour
{
    public StoreProductListItem PurchaseItem;
    public QuestListItem ADItem;
    PersonaQuest currentQuestList;
    private const string vitQuestSlug = "DailyVitQuestSlug";
    ExpandedProductSDK CurrentProductSDK;
    public Text VITText;
    
    public bool IsLinkedQuest;
    public List<string> LinkedQuestSlugNames;
    private void OnEnable()
    {
        CurrentProductSDK = CategoryHelper.LocalProducts[CategoryHelper.DefaultVITCategory].Products[0];
        //Debug.LogError(currentQuestList.DisplayName);
        RefreshItemList();
        RefreshVIT();
        VitalityHelper.Instance.OnVitalityCheck += RefreshVIT;
    }

    private void OnDisable()
    {
        if(VitalityHelper.Instance != null)
            VitalityHelper.Instance.OnVitalityCheck -= RefreshVIT;
    }

    void RefreshItemList()
    {
        currentQuestList = QuestHelper.PersonaQuests[vitQuestSlug];
        Dictionary<string, List<PersonaQuestItem>> LinkedQuestsMap = new Dictionary<string, List<PersonaQuestItem>>();
        LinkedQuestsMap.Clear();
        foreach (var questSlug in LinkedQuestSlugNames)
        {
            LinkedQuestsMap.Add(questSlug, new List<PersonaQuestItem>());
        }

        foreach (var quest in currentQuestList.Items)
        {
            foreach (var questSlug in LinkedQuestSlugNames)
            {
                //判断是否是同类型的任务
                var slug = quest.SlugName.Split('_')[0];
                if (questSlug == slug)
                {
                    LinkedQuestsMap[questSlug].Add(quest);
                }
            }
        }

        foreach (var kv in LinkedQuestsMap)
        {
            Dictionary<string, PersonaQuestItem> questItems = new Dictionary<string, PersonaQuestItem>();
            foreach (var quest in kv.Value)
            {
                questItems[quest.SlugName] = quest;
            }

            PersonaQuestItem target = null;
            for (int i = 0; i < questItems.Count; i++)
            {
                if (questItems.TryGetValue($"{kv.Key}_{i}", out PersonaQuestItem data))
                {
                    if (i == questItems.Count - 1)
                    {
                        target = data;
                        break;
                    }
                    else
                    {
                        if (data.Completed && data.Redeemed)
                        {
                            continue;
                        }
                        else
                        {
                            target = data;
                            break;
                        }
                    }
                }

                
            }

            if (target != null)
            {
                ADItem.InitAD(target, (RefreshItemList),(RefreshItemList));

            }
            else
            {
                ADItem.gameObject.SetActive(false);
            }
            
        }
        
        
        PurchaseItem.Init(CurrentProductSDK);
    }

    void RefreshVIT()
    {
        if (VitalityHelper.Instance.CurrentVitality >= VitalityHelper.Instance.MaxVitality)
        {
            VITText.text = "体力已满";
        }
        else
        {
            TimeSpan time = TimeSpan.FromSeconds(VitalityHelper.Instance.NextTimeRemain);   // 转换为时间跨度
            string formattedTime = $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}"; 
            VITText.text = $"{formattedTime}后 恢复1点体力";
        }
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
