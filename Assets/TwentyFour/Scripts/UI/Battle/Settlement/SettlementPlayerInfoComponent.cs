using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.UOS.TwentyFour.Model.Sync;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.UI;

public class SettlementPlayerInfoComponent : MonoBehaviour
{
    public List<Text> Results = new List<Text>();
    public Text PlayerNameText;

    public Text AveTimeText;

    public void Init(TeamProgress teamProgress)
    {
        gameObject.SetActive(true);
        PlayerNameText.text = teamProgress.teamPlayers[0].displayName;
        int count = teamProgress.resolved.Count(status => status);
        if (count == 0)
        {
            Results[0].gameObject.SetActive(true);
            Results[0].text = "未解答出题目";
        }
        else
        {
            for (int i = 0; i < teamProgress.resolved.Count; i++)
            {
                Results[i].gameObject.SetActive(teamProgress.resolved[i]);
                if (teamProgress.resolved[i])
                {
                    
                    var stage = MuninnMessage.BattleData.stages[i].question;
                    List<int> numbers = new List<int>();
                    foreach (var card in stage.cards)
                    {
                        numbers.Add(card.number);
                    }

                    var text = string.Join(',', numbers);
                    Results[i].text = text +" √";
                }
            }
        }
        
    }

    public void InitUnResolved(List<ResolvedStatus> status)
    {
        int count = status.Count(status => !status.resolved);
        if (count == 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            for (int i = 0; i < status.Count; i++)
            {
                Results[i].gameObject.SetActive(!status[i].resolved);
                var stage = MuninnMessage.BattleData.stages[i].question;
                List<int> numbers = new List<int>();
                foreach (var card in stage.cards)
                {
                    numbers.Add(card.number);
                }

                var text = string.Join(',', numbers);
                Results[i].text = text;
            }
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
