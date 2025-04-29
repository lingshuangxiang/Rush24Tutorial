using System;
using System.Collections;
using System.Collections.Generic;
using Quest;
using TwentyFour.Scripts.Quest;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QuestPanel : MonoBehaviour
{
    public string QuestsSlugName;
    public GameObject QuestItemPrefab;
    public GameObject ADQuestItemPrefab;
    public RectTransform QuestItemContainer;
    PersonaQuest currentQuestList;
    public ScrollRect scrollRect;
    public bool IsLinkedQuest;
    public List<string> LinkedQuestSlugNames;

    public UnityEvent ToDoEvent;

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
        RefreshItemList();
    }

    public void RefreshItemList()
    {
        currentQuestList = QuestHelper.PersonaQuests[QuestsSlugName];
        foreach (var child in QuestItemContainer.transform.GetComponentsInChildren<QuestListItem>())
        {
            Destroy(child.gameObject);
        }

        if (IsLinkedQuest)
        {
            InitLinkedQuest();
        }
        else
        {
            InitQuest();
        }
    }

    void InitLinkedQuest()
    {
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
                bool isAD = false;
                if (target.Properties.TryGetValue("type", out string type))
                {
                    isAD = type == "ad";
                }
                var Prefab = isAD ? ADQuestItemPrefab : QuestItemPrefab;
                var item = Instantiate(Prefab, QuestItemContainer).GetComponent<QuestListItem>();
                if (isAD)
                {
                    item.InitAD(target, (RefreshItemList),(RefreshItemList));
                }
                else
                {
                    item.Init(target, (RefreshItemList),(() =>
                    {
                        ToDoEvent?.Invoke();
                    }));
                }
            }
            
            
        }
    }

    void InitQuest()
    {
        foreach (var quest in currentQuestList.Items)
        {
            var item = Instantiate(QuestItemPrefab, QuestItemContainer).GetComponent<QuestListItem>();
            item.Init(quest);
        }
    }

    public void LockScroll()
    {
        StopCoroutine(ResetVertical());
        StartCoroutine(ResetVertical());
    }

    IEnumerator ResetVertical()
    {
        scrollRect.vertical = false;
        yield return new WaitForSeconds(0.5f);
        scrollRect.vertical = true;
    }

    // Update is called once per frame
    void Update()
    {
    }
}