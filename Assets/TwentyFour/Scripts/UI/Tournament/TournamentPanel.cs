using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DG.Tweening;
using kcp2k;
using TMPro;
using TwentyFour.Scripts.Common;
using TwentyFour.Scripts.Quest;
using TwentyFour.Scripts.RemoteConfig;
using TwentyFour.Scripts.Tournament;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class TournamentPanel : MonoBehaviour
{
    private const float PageTransDuration = 0.3f;
    
    public string CurrentTournamentSlugName;
    public TournamentData CurrentTournamentData;
    string CurrencySlug;
    double StartDateTime;
    double EndDateTime;
    public QuestPanel TournamentQuestPanel;
    public GameObject RedeemQuestHint;
    public StorePanel TournamentStorePanel;
    public TiersLeaderboard LeaderboardPanel;

    public GameObject ActiveHint;
    public GameObject EnableEntry;
    public Text EnableEntryText;
    public Text TournamentTitle;
    public Text TournamentDescription;
    public Text LeaderboardTitle;
    public GameObject DisableEntry;
    public Text DisableEntryText;
    public Text TournamentDurationDate;
    public Button MatchMakingButton;
    public TextMeshProUGUI CoinText;


    public LeaderBoardMemberInfo PerfectWinPlayer;
    public LeaderBoardMemberInfo FastSinglePlayer;
    public LeaderBoardMemberInfo FastAvgPlayer;
    public Text MainLeaderboardRewardText;
    public Text SinglePlayerLeaderboardRewardText;
    public Text CurrentRankText;
    public Text ScoreText;
    public Text WinRateText;
    public Text WinAllCountText;
    public Text AverageTimeText;
    
    public Button CloseButton;
    public Button SubscribeButton;

    public MatchMakingManager TournamentMatchMakingManager;
    
    [Header("动画坐标")] public RectTransform LogoTarget;
    public RectTransform MatchMakingButtonTarget;
    public RectTransform InfoTarget;
    public RectTransform ButtonsTarget;
    public RectTransform LogoStart;
    public RectTransform LogoEnd;
    public RectTransform MatchMakingButtonStart;
    public RectTransform MatchMakingButtonEnd;
    public RectTransform InfoStart;
    public RectTransform InfoEnd;
    public RectTransform ButtonsStart;
    public RectTransform ButtonsEnd;
    public RectTransform MatchMakingHintTarget;
    public RectTransform MatchMakingHintStart;
    public RectTransform MatchMakingHintEnd;

    private string StoreSlugName;
    private void Awake()
    {
        CurrentTournamentData = TournamentData.Current;
        CurrentTournamentSlugName = CurrentTournamentData.SlugName;
        var storeData = CurrentTournamentData.ReferenceSlugs.Find(item => item.SlugType == TournamentDataReferenceSlugType.Categories);
        var questData = CurrentTournamentData.ReferenceSlugs.Find(item => item.SlugType == TournamentDataReferenceSlugType.Quests);
        TournamentQuestPanel.QuestsSlugName = questData.Datas.ToDictionary()["main"];
        StoreSlugName = storeData.Datas.ToDictionary()["main"];
        LeaderboardPanel.LeaderboardSlugName =
            CurrentTournamentData.GetLeaderboardData()["TournamentLeaderboardSlug_Main"];
        foreach (var item in CurrentTournamentData.ReferenceSlugs)
        {
            if (item.SlugType == TournamentDataReferenceSlugType.Currency)
            {
                CurrencySlug = item.Datas.ToDictionary()["main"];
            }
        }
    }

    private void OnEnable()
    {
        StartDateTime = TimeConverter.ToTotalSeconds(CurrentTournamentData.StartDate);
        EndDateTime = TimeConverter.ToTotalSeconds(CurrentTournamentData.EndDate);
        MatchMakingButtonTarget.DOMove(MatchMakingButtonStart.position, 0f).From(MatchMakingButtonEnd.position);
        InfoTarget.DOMove(InfoStart.position, PageTransDuration).From(InfoEnd.position);
        ButtonsTarget.DOMove(ButtonsStart.position, PageTransDuration).From(ButtonsEnd.position);
        LogoTarget.DOMove(LogoStart.position, PageTransDuration).From(LogoEnd.position);
        
        RefreshScoreInfo();
        RefreshLeaderboardInfo();
        RefreshQuestInfo();
        RefreshPanel();
        InventoryHelper.OnInventoryUpdated += RefreshCoin;
        QuestHelper.OnSearchPersonaQuests += RefreshQuestInfo;

    }

    public void ShowStore()
    {
        TournamentStorePanel.Show(StoreSlugName);
    }

    public void Subscribe()
    {
        var (start, end) = CurrentTournamentData.GetNearByStartDateTime();
        var startStr = $"{start.ToLocalTime().ToString("HH:mm", new CultureInfo("zh-CN"))}";
        var endStr = $"{end.ToLocalTime().ToString("HH:mm", new CultureInfo("zh-CN"))}";
        Logger.Log($"{start.ToString("f")} {end.ToString("f")}");
        DateTime utcNow = DateTime.UtcNow; // 当前UTC时间
        var startSeconds = TimeConverter.DateTimeToTotalSeconds(start);
        var nowSeconds = TimeConverter.DateTimeToTotalSeconds(utcNow);
        if (nowSeconds < startSeconds)
        {
            string formattedTime = String.Empty;
            if (nowSeconds + 300 >= startSeconds)
            {
                SubscribeButton.gameObject.SetActive(false);
                return;
            }
            else
            {
                //设置发送时间
                DateTime futureUtcTime = start.AddMinutes(-5); 
                formattedTime = futureUtcTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
            }
            
            
            Logger.LogInfo("订阅发送UTC时间：" + formattedTime);
            var tempId = RemoteConfigHelper.GetString(RemoteConfigKeys.WXSubscribe_TournamentMatch);
            if (!string.IsNullOrEmpty(tempId))
            {
                

                WXSubscribe.Subscribe(tempId,(() =>
                {
                    SubscribeButton.gameObject.SetActive(false);
                    StartCoroutine(WXSubscribe.SendPostRequest(tempId,
                        startStr, endStr, formattedTime, CurrentTournamentData.SlugName,(() =>
                        {
                            StartCoroutine(WXSubscribe.SendGETRequest(CurrentTournamentData.SlugName));
                        })));
                }));
            }
        }
        else
        {
            RefreshPanel();
        }

        
        
    }
    void ShowSingleMemberInfo(string leaderboard,LeaderBoardMemberInfo target)
    {
        if (TiersHelper.AllLeaderboardScoresResponse.TryGetValue(leaderboard, out var score))
        {
            if (score.Scores.Count > 0)
            {
                target.Show(score.Scores[0]);
            }
            else
            {
                target.Show(null);
            }
                
        }
        else
        {
            target.Show(null);
        }
    }
    private void OnDestroy()
    {

    }

    public void PlayFadeOut()
    {
        MatchMakingButtonTarget.DOMove(MatchMakingButtonEnd.position, PageTransDuration).From(MatchMakingButtonStart.position).SetEase(Ease.InBack);
        InfoTarget.DOMove(InfoEnd.position, PageTransDuration).From(InfoStart.position).SetEase(Ease.InBack);
        ButtonsTarget.DOMove(ButtonsEnd.position, PageTransDuration).From(ButtonsStart.position).SetEase(Ease.InBack);
        LogoTarget.DOMove(LogoEnd.position, PageTransDuration).From(LogoStart.position).SetEase(Ease.InBack);
        EnableEntryText.DOFade(0, 0.5f).From(PageTransDuration);
        MatchMakingHintTarget.DOMove(MatchMakingHintEnd.position, PageTransDuration).From(MatchMakingHintStart.position);

    }

    public void PlayFadeIn()
    {
        MatchMakingButtonTarget.DOMove(MatchMakingButtonStart.position, PageTransDuration).From(MatchMakingButtonEnd.position);
        InfoTarget.DOMove(InfoStart.position, PageTransDuration).From(InfoEnd.position);
        ButtonsTarget.DOMove(ButtonsStart.position, PageTransDuration).From(ButtonsEnd.position);
        LogoTarget.DOMove(LogoStart.position, PageTransDuration).From(LogoEnd.position);
        EnableEntryText.DOFade(0.5f, PageTransDuration).From(0);
        MatchMakingHintTarget.DOMove(MatchMakingHintStart.position, PageTransDuration).From(MatchMakingHintEnd.position);


    }

    void RefreshPanel()
    {
        var enable = EnableShowEntry();
        if (!enable)
        {
            CheckTournamentCountDown();
        }

        RefreshCoin();
        var StartDate = TimeConverter.ToUTCDate(CurrentTournamentData.StartDate).ToLocalTime();
        var EndDate = TimeConverter.ToUTCDate(CurrentTournamentData.EndDate).ToLocalTime();
        TournamentDurationDate.text =
            $"活动日期：{StartDate.ToString("d")} ~ {EndDate.ToString("d")}";
        LeaderboardTitle.text = TournamentTitle.text = CurrentTournamentData.DisplayName;
        TournamentDescription.text = CurrentTournamentData.Description;
        
        
        
        RefreshEntry(enable);
        MatchMakingButton.onClick.RemoveAllListeners();
        MatchMakingButton.onClick.AddListener(OnMatchMakingButtonClick);
    }

    void RefreshCoin()
    {
        foreach (var inventory in InventoryHelper.ExpandedInventoryItems)
        {
            if (inventory.Resource.ResourceSlug == CurrencySlug)
            {
                CoinText.text = inventory.Quantity.ToString();
            }
            
        }
    }
    private void OnMatchMakingButtonClick()
    {
        var enable = EnableShowEntry();
        RefreshEntry(enable);
        if (enable)
        {
            PlayFadeOut();
            Logger.Log("开始匹配");
            int tournamentScore = 0;
            if (TiersHelper.SelfLeaderboardScoresResponse.TryGetValue(
                            CurrentTournamentData.GetLeaderboardData()["TournamentLeaderboardSlug_Main"], out var score))
                    {
                        if (score.Scores.Any())
                        {
                            tournamentScore = (int)score.Scores[0].Score;
                        }
                    }
            TournamentMatchMakingManager.MatchMakingScore = tournamentScore;
            TournamentMatchMakingManager.TournamentSlugName = CurrentTournamentSlugName;
            TournamentMatchMakingManager.MatchMakingConfigID = CurrentTournamentData.MatchMakingConfig;
            TournamentMatchMakingManager.SetBattleMode(4);
            TournamentMatchMakingManager.MatchMaking();
            CloseButton.gameObject.SetActive(false);
        }
        else
        {
            Logger.Log("不在活动时间");
        }
    }

    void RefreshEntry(bool enable)
    {
        DisableEntry.SetActive(!enable);
        EnableEntry.SetActive(enable);
        ActiveHint.SetActive(enable);
        var now = DateTime.UtcNow;
        var currentTime = TimeConverter.DateTimeToTotalSeconds(now);
        var (start,end) = CurrentTournamentData.GetNearByStartDateTime();
        var enableSubscribe = start != DateTime.MinValue && end != DateTime.MinValue;
        var timeEnable = TimeConverter.DateTimeToTotalSeconds(start) - currentTime > 300;
        SubscribeButton.gameObject.SetActive(!WXSubscribe.Subscribed && enableSubscribe && timeEnable);
        
        
        if (currentTime > EndDateTime)
        {
            DisableEntryText.text = $"比赛已结束";
            SubscribeButton.gameObject.SetActive(false);
        }
        else if(currentTime < StartDateTime)
        {
            DisableEntryText.text = $"比赛未开始";
            
        }
        else
        {
            //Debug.LogError(WXSubscribe.Subscribed +" 已定岳");
            DisableEntryText.text =
                $"今日比赛将于\n{CurrentTournamentData.GetActiveTimeString()}进行";
            

        }
        EnableEntryText.text = $"比赛进行中\n{CurrentTournamentData.GetNearbyTimeString()}";
    }

    bool EnableShowEntry()
    {
        return CurrentTournamentData.IsActive();
    }

    void CheckTournamentCountDown()
    {
        isCheckCountDown = true;
        StartCoroutine(CheckCountDown());
    }
    YieldInstruction oneSecond = new WaitForSeconds(1);
    bool isCheckCountDown = false;
    IEnumerator CheckCountDown()
    {
        while (isCheckCountDown)
        {
            yield return oneSecond;
            if (EnableShowEntry())
            {
                isCheckCountDown = false;
                StopCoroutine(CheckCountDown());
                RefreshEntry(true);
            }
            
        }
    }

    void RefreshQuestInfo()
    {
        var questData = QuestHelper.PersonaQuests[TournamentQuestPanel.QuestsSlugName];
        var canRedeem = false;
        foreach (var quest in questData.Items)
        {
            if (quest.Completed && !quest.Redeemed)
            {
                canRedeem = true;
                break;
            }
        }
        RedeemQuestHint?.SetActive(canRedeem);
    }
    void RefreshScoreInfo()
    {
        if (TiersHelper.SelfLeaderboardScoresResponse.TryGetValue(
                CurrentTournamentData.GetLeaderboardData()["TournamentLeaderboardSlug_Main"], out var score))
        {
            if (score.Scores.Any())
            {
                CurrentRankText.text = score.Scores[0].Rank.ToString(); 
                ScoreText.text = score.Scores[0].Score.ToString();
            }
            else
            {
                CurrentRankText.text = "暂未上榜";
                ScoreText.text = "0";
            }
        }
        else
        {
            CurrentRankText.text = "暂未上榜";
            ScoreText.text = "0";
        }

        if (TournamentDataHelper.TournamentScoreData == null)
        {
            var data = new SelfTournamentScoreData();
            data.winRate = data.winCount = data.avgTime = data.completeWinCount = data.totalCount = "0";
            TournamentDataHelper.TournamentScoreData = data;
        }
        WinRateText.text = TournamentDataHelper.TournamentScoreData.winRate+"%";
        WinAllCountText.text = TournamentDataHelper.TournamentScoreData.completeWinCount;
        AverageTimeText.text = TournamentDataHelper.TournamentScoreData.avgTime + "秒";
    }
    void RefreshLeaderboardInfo()
    {
        if (CurrentTournamentData.GetLeaderboardData()
            .TryGetValue(TournamentDataHelper.LeaderboardSlug_Main, out var main))
        {
            //Debug.LogError(main);
            if (TiersHelper.LeaderboardInfos.TryGetValue(main, out var mainLeaderboard))
            {
                var mainSB = new StringBuilder();
                mainSB.AppendLine("排行榜：");
                foreach (var item in mainLeaderboard.Leaderboard.RewardRules)
                {
                    if (item.From == item.To)
                    {
                        mainSB.AppendLine($"第{item.From}名：{item.Rewards[0].DisplayName}x{item.Rewards[0].Quantity}");
                    }
                    else
                    {
                        mainSB.AppendLine(
                            $"第{item.From}名至{item.To}名：{item.Rewards[0].DisplayName}x{item.Rewards[0].Quantity}");
                    }
                }
                MainLeaderboardRewardText.text = mainSB.ToString();
            }
            
            var singleSB = new StringBuilder();
            singleSB.AppendLine("单项奖：");
            if (CurrentTournamentData.GetLeaderboardData()
                .TryGetValue(TournamentDataHelper.LeaderboardSlug_PerfectWin, out var perfectWin))
            {
                ShowSingleMemberInfo(perfectWin, PerfectWinPlayer);
                if (!TiersHelper.LeaderboardInfos.TryGetValue(perfectWin, out var perfectWinLeaderboard)) return;
                if (perfectWinLeaderboard.Leaderboard.RewardRules.Count > 0)
                {
                    singleSB.AppendLine(
                        $"最多完胜：{perfectWinLeaderboard.Leaderboard.RewardRules[0].Rewards[0].DisplayName}x{perfectWinLeaderboard.Leaderboard.RewardRules[0].Rewards[0].Quantity}");
                }
            }

            if (CurrentTournamentData.GetLeaderboardData()
                .TryGetValue(TournamentDataHelper.LeaderboardSlug_FastSolveSingle, out var fastSingle))
            {
                ShowSingleMemberInfo(fastSingle, FastSinglePlayer);

                if (!TiersHelper.LeaderboardInfos.TryGetValue(fastSingle, out var fastSingleLeaderboard)) return;
                if (fastSingleLeaderboard.Leaderboard.RewardRules.Count > 0)
                {
                    singleSB.AppendLine(
                        $"单题最快：{fastSingleLeaderboard.Leaderboard.RewardRules[0].Rewards[0].DisplayName}x{fastSingleLeaderboard.Leaderboard.RewardRules[0].Rewards[0].Quantity}");
                }
            }

            if (CurrentTournamentData.GetLeaderboardData()
                .TryGetValue(TournamentDataHelper.LeaderboardSlug_FastestAverageTime, out var fastAvg))
            {
                ShowSingleMemberInfo(fastAvg, FastAvgPlayer);
                if (!TiersHelper.LeaderboardInfos.TryGetValue(fastAvg, out var fastAvgLeaderboard)) return;
                if (fastAvgLeaderboard.Leaderboard.RewardRules.Count > 0)
                {
                    singleSB.AppendLine(
                        $"平均最快：{fastAvgLeaderboard.Leaderboard.RewardRules[0].Rewards[0].DisplayName}x{fastAvgLeaderboard.Leaderboard.RewardRules[0].Rewards[0].Quantity}");
                }
            }
            
            
            SinglePlayerLeaderboardRewardText.text = singleSB.ToString();
        }
        
    }
    private void OnDisable()
    {
        QuestHelper.OnSearchPersonaQuests -= RefreshQuestInfo;
        InventoryHelper.OnInventoryUpdated -= RefreshCoin;
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
