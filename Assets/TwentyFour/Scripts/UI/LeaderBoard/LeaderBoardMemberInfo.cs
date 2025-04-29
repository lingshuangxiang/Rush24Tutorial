using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Leaderboard;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardMemberInfo : MonoBehaviour
{
    public PlayerCharatorManager playerCharatorManager;
    public Text PlayerName;
    public Text ScoreText;
    public string EmptyText;
    public Text EmptyHint;

    public void Show(LeaderboardMemberScore info)
    {
        if (info == null)
        {
            EmptyHint.text = EmptyText;
            EmptyHint.gameObject.SetActive(true);
            PlayerName.gameObject.SetActive(false);
            ScoreText.text = string.Empty;
            playerCharatorManager.gameObject.SetActive(false);
        }
        else
        {
            playerCharatorManager.gameObject.SetActive(true);
            EmptyHint.gameObject.SetActive(false);
            PlayerName.gameObject.SetActive(true);
            PlayerName.text = info.DisplayName;
            ScoreText.text = info.Score.ToString("0.##").TrimEnd('0').TrimEnd('.');
            playerCharatorManager.InitPlayerAvatar(info.PersonaProperties.ToDictionary(x => x.Key, x => x.Value));
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
