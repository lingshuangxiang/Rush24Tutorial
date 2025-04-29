using System;
using System.Collections;
using System.Collections.Generic;
using TwentyFour.Scripts.Accomplishment;
using UnityEngine;
using UnityEngine.UI;

public class AccomplishmentItem : MonoBehaviour
{
    public GameObject Win;
    public GameObject Lose;
    public GameObject Draw;
    
    public GameObject WinAll;
    public GameObject LoseAll;

    public Text ScoreText;
    public Text PlayerNameText;
    public Text GameModeText;
    public Text TimeText;
    public List<Text> CardsTexts = new List<Text>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void ScrollCellIndex (int index)
    {
        var data = AccomplishmentHelper.LocalAccomplishmentData.data[index];
        Win.SetActive(false);
        Lose.SetActive(false);
        WinAll.SetActive(false);
        LoseAll.SetActive(false);
        Draw.SetActive(false);
        TeamProgress selfTeamProgress;
        TeamProgress opponentTeamProgress;
        if (data.playerTeam == "BLUE")
        {
            selfTeamProgress = data.blueTeamProgress;
            opponentTeamProgress = data.redTeamProgress;
            
        }
        else
        {
            selfTeamProgress = data.redTeamProgress;
            opponentTeamProgress = data.blueTeamProgress;
            
        }

        if (selfTeamProgress.score > opponentTeamProgress.score)
        {
            Win.SetActive(true);
            if (selfTeamProgress.score == 5)
            {
                WinAll.SetActive(true);
            }
        }
        else if (selfTeamProgress.score < opponentTeamProgress.score)
        {
            Lose.SetActive(true);
            if (opponentTeamProgress.score == 5)
            {
                LoseAll.SetActive(true);
            }
        }
        else
        {
            Draw.SetActive(true);
        }

        ScoreText.text = $"{selfTeamProgress.score} : {opponentTeamProgress.score}";
        PlayerNameText.text = opponentTeamProgress.teamPlayers[0].displayName;
        string mode = String.Empty;
        if (data.battleMode == 1)
        {
            mode = "排位赛";
        }
        else if (data.battleMode == 2)
        {
            mode = "好友娱乐赛";
        }
        else if(data.battleMode == 4)
        {
            mode = "锦标赛";
        }

        GameModeText.text = mode;
        //Debug.LogError(data.startTime);
        var startTimeStr = DateTime.Parse(data.startTime).ToString("MM-dd HH:mm");
        TimeText.text = startTimeStr;
        for (int i = 0; i < CardsTexts.Count; i++)
        {
            var text = CardsTexts[i];
            var card = data.questions[i].question.cards;
            var cardList = new List<int>(4);
            foreach (var c in card)
            {
                cardList.Add(c.number);
            }
            var content = string.Join(",", cardList);
            text.text = content;
        }
        
        
    }
}
