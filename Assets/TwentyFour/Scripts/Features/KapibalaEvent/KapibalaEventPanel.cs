using System.Collections;
using System.Collections.Generic;
using TwentyFour.Scripts.Achievement;
using TwentyFour.Scripts.PersonaProperty;
using UnityEngine;
using UnityEngine.Events;

public class KapibalaEventPanel : MonoBehaviour
{
    public GameObject KapibalaEventItemPrefab;

    public RectTransform ItemList;
    
    public ChangeAvatarPanel ChangePanel;

    public UnityEvent ShowRankBattleEvent;
    public UnityEvent ShowCustomBattleEvent;
    public UnityEvent ShowStageEvent;
    public List<string> KapibalaEventItems = new List<string>();
    // Start is called before the first frame update
    void Start()
    {
        RefreshItemList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void RefreshItemList()
    {
        foreach (var child in ItemList.transform.GetComponentsInChildren<KapibalaEventItem>())
        {
            Destroy(child.gameObject);
        }
        foreach (var achievement in AchievementManager.LocalAchievements)
        {
            if (KapibalaEventItems.Contains(achievement.SlugName))
            {
                var item = Instantiate(KapibalaEventItemPrefab, ItemList).GetComponent<KapibalaEventItem>();
                item.Init(achievement);
                switch (achievement.SlugName)
                {
                    case AchievementKeys.CLEAR_STAGES:
                        item.OnTodoEvent = ShowStageEvent;
                        break;
                    case AchievementKeys.WIN_BATTLE_RANK_3:
                        item.OnTodoEvent = ShowRankBattleEvent;
                        break;
                    case AchievementKeys.JOIN_BATTLE_CUSTOM:
                        item.OnTodoEvent = ShowCustomBattleEvent;
                        break;
                }
            }
            
            
        }
    }
}
