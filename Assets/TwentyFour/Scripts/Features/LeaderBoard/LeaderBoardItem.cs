using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Leaderboard;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardItem : MonoBehaviour
{
    public Text NameText;

    public TextMeshProUGUI RankingText;
    
    public TextMeshProUGUI LevelLabelText;
    
    public TiersBadge CurrentTierBadge;

    public List<Color> leaderboardColors = new List<Color>();
    public Color defaultLeaderboardColor = Color.black;
    public Text ScoreText;
    
    public string LeaderboardSlugName;
    
    LeaderboardMemberScore leaderboardMemberScore;
    public void Init(LeaderboardMemberScore memberScore,Color color,int ranking)
    {
        leaderboardMemberScore = memberScore;
        RankingText.color = color;
        NameText.text = memberScore.DisplayName;
        RankingText.text = (ranking + 1).ToString();
        //fill star count
        int starCount = (int)(memberScore.Score > TiersHelper.TopTierScore
            ? memberScore.Score - TiersHelper.TopTierScore
            : memberScore.Score % TiersHelper.StarCountPerTier);
        LevelLabelText.text = starCount.ToString();
        if (ScoreText != null)
        {
            ScoreText.text = memberScore.Score.ToString();
            //ScoreText.color = color;
        }
        // resolve badge
        CurrentTierBadge.SetupBadge(true, (int)memberScore.Score, memberScore.Tier);
        if (TryGetComponent<Button>(out var button))
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    void OnClick()
    {
        UIManager.Instance.PlayerInfoPanelInstance.ShowPlayerInfo(leaderboardMemberScore.MemberId, leaderboardMemberScore);
    }
    void ScrollCellIndex (int index)
    {
        var data = TiersHelper.AllLeaderboardScoresResponse[LeaderboardSlugName].Scores[index];
        string name = $"{index}_{data.DisplayName}_{data.Score}_{data.Tier} ";
        gameObject.name = name;
        if (index < leaderboardColors.Count)
        {
            
            RankingText.transform.localScale = Vector3.one * (1.2f + 0.4f * (leaderboardColors.Count - index));
            Init(data, leaderboardColors[index], index);
        }
        else
        {
            RankingText.transform.localScale = Vector3.one;
            Init(data, defaultLeaderboardColor, index);
        }
        
    }
}
