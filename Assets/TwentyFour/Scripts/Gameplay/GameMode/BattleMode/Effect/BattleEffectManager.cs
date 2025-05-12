using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Achievement;
using DG.Tweening;
using Economy;
using Quest;
//using Google.Protobuf.WellKnownTypes;
using TMPro;
using TwentyFour.Scripts.Achievement;
using TwentyFour.Scripts.Metrics;
using TwentyFour.Scripts.Quest;
using TwentyFour.Scripts.RemoteConfig;
using TwentyFour.Scripts.Tournament;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.UI;
using Unity.UOS.TwentyFour;
using Unity.UOS.TwentyFour.Model.Sync;
using Unity.UOS.TwentyFour.Robot;
using Unity.UOS.TwentyFour.Scripts.Battle.Model;
using Unity.UOS.TwentyFour.UOSGateway;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Action = Achievement.Action;
using Random = UnityEngine.Random;
using Logger = Unity.UOS.TwentyFour.Common.Logger;


public class BattleEffectManager : MonoBehaviour
{
    #region 全局方法

    /// <summary>
    /// 判断是否被答题
    /// </summary>
    /// <param name="questionIndex"></param>
    /// <returns></returns>
    public static bool IsGotAnswer(int questionIndex)
    {
        if (questionIndex < MuninnMessage.BattleData.resolvedStatus.Count && questionIndex >= 0)
        {
            Logger.Log("IsGotAnswer:" + questionIndex);
            return MuninnMessage.BattleData.resolvedStatus[questionIndex].resolved;
        }
        else
        {
            Logger.LogInfo("IsGotAnswer Error:" + questionIndex);
            return false;
        }
    }

    /// <summary>
    /// 判断是否当前的题目被答对
    /// </summary>
    /// <returns></returns>
    public static bool IsCurrentQuestionResolved()
    {
        if (instance?._TargetQuestionList == null)
        {
            return false;
        }
        return IsGotAnswer(BattleEffectManager.instance._TargetQuestionList.myIndex);
    }

    public static string GetResolvedTeam(int index)
    {
        return GetTeamById(MuninnMessage.BattleData.resolvedStatus[index].resolvedPersonaID);
    }

    public static string GetTeamById(string id)
    {
        if (MuninnMessage.BattleData.redTeamProgress.teamPlayers.Find(x => x.uniqueId == id) != null)
            return "RED";
        if (MuninnMessage.BattleData.blueTeamProgress.teamPlayers.Find(x => x.uniqueId == id) != null)
            return "BLUE";
        return "";
    }


    // public static int GetAllResolvedQuestionsCount()
    // {
    //     int Resolved = 0;
    //     string mid = PassportSDK.CurrentPersona.PersonaID;
    //     for (int i = 0; i < MuninnMessage.BattleData.resolvedStatus.Count; i++)
    //     {
    //         if (MuninnMessage.BattleData.resolvedStatus[i].resolved &&
    //             MuninnMessage.BattleData.resolvedStatus[i].resolvedPersonaID.ToString() == mid)
    //         {
    //             Resolved++;
    //         }
    //     }
    //
    //     return Resolved;
    // }

    #endregion

    #region 全局变量

    public static bool NeedMatchImmediately = false;
    //public  static  
    #endregion

    public List<CardGroup> allCardGroups = new List<CardGroup>();
    public Transform TextParent;
    public SimpleAnimTrigger TextAnimTrigger;
    public bool TimerFinalCountTriggered;
    public float TimerFinalCountThreshold = 10;
    public CollectCardsEffect CardCollector;
    public RectTransform OperatorGroupTransform;
    public float hideDuration = .3f; // 隐藏动画持续时间
    public float hideDistance = 200f; // 隐藏距离，根据实际UI宽度调整
    public QuestionList _TargetQuestionList;
    public int CurrentIndex = 0;

    public TextMeshProUGUI TimerText;

    // public List<BattlePlayer> AllPlayersAvatar = new List<BattlePlayer>();//现在玩家头像 0,1,是红队,2,3是蓝队
    public List<BattlePlayer> AllBattlePlayers = new List<BattlePlayer>();
    public GameObject AvatarPrefab;
    public Transform BlueTeamAvatarParent, RedTeamAvatarParent;
    public Color BlueTeamColor, RedTeamColor;
    public GameObject RobbedPanel, RobboedIcon;
    public GameObject CounterPanel;
    public GameObject tierPanel;
    public static BattleEffectManager instance;
    public GameObject SceneCanvas;
    public GameObject WinPrefab;
    public GameObject LosPrefab;
    public GameObject DrawPrefab;
    public int playTimeRecorder;
    public float playTimeCounter;
    
    private Vector2 operatorsOriginalPosition;
    MuninnMessage myGameManager;
    private bool isSettlement = false;
    private bool ifWin;
    private Coroutine operatorMovingCoroutine;
   

    #region 数据记录

    public static string MyTeamName;

    #endregion

    #region 事件

    #endregion

    private void OnEnable()
    {
        MuninnMessage.OnProgress.AddListener(CheckIfChangedMessage);
        MuninnMessage.OnCountDown.AddListener(SetTextEffect);
        MuninnMessage.OnSyncStatus.AddListener(SyncOthersWorkProgress);
    }

    private void OnDisable()
    {
        MuninnMessage.OnProgress.RemoveListener(CheckIfChangedMessage);
        MuninnMessage.OnCountDown.RemoveListener(SetTextEffect);
        MuninnMessage.OnSyncStatus.RemoveListener(SyncOthersWorkProgress);
    }

    private void OnDestroy()
    {
        tournamentSlug = string.Empty;
        StopAllCoroutines();
        if (MuninnManager.Singleton != null)
        {
            MuninnManager.Singleton.OnDisconnectAction -= LoadSceneOnDisconnect;

        }
    }

    void Start()
    {
        instance = this;
        playTimeRecorder = 0;
        playTimeCounter = 0;
        _TargetQuestionList = FindAnyObjectByType<QuestionList>();
        operatorsOriginalPosition = OperatorGroupTransform.anchoredPosition;
        //初始化ServerMessage
        localServerMessage = new MuninnMessageData();
        localServerMessage.redTeamProgress = new TeamProgress();
        localServerMessage.blueTeamProgress = new TeamProgress();
        localServerMessage.redTeamProgress.resolved = new List<bool>() { false, false, false, false, false };
        localServerMessage.blueTeamProgress.resolved = new List<bool>() { false, false, false, false, false };
        StartCoroutine(AssignmentTeamInfoAtFirstTime(MuninnMessage.BattleData));
        tierPanel.gameObject.SetActive(false);
        LateStart();
        MuninnMessage.OnEndGame.AddListener(OnEndGame);
        
        //MuninnMessage.OnEndGameResponse.AddListener(OnEndGameResponse);

    }

    private bool receivedEndGame;
    private void OnEndGame(MuninnMessageData message)
    {
        //SettlementEndingAnim();
        receivedEndGame = true;
        DowngradeHelper.SetDowngradeInfo();
    }

    private void Update()
    {
        playTimeCounter+=Time.deltaTime;
    }

    /// <summary>
    /// 避免初始化冲突，弄一个lateStart.比Start慢100
    /// </summary>
    async void LateStart()
    {
        //await Task.Delay(100);
        // _TargetQuestionList.SetIndex(0);//默认显示选择第一道题目
        StartCoroutine(WaitLateStart());
    }

    IEnumerator WaitLateStart()
    {
        yield return 100;
        _TargetQuestionList.SetIndex(0); //默认显示选择第一道题目
    }

    public void HideOperators()
    {
        // hideDuration = .3f;
        if (operatorMovingCoroutine != null)
        {
            StopCoroutine(operatorMovingCoroutine);
        }
        operatorMovingCoroutine = StartCoroutine(MoveOperators(new Vector2(-hideDistance * Screen.width, operatorsOriginalPosition.y)));
    }

    public void ShowOperators()
    {
        // StopAllCoroutines();
        if (operatorMovingCoroutine != null)
        {
            StopCoroutine(operatorMovingCoroutine);
        }

        // hideDuration = 0;
        operatorMovingCoroutine = StartCoroutine(MoveOperators(operatorsOriginalPosition));
    }

    private IEnumerator MoveOperators(Vector2 targetPosition)
    {
        float elapsedTime = 0f;
        Vector2 startPosition = OperatorGroupTransform.anchoredPosition;

        while (elapsedTime < hideDuration)
        {
            OperatorGroupTransform.anchoredPosition =
                Vector2.Lerp(startPosition, targetPosition, elapsedTime / hideDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        OperatorGroupTransform.anchoredPosition = targetPosition;
    }

    // 在指定时间后自动隐藏
    public void AutoHideAfterDelay(float delay)
    {
        // StartCoroutine(AutoHideCoroutine(delay));
    }

    private IEnumerator AutoHideCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideOperators();
    }

    /// <summary>
    /// 将文字恢复至原来的大小,在新一轮比赛的时候必须重置
    /// </summary>
    //   [Button]
    public void ResetText()
    {
        //TextParent.DOScale(1, 1).From(1);
        if (PlayOnlyOnce != null)
            StopCoroutine(PlayOnlyOnce);
        TextParent.localScale = Vector3.one;
        TimerFinalCountTriggered = false;
    }

    //[FoldoutGroup("测试数字")]
    public float TestTarget = 12;

    //  [FoldoutGroup("测试数字")]
    //[Button]
    public void TestSetTextEffect()
    {
        SetTextEffect((int)TestTarget);
    }

    /// <summary>
    /// 可以在每次倒计时的时候调用，根据当前倒计时进行显示
    /// </summary>
    /// <param name="targetCount"></param>
    public void SetTextEffect(int targetCount)
    {
        if (targetCount >= 181)
        {
            int restcount = targetCount - 181;
            CounterPanel.GetComponent<ReadyPanel>().Play(restcount);
            //CounterPanel.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = restcount.ToString();
            CounterPanel.gameObject.SetActive(true);
            TimerText.text = "03:00";
            return;
        }
        else
        {
            CounterPanel.gameObject.SetActive(false);
        }

        TimeSpan time = TimeSpan.FromSeconds(targetCount);
        if (targetCount <= TimerFinalCountThreshold && TimerFinalCountTriggered == false)
        {
            TimerText.text = targetCount.ToString();
            TimerFinalCountTriggered = true;
            SetTextAlarm();
        }
        else if (targetCount < TimerFinalCountThreshold)
        {
            TimerText.text = targetCount.ToString();
            TextAnimTrigger.PlayTarget();
            // 时间结束进入结算
            if (targetCount == 0)
            {
                //ifWin = ifMyTeamWin();
                SettlementEndingAnim();
            }
        }

        else
        {
            TimerText.text = $"{time.Minutes.ToString("D2")}:{time.Seconds.ToString("D2")}";
        }
    }

    
    /// <summary>
    /// 胜利与否判断
    /// </summary>
    /// <returns></returns>
    public bool IfMyTeamWin()
    {
        MuninnMessageData getData = MuninnMessage.BattleData;
        int DifScore = getData.redTeamProgress.score - getData.blueTeamProgress.score;
        // 比较得分
        // 红队获胜
        if (MuninnManager.MyTeamTag == TeamTag.RED)
        {
            Logger.LogInfo("RED Win");
            return DifScore > 0;
        }
        // 蓝队获胜
        else if (MuninnManager.MyTeamTag == TeamTag.BLUE)
        {
            Logger.LogInfo("BLUE Win");
            return DifScore < 0;
        }
        else
        {
            // 得分相同，默认返回失败
            // TODO
            // 平局或者需要进一步的逻辑来决定胜者
            Logger.LogInfo("The result is a draw");
            return false;
        }
    }

    /// <summary>
    /// 判断对手是否完胜
    /// </summary>
    /// <returns></returns>
    public bool IfOtherTeamCompletelyWin()
    {
        var other = GetOtherProgress();
        return other.score == 5;
    }

    public bool IsDraw()
    {
        MuninnMessageData getData = MuninnMessage.BattleData;
        int DifScore = getData.redTeamProgress.score - getData.blueTeamProgress.score;
        return DifScore == 0;
    }

    /// <summary>
    /// 播放对应的anim动画
    /// </summary>
    /// <param name="ifWin"></param>
    private GameObject settlementObject;

    public void SettlementEndingAnim()
    {
        if(isSettlement) return;
        Logger.Log("[BattleEffectManager] SettlementEndingAnim");
        
        isSettlement = true;
        Dictionary<string, string> properties = MuninnManager.GetRoom().Room.Properties;
        if (properties.TryGetValue(CustomRoomPropertyKey.TournamentSlugName,
                out string tournament))
        {
            tournamentSlug = tournament;
        }
        string getId = PassportSDK.CurrentPersona.PersonaID;
        List<ResolvedStatus> getAllQuestions = MuninnMessage.BattleData.resolvedStatus;
        int yourCorrectAnswers = 0;
        int otherCorrectAnswers = 0;
        getAllQuestions.RemoveAll(x => x.resolved == false);

        for (int i = 0; i < getAllQuestions.Count; i++)
        {
            if (getAllQuestions[i].resolvedPersonaID == getId)
            {
                yourCorrectAnswers++;
            }
            else
            {
                otherCorrectAnswers++;
            }
        }

        var battleData = MuninnMessage.BattleData;
        Logger.LogInfo($"battleMode:{battleData.battleMode}");

        if (battleData.battleMode != GameInitManagerLocal.PreviousBattleMode)
        {
            Logger.LogError("BattleMode not equal");
            GameInitManagerLocal.PreviousBattleMode = battleData.battleMode;
        }
        DestroySettlementAndShowTierPanel();
    }

    
    public UnityEvent OnShowSettlementPanelEvent = new UnityEvent();
    void DestroySettlementAndShowTierPanel()
    {
        var roomPlayers = MuninnManager.Singleton.GetMuninnRoomView().Players;
        var redTeamPlayers = MuninnMessage.BattleData.redTeamProgress.teamPlayers;
        var blueTeamPlayers = MuninnMessage.BattleData.redTeamProgress.teamPlayers;

        var settlePrefab = IsDraw() ? DrawPrefab : IfMyTeamWin() ? WinPrefab : LosPrefab;
        settlementObject = Instantiate(settlePrefab, SceneCanvas.transform);
        var AnimEventBridge = settlementObject.GetComponent<SettlementAnimBridge>();
        // 动画事件：结束时Hook上传数据事件
        AnimEventBridge?.SetOnDestroyAction(OnSettlementEndingAnim);
        Dictionary<string, string> properties = MuninnManager.GetRoom().Room.Properties;
        properties[CustomRoomPropertyKey.ROOM_STATE] = CustomRoomState.Default.ToString();
        properties[CustomRoomPropertyKey.TournamentSlugName] = string.Empty;
        RoomManager.UpdateRoomCustomProperties(properties);
        AFXMusic afx = IsDraw()? AFXMusic.BattleDraw : IfMyTeamWin()? AFXMusic.BattleWin : AFXMusic.BattleLose;
        BGMManager.Instance.PlayAFX(afx);
        

    }

    string tournamentSlug = string.Empty;
    void OnSettlementEndingAnim()
    {
        if (GameInitManagerLocal.PreviousBattleMode != BattleMode.OneOnOneCustom)
            RoomManager.LeaveRoom(); //离开房间
        StartCoroutine(SetAndShowSettlementPanel());
    }
    IEnumerator SetAndShowSettlementPanel()
    {
        Destroy(settlementObject);
        var battlePassBadge = tierPanel.GetComponentInChildren<BattleSettlementPanel>();

        string red = "Player0", blue = "Player1";

        foreach (var player in AllBattlePlayers)
        {
            if (player.Team.Equals(TeamTag.RED))
            {
                red = player.PlayerName;
            }
            if (player.Team.Equals(TeamTag.BLUE))
            {
                blue = player.PlayerName;
            }
        }
        battlePassBadge?.SetPlayerName(red, blue);

        UIManager.Instance.ShowCommonLoading("正在更新成绩");
        
        var updateScoreAwaiter = battlePassBadge?.SetBattleScore();
        if(battlePassBadge != null)
            yield return new WaitUntil(() => updateScoreAwaiter.IsCompleted);
        
        yield return UpLoadAchievement(ShowTierPanel);
        yield return UploadQuest();
        yield return new WaitUntil(() => receivedEndGame);
        TrackMetricsEvent();
        //Destroy(settlementObject);
        ShowTierPanel();
    }


    void TrackMetricsEvent()
    {
        var self = GetSelfProgress();
        var other = GetOtherProgress();
        TeamPlayer selfPlayer = GetSelfPlayer();
        var dic = new Dictionary<string, object>()
        {
            { MetricsKeys.PARAM_BATTLE_MODE, GameInitManagerLocal.PreviousBattleMode.ToString() },
            { MetricsKeys.PARAM_COMPETITOR_ID, other.teamPlayers[0].uniqueId },
            { MetricsKeys.PARAM_BATTLE_RESULT, self.score > other.score },
            { MetricsKeys.PARAM_IS_ROBOT_ROOM, MuninnManager.IsBotRoom },
            { MetricsKeys.PARAM_ROOM_ID, MuninnManager.Singleton.RoomId },
            { MetricsKeys.PARAM_SCORE, selfPlayer.currentScore },
            { MetricsKeys.PARAM_TIER, selfPlayer.currentTier },
        };
        if (self.score >= 5 || other.score >= 5)
        {
            dic[MetricsKeys.PARAM_BATTLE_COMPLETE_WIN_OR_LOSE] = true;
        }
        else
        {
            dic[MetricsKeys.PARAM_BATTLE_COMPLETE_WIN_OR_LOSE] = false;
        }
        MetricsHelper.TrackEvent(MetricsKeys.EVENT_ENGAGE_BATTLE,dic);
    }
    TeamPlayer GetSelfPlayer()
    {
        TeamPlayer self;
        if (MuninnManager.MyTeamTag == TeamTag.RED)
        {
            self = MuninnMessage.BattleData.redTeamProgress.teamPlayers[0];
        }
        // 蓝队获胜
        else if (MuninnManager.MyTeamTag == TeamTag.BLUE)
        {
            self = MuninnMessage.BattleData.blueTeamProgress.teamPlayers[0];
        }
        else
        {
            self = new TeamPlayer();
        }
        return self;
    }

    TeamProgress GetSelfProgress()
    {
        TeamProgress self;
        if (MuninnManager.MyTeamTag == TeamTag.RED)
        {
            self = MuninnMessage.BattleData.redTeamProgress;
        }
        // 蓝队获胜
        else if (MuninnManager.MyTeamTag == TeamTag.BLUE)
        {
            self = MuninnMessage.BattleData.blueTeamProgress;
        }
        else
        {
            self = new TeamProgress();
        }
        return self;
    }

    TeamProgress GetOtherProgress()
    {
        TeamProgress other;
        if (MuninnManager.MyTeamTag == TeamTag.RED)
        {
            other = MuninnMessage.BattleData.blueTeamProgress;
        }
        // 蓝队获胜
        else if (MuninnManager.MyTeamTag == TeamTag.BLUE)
        {
            other = MuninnMessage.BattleData.redTeamProgress;
        }  
        else
        {
            other = new TeamProgress();
        }
        return other;
    }
    void ShowTierPanel()
    {
        var battlePassBadge = tierPanel.GetComponentInChildren<BattleSettlementPanel>();
        UIManager.Instance.HideCommonLoading();
        OnShowSettlementPanelEvent?.Invoke();
        tierPanel.gameObject.SetActive(true);
        TeamPlayer self = GetSelfPlayer();

        if (itemList is { Count: > 0 })
        {
            var desc = $"你达到了{string.Join(",", titleList)}段位！这是你的奖励！";
            GetItemParams p = new GetItemParams()
            {
                GetItemDataList = itemList,
                Title = desc
            };
            battlePassBadge?.SetAndShowTier(self, p);
        }
        else
        {
            battlePassBadge?.SetAndShowTier(self, null);
        }
        
    }
    List<GetItemData> itemList = new List<GetItemData>();
    List<string> titleList = new List<string>();
    IEnumerator UpLoadAchievement(System.Action failedAction = null)
    {
        if (GameInitManagerLocal.PreviousBattleMode == BattleMode.OneOnOneCustom)
        {
            var upLoadAchievement =
                AchievementManager.UpdatePersonaAchievement(AchievementKeys.JOIN_BATTLE_CUSTOM, Action.Increase, 1,null,failedAction);
            yield return new WaitUntil(() => upLoadAchievement.IsCompleted);
        }
        else if (GameInitManagerLocal.PreviousBattleMode == BattleMode.OneOnOne)
        {
            if (IfMyTeamWin())
            {
                var upLoadAchievement =
                    AchievementManager.UpdatePersonaAchievement(AchievementKeys.WIN_BATTLE_RANK_3, Action.Increase, 1,null,failedAction);
                yield return new WaitUntil(() => upLoadAchievement.IsCompleted);
                Logger.Log($"{upLoadAchievement.Result.DisplayName} {upLoadAchievement.Result.AchievedValue}");
            }
            
            var fetchAchievement = AchievementManager.ListPersonaAchievements();
            yield return new WaitUntil(() => fetchAchievement.IsCompleted);
            List<AchievementInfoExpanded> redeem_season_achievements = new();
            var seasonKey = RemoteConfigHelper.GetString(RemoteConfigKeys.CurrentSeasonSlug);
            if (!string.IsNullOrEmpty(seasonKey))
            {
                foreach (var achievement in AchievementManager.LocalAchievements)
                {
                    if (achievement.SlugName.Contains(seasonKey))
                    {
                        if (achievement.Completed && !achievement.Redeemed)
                        {
                            redeem_season_achievements.Add(achievement);
                        }
                    }
                }

                itemList?.Clear();
                titleList?.Clear();
                foreach (var achievement in redeem_season_achievements)
                {
                    var redeem = AchievementManager.RedeemAchievementRewards(achievement.SlugName);
                    yield return new WaitUntil(() => redeem.IsCompleted);
                    if (redeem.IsCompletedSuccessfully)
                    {
                        foreach (var reward in achievement.Rewards)
                        {
                            var data = new GetItemData(reward);
                            itemList.Add(data);
                        }
                        if(achievement.Properties.TryGetValue("rank",out string rank))
                            titleList.Add(rank);
                        else
                            Logger.LogError($"custom property [rank] not found in achievement {achievement.SlugName}");
                        
                    }
                
                }
            }
            
        }

        yield return null;
    }

    IEnumerator UploadQuest(System.Action failedAction = null)
    {
        var self = GetSelfProgress();
        var other = GetOtherProgress();
        if (GameInitManagerLocal.PreviousBattleMode == BattleMode.TournamentOneOnOne)
        {
            var currenttournament = TournamentData.Current.SlugName;
            if (!string.IsNullOrEmpty(tournamentSlug) && currenttournament == tournamentSlug)
            {
                var tournament = tournamentSlug;
                if (self.score > other.score)
                {
                    var dailyWin = QuestHelper.UpdatePersonaQuestItem(
                        $"{tournament}_{QuestKeys.DailyMatchMakingWin}", QuestItemUpdateAction.Increase,
                        1);
                    yield return new WaitUntil(() => dailyWin.IsCompleted);
                    if (self.score >= 5)
                    {
                        //完胜
                        var dailyWinCompletely = QuestHelper.UpdatePersonaQuestItem(
                            $"{tournament}_{QuestKeys.DailyMatchMakingWinCompletely}", QuestItemUpdateAction.Increase,
                            1);
                        yield return new WaitUntil(() => dailyWinCompletely.IsCompleted);
                    }
                }

                var daily = QuestHelper.UpdatePersonaQuestItem(
                    $"{tournament}_{QuestKeys.DailyMatchMaking}", QuestItemUpdateAction.Increase,
                    1);
                yield return new WaitUntil(() => daily.IsCompleted);
            }
            
        }
        else if(GameInitManagerLocal.PreviousBattleMode == BattleMode.OneOnOne)
        {
            
            if (self.score > other.score)
            {
                var dailyWin = QuestHelper.UpdatePersonaQuestItem(
                    $"{QuestKeys.DailyMatchMakingWin}_0", QuestItemUpdateAction.Increase,
                    1);
                var dailyWin1 = QuestHelper.UpdatePersonaQuestItem(
                    $"{QuestKeys.DailyMatchMakingWin}_1", QuestItemUpdateAction.Increase,
                    1);
                var dailyWin2 = QuestHelper.UpdatePersonaQuestItem(
                    $"{QuestKeys.DailyMatchMakingWin}_2", QuestItemUpdateAction.Increase,
                    1);
                
                yield return new WaitUntil(() => dailyWin.IsCompleted && dailyWin1.IsCompleted && dailyWin2.IsCompleted);
                if (self.score >= 5)
                {
                    //完胜
                    var dailyWinCompletely = QuestHelper.UpdatePersonaQuestItem(
                        $"{QuestKeys.DailyMatchMakingWinCompletely}_0", QuestItemUpdateAction.Increase,
                        1);
                    var dailyWinCompletely1 = QuestHelper.UpdatePersonaQuestItem(
                        $"{QuestKeys.DailyMatchMakingWinCompletely}_1", QuestItemUpdateAction.Increase,
                        1);
                    var dailyWinCompletely2 = QuestHelper.UpdatePersonaQuestItem(
                        $"{QuestKeys.DailyMatchMakingWinCompletely}_2", QuestItemUpdateAction.Increase,
                        1);
                    yield return new WaitUntil(() => dailyWinCompletely.IsCompleted && dailyWin1.IsCompleted && dailyWin2.IsCompleted);
                }
            }

            var daily = QuestHelper.UpdatePersonaQuestItem(
                $"{QuestKeys.DailyMatchMaking}_0", QuestItemUpdateAction.Increase,
                1);
            var daily1 = QuestHelper.UpdatePersonaQuestItem(
                $"{QuestKeys.DailyMatchMaking}_1", QuestItemUpdateAction.Increase,
                1);
            var daily2 = QuestHelper.UpdatePersonaQuestItem(
                $"{QuestKeys.DailyMatchMaking}_2", QuestItemUpdateAction.Increase,
                1);
            yield return new WaitUntil(() => daily.IsCompleted && daily1.IsCompleted && daily2.IsCompleted);
        }
        
    }
    private void SetTextAlarm()
    {
        //  TextParent.DOScale(2, 15);
        if (PlayOnlyOnce != null)
            StopCoroutine(PlayOnlyOnce);
        PlayOnlyOnce = SetScale(1, 3, 15, TextParent);
        StartCoroutine(PlayOnlyOnce);

        //TestForGameEnding(newserverMessage);
    }

    private IEnumerator PlayOnlyOnce;

    IEnumerator SetScale(float _From, float Scaler, float SetTime, Transform _target)
    {
        float timers = 0;

        while (timers < SetTime)
        {
            timers += Time.deltaTime;
            yield return null;
            float tolerp = timers / SetTime;
            tolerp = Mathf.Min(1, tolerp);
            _target.transform.localScale = Vector3.Lerp(new Vector3(_From, _From, _From),
                new Vector3(Scaler, Scaler, Scaler), tolerp);
        }

        _target.transform.localScale = new Vector3(Scaler, Scaler, Scaler);
    }

    /// <summary>
    /// 这只是个暂时的卡牌演示
    /// </summary>
    IEnumerator CollectCardsDelay()
    {
        yield return new WaitForSeconds(01);
        CollectCards();
        // Invoke("CollectCards", 1f);
    }

    /// <summary>
    /// 当作对题目的时候，所有的牌收集到一起并且有一个动画
    /// </summary>
    // [Button]
    public void CollectCards()
    {
        CardCollector.ShowAndPlay();
    }

    /// <summary>
    /// 当要开始下一轮答题的时候，必须进行重置牌组
    /// </summary>
    //  [Button]
    public void ResetCards()
    {
        CardCollector.ResetToOrigin();
        ShowOperators();
    }

    /// <summary>
    /// 每次赢了一道题目后会调用这个
    /// </summary>
    public void RecordThePlayTimeInGame()
    {
        playTimeRecorder = (int)playTimeCounter;
    }

    #region 具体联机同步表现方案的mvc

    MuninnMessageData localServerMessage = new MuninnMessageData(); //本地的ServerMessage集成了全部队伍输赢和信息。

    public UnityEvent<int,bool> OnRedScoreChange = new UnityEvent<int,bool>();
    public UnityEvent<int,bool> OneBlueScoreChange = new UnityEvent<int,bool>();

    /// <summary>
    /// 比照本地的玩家数据跟服务端发送的数据
    /// </summary>
    async void CheckIfChangedMessage(MuninnMessageData newServerMessage)
    {
        StartCoroutine(WaitCheckMessage(newServerMessage));
        // 解题结束判断
        if (newServerMessage.allResolved)
        {
            SettlementEndingAnim();
        }
    }

    IEnumerator WaitCheckMessage(MuninnMessageData newServerMessage)
    {
        yield return StartCoroutine(AssignmentTeamInfoAtFirstTime(newServerMessage)); //初始化玩家的头像设置,只在第一次同步的时候更新
        Logger.Log($"WaitCheckMessage :{JsonUtility.ToJson(newServerMessage)}");
        Logger.LogInfo($"currentResolvedTeam :{newServerMessage.currentResolvedTeam} myTeamTag:{MuninnManager.MyTeamTag}");
        Logger.LogInfo(
            $"currentResolvedQuestionIndex :{newServerMessage.currentResolvedQuestionIndex} myIndex:{_TargetQuestionList.myIndex}");
        //题目被抢
        if (!MuninnManager.MyTeamTag.Equals(newServerMessage.currentResolvedTeam) &&
            _TargetQuestionList.myIndex == newServerMessage.currentResolvedQuestionIndex)
        {
            RobbedPanel.GetComponent<RobbedEffect>().CurrentResolvedPersonaID =
                newServerMessage.currentResolvedPersonaID;
            BGMManager.Instance.PlayAFX(AFXMusic.BattleRobbed);
            Effect_isRobbed();
            Effect_SelectOtherTeamDone();
        }

        //update resolved question icon's color to resolved team
        _TargetQuestionList.SetCorrect(newServerMessage.currentResolvedQuestionIndex,
            newServerMessage.currentResolvedTeam.ToString());

        //display collect and spread cards effect
        if (newServerMessage.currentResolvedQuestionIndex.Equals(_TargetQuestionList.myIndex))
        {
            // Effect_MyTeamDone();
            for (int c = 0; c < allCardGroups.Count; c++)
            {
                allCardGroups[c].HideValueTag();
            }
            RecordThePlayTimeInGame();//record my Play Time if finish question 
            CollectCards();
            HideOperators();
        }

        //display resolved player's effect on avatar
        ShowWinner(newServerMessage);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SyncOthersWorkProgress(int currentIndex, string personaID)
    {
        string myId = PassportSDK.CurrentPersona.PersonaID;
        if (personaID != myId)
        {
            Logger.Log("SyncOthersWorkProgress:2");
            _TargetQuestionList.ShowOtherIndex(currentIndex);
        }
    }


    #region 工具-获取最新得分队伍的全部数据

    /// <summary>
    /// 正确显示赢了的玩家的头像，分数等
    /// </summary>
    /// <param name="newServerMessage"></param>
    private void ShowWinner(MuninnMessageData newServerMessage)
    {
        bool isRed = MuninnManager.MyTeamTag.Equals(TeamTag.RED);
        bool isBlue = MuninnManager.MyTeamTag.Equals(TeamTag.BLUE);
        Logger.LogInfo(MuninnManager.MyTeamTag.ToString()+$"  isRed:{isRed} isBlue:{isBlue}");
        OnRedScoreChange?.Invoke(newServerMessage.redTeamProgress.score, isRed);
        OneBlueScoreChange?.Invoke(newServerMessage.blueTeamProgress.score, isBlue);

        for (int i = 0; i < AllBattlePlayers.Count; i++)
        {
            if (AllBattlePlayers[i].PersonaID.Equals(newServerMessage.currentResolvedPersonaID))
            {
                AllBattlePlayers[i].AnswerCorrect();
            }
        }

    }
    

    #endregion
    
    

    #region 表情效果根据联网进行封装

    private bool isTeamPlayerInfoSet = false; //如果已经设置过了

    /// <summary>
    /// 如果第一次传输胜负的值，就会有限配置一下用户的基础信息
    /// </summary>
    IEnumerator AssignmentTeamInfoAtFirstTime(MuninnMessageData newMessage)
    {
        if (isTeamPlayerInfoSet) yield break;

        isTeamPlayerInfoSet = true; //只能配置一次
        foreach (Transform m_child in BlueTeamAvatarParent)
        {
            m_child.gameObject.SetActive(false);
        }

        foreach (Transform m_child in RedTeamAvatarParent)
        {
            m_child.gameObject.SetActive(false);
        }

        string id = PassportSDK.CurrentPersona.PersonaID;

        GameObject _playerObj = null; //

        for (int i = 0; i < newMessage.redTeamProgress.teamPlayers.Count; i++) //处理红队信息
        {
            TeamPlayer teamPlayer = newMessage.redTeamProgress.teamPlayers[i];
            GameObject cloneTarget = Instantiate(AvatarPrefab, RedTeamAvatarParent.transform);
            cloneTarget.transform.Find("Character_(Mask)").Find("BackGround").GetComponent<Image>().color =
                RedTeamColor;

            BattlePlayer battlePlayer = cloneTarget.GetComponent<BattlePlayer>();
            battlePlayer.Team = TeamTag.RED;
            battlePlayer.PersonaID = teamPlayer.uniqueId;
            battlePlayer.InitPlayerInfo();

            if (battlePlayer.PersonaID == id) //是本地玩家本人
            {
                cloneTarget.GetComponent<Image>().color = Color.yellow;
                _playerObj = cloneTarget;
            }

            AllBattlePlayers.Add(battlePlayer);
            yield return null;
        }

        for (int i = 0; i < newMessage.blueTeamProgress.teamPlayers.Count; i++) //处理蓝队信息
        {
            TeamPlayer teamPlayer = newMessage.blueTeamProgress.teamPlayers[i];
            GameObject cloneTarget = Instantiate(AvatarPrefab, BlueTeamAvatarParent.transform);
            cloneTarget.transform.Find("Character_(Mask)").Find("BackGround").GetComponent<Image>().color =
                BlueTeamColor;

            BattlePlayer battlePlayer = cloneTarget.GetComponent<BattlePlayer>();
            battlePlayer.Team = TeamTag.BLUE;
            battlePlayer.PersonaID = teamPlayer.uniqueId;
            battlePlayer.InitPlayerInfo();

            if (battlePlayer.PersonaID == id) //是本地玩家本人
            {
                cloneTarget.GetComponent<Image>().color = Color.yellow;
                _playerObj = cloneTarget;
            }

            AllBattlePlayers.Add(battlePlayer);
            //
            // PlayerAvatarData newdata = new PlayerAvatarData();
            // newdata.Playerid = newMessage.blueTeamProgress.teamPlayers[i].uniqueId;
            // newdata.m_Avatar = cloneTarget.GetComponent<BattlePlayer>();
            // newdata.TeamName = "BLUE";
            // newdata.Playerid = newMessage.blueTeamProgress.teamPlayers[i].uniqueId;
            //
            // if (newMessage.blueTeamProgress.teamPlayers[i].uniqueId == id) //是本地玩家本人
            // {
            //     cloneTarget.GetComponent<Image>().color=Color.yellow;
            //     MyTeamName = "BLUE";
            //     _playerObj = cloneTarget;
            // }
            //
            // AllPlayerAvatarDatas.Add(newdata);
            yield return null;
        }

        if (_playerObj != null) //将玩家物体设置为子物体的第一位
        {
            _playerObj.transform.SetSiblingIndex(0);
        }
        else
        {
            Logger.LogError("没有在列表中发现本地玩家的id!");
        }
    }

    #endregion

    /// <summary>
    /// 主要用于给战斗场景的选择题目的按钮调用，及时刷新InGameManager.currentStage 
    /// </summary>
    /// <param name="stageIndex"></param>
    public void btn_SetTargetStageToCurrentStage(int stageIndex)
    {
        InGameManager.currentStage = StageManager.GetAllStages(GameMode.Battle)[stageIndex];
    }


    /// <summary>
    /// 同步显示被抢题目
    /// </summary>
    public void Effect_isRobbed()
    {
        RobbedPanel.SetActive(true);
        // RobboedIcon.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Effect_SelectOtherTeamDone()
    {
        // OperatorGroupTransform.gameObject.SetActive(false);
        // CardCollector.cards[0].parent.parent.gameObject.SetActive(false);  
        ///   RobbedPanel.SetActive(false);
        RobboedIcon.SetActive(true);
    }

    public void Effect_NoTeamDoneYet()
    {
        // OperatorGroupTransform.gameObject.SetActive(true);
        // CardCollector.cards[0].parent.parent.gameObject.SetActive(true);         
        //RobbedPanel.SetActive(false);
        RobboedIcon.SetActive(false);
    }

    /// <summary>
    /// 自己的队伍答对了就会展开牌面隐藏算数
    /// </summary>
    public void Effect_MyTeamDone()
    {
        // OperatorGroupTransform.gameObject.SetActive(false);
        // CardCollector.cards[0].parent.parent.gameObject.SetActive(false);  
        //HideOperators();
        //  StartCoroutine(CollectCardsDelay()); //收集卡牌的效果实现        
    }


    /// <summary>
    ///通过for循环显示哪些完成了哪些没有完成
    /// </summary>
    /// <param name="Results"></param>
    /// <returns></returns>
    IEnumerator refreshOwnerProgress(List<bool> Results)
    {
        for (int i = 0; i < Results.Count; i++)
        {
            if (Results[i] == true) //通过了
            {
                //yield return SetNext(i);
                _TargetQuestionList.SetCorrect(i);
                yield return null;
            }
        }
    }

    #endregion

    /// <summary>
    /// 方便按钮加载场景，目前用于返回主界面
    /// </summary>
    /// <param name="sceneName"></param>

    #region Button的方法放这儿了

    public void LoadMainScene()
    {
        GameRouter.LoadHomeScene();
    }
    public static void btn_LoadTargetScene(string sceneName)
    {
        RoomManager.LeaveRoom();
        SceneManager.LoadScene(sceneName);
        // try
        // {
        //     SceneManager.UnloadSceneAsync("BattleScene");
        // }
        // catch (Exception e)
        // {
        //     Console.WriteLine(e);
        //     throw;
        // }
        //
        // try
        // {
        //     SceneManager.UnloadSceneAsync("LoadingSceneAnim");
        // }
        // catch (Exception e)
        // {
        //     Console.WriteLine(e);
        //     throw;
        // }
    }

    void LoadSceneOnDisconnect()
    {
        Logger.LogInfo("OnDisconnectTriggerOnceMore Player Count < 2,LoadHomeScene");
        MuninnManager.Singleton.OnDisconnectAction -= LoadSceneOnDisconnect;
        GameRouter.LoadHomeScene();
    }

    private bool clicked;
    public void btn_NeedPlayAgain(bool _istrue)
    {
        if (_istrue)
        {
            if(clicked) return;
            clicked = true;
            GameInitManagerLocal.MyInitState = InitState.MatchAgain;
            if (GameInitManagerLocal.PreviousBattleMode == BattleMode.OneOnOne || GameInitManagerLocal.PreviousBattleMode == BattleMode.TournamentOneOnOne)
            {
                GameRouter.LoadHomeScene();
                return;
            }

            if (GameInitManagerLocal.ReceiveCustomOnceMoreResponse)
            {
                GameInitManagerLocal.MyInitState = InitState.MatchAgain;
                GameRouter.LoadHomeScene();
                return;
            }
            //点击再来一次按钮
            if (GameInitManagerLocal.PreviousBattleMode == BattleMode.OneOnOneCustom && !GameInitManagerLocal.IsCustomOnceMoreSender)
            {
                //房间人数>2
                if (MuninnManager.Singleton.GetMuninnRoomView().Players.Count >= 2)
                {
                    GameInitManagerLocal.IsCustomOnceMoreSender = true;
                    MuninnManager.Singleton.CustomOnceMoreRequest();
                }
                else
                {
                    MuninnManager.Singleton.OnDisconnectAction -= LoadSceneOnDisconnect;
                    MuninnManager.Singleton.OnDisconnectAction += LoadSceneOnDisconnect;
                    RoomManager.LeaveRoom();
                    StartCoroutine(LeaveRoomTimeOutDetect());
                }
                

            }
        }
        else
        {
            if (GameInitManagerLocal.PreviousBattleMode == BattleMode.TournamentOneOnOne)
            {
                GameInitManagerLocal.MyInitState = InitState.ShowTournamentPanel;

            }
            else
            {
                GameInitManagerLocal.MyInitState = InitState.None;

            }

        }
        
    }

    bool isLoadMainScene = false;
    public void CheckLoadScene()
    {
        if (isLoadMainScene) return;
        isLoadMainScene = true;
        if (GameInitManagerLocal.PreviousBattleMode == BattleMode.OneOnOneCustom)
        {
            MuninnManager.Singleton.OnDisconnectAction -= LoadSceneOnDisconnect;
            MuninnManager.Singleton.OnDisconnectAction += LoadSceneOnDisconnect;
            RoomManager.LeaveRoom();
            StartCoroutine(LeaveRoomTimeOutDetect());
        }
        else
        {
            LoadMainScene();
        }
    }

    IEnumerator LeaveRoomTimeOutDetect()
    {
        Logger.LogInfo("房间人数<2,开始检测离开CustomRoom超时");
        yield return new WaitForSeconds(5f);
        Logger.LogInfo("房间人数<2,离开CustomRoom超时");
        GameInitManagerLocal.MyInitState = InitState.None;
        LoadSceneOnDisconnect();
    }

    #endregion
}


#if UNITY_EDITOR

[CustomEditor(typeof(BattleEffectManager))]
public class BattleEffectManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BattleEffectManager myScript = (BattleEffectManager)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("收集卡牌"))
        {
            ///   myScript.CollectCards();
        }

        if (GUILayout.Button("恢复卡牌"))
        {
            myScript.ResetCards();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("测试倒数数字"))
        {
            myScript.TestSetTextEffect();
        }

        if (GUILayout.Button("重置数字"))
        {
            myScript.ResetText();
        }


        if (GUILayout.Button("隐藏操作符"))
        {
            myScript.HideOperators();
        }

        if (GUILayout.Button("显示操作符"))
        {
            myScript.ShowOperators();
        }
        //if (GUILayout.Button("Win结算测试"))
        //{
        //    myScript.CccTestForEnding(true);
        //}
        //if (GUILayout.Button("Lose结算测试"))
        //{
        //    myScript.CccTestForEnding(false);
        //}
    }
}
#endif