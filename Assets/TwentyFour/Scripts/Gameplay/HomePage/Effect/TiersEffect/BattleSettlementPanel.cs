using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using TwentyFour.Scripts.PersonaProperty;
using Unity.Muninn.Model;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.UI;
using Unity.UOS.TwentyFour;
using Unity.UOS.TwentyFour.Model.Sync;
using Unity.UOS.TwentyFour.Scripts.Battle.Model;
using Unity.UOS.TwentyFour.Scripts.Component;
using Unity.UOS.TwentyFour.UOSGateway;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class BattleSettlementPanel : MonoBehaviour
{
    public GameObject titleWin, titleLose, tagPerfectWin, tagPerfectLose,titleDraw;
    public Text totalAnswerCount, totalPlayTime, redScore, blueScore, redTime, blueTime, redName, blueName;
    public GameObject Index01;
    public GameObject Index02;
    public GameObject Badge;
    public GameObject CustomGameHint;
    private const string infiniteTxt = "+\u221e";


    public Button OnceMoreButton;
    public float ShakeInterval = 0.5f;
    public float ShakeStrength = 0.5f;
    public int ShakeTimes;
    
    public PlayerCharatorManager CharatorManager;
    public RectTransform StartPos;
    public RectTransform EndPos;
    public Image InviteHintImage;

    public GameObject TournamentHint;
    public Text TournamentScoreText;
    public ParticleSystem TournamentParticleSystem;
    
    public CanvasGroup ButtonGroup;
    
    public SettlementPlayerInfoComponent RedPlayerInfo;
    public SettlementPlayerInfoComponent BluePlayerInfo;
    public SettlementPlayerInfoComponent UnResolvedInfo;
    public RectTransform InfoContainer;
    public RectTransform WinBG;
    public RectTransform LoseBG;
    
    public Button MatchAgainButton;
    public Button MatchAgainLowVITButton;
    private void OnEnable()
    {
        TiersBadge tiersBadge = Badge.GetComponent<TiersBadge>();
        CustomGameHint.SetActive(MuninnManager.Singleton.GetBattleMode() == BattleMode.OneOnOneCustom);
        var tournament = GameInitManagerLocal.PreviousBattleMode == BattleMode.TournamentOneOnOne;
        TournamentHint.SetActive(tournament);
        Badge.SetActive(!tournament);
        Init();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        if (MuninnManager.Singleton != null)
        {
            MuninnManager.Singleton.OnDisconnectAction -= OnDisconnectTriggerOnceMore;
            MuninnManager.Singleton.OnLeftRoomAction -= OnLeftRoom;
        }
    }

    private void Init()
    {
        Index01.SetActive(false);
        Index02.SetActive(false);

        StartCoroutine(ShowResultPanel());
    }

    private void OnLeftRoom(LeaveRoomEvent room)
    {
        Logger.Log("On CustomRoom LeftRoom");
        MuninnManager.Singleton.OnLeftRoomAction -= OnLeftRoom;
        MuninnManager.Singleton.OnDisconnectAction -= OnDisconnectTriggerOnceMore;
        MuninnManager.Singleton.OnDisconnectAction += OnDisconnectTriggerOnceMore;

    }

    void OnDisconnectTriggerOnceMore()
    {
        Logger.Log("On CustomRoom OnDisconnectTriggerOnceMore");
        GameInitManagerLocal.ReceiveCustomOnceMoreResponse = true;
        MuninnManager.Singleton.OnDisconnectAction -= OnDisconnectTriggerOnceMore;
        if (GameInitManagerLocal.IsCustomOnceMoreSender)
        {
            GameRouter.LoadHomeScene();
        }
        else
        {
            VibrateHelper.VibrateLong();
            StartCoroutine(ShowOnceMoreEffect());

            // UIManager.Instance.ShowPopUp("再来一局？", "对手邀请你再来一局，是否同意？", (() =>
            // {
            //     GameInitManagerLocal.MyInitState = InitState.MatchAgain;
            //     GameRouter.LoadHomeScene();
            //
            // }), (() => { GameRouter.LoadHomeScene(); }));
        }
    }

    YieldInstruction onceMoreWait = new WaitForSeconds(4);
    IEnumerator ShowOnceMoreEffect()
    {
        var dic = new Dictionary<string, string>();
        if (InviteOnceMorePlayer != null)
        {
            dic = InviteOnceMorePlayer.Properties;
        }
        CharatorManager.InitPlayerAvatar(dic);
        CharatorManager.gameObject.SetActive(true);
        CharatorManager.transform.DOMove(EndPos.position, 0.5f).From(StartPos.position).OnComplete((() =>
        {
            InviteHintImage.DOFade(1, 1).From(0);
        }));
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            OnceMoreButton.transform.DoCommonShakeRotationZ(ShakeInterval, ShakeStrength, ShakeTimes);
            yield return onceMoreWait;
        }
    }

    private MuninnPlayer InviteOnceMorePlayer;
    void OnReceiveCustomOnceMore(MuninnPlayer Sender)
    {
        InviteOnceMorePlayer = Sender;
        MuninnMessage.OnCustomOnceMore.RemoveListener(OnReceiveCustomOnceMore);
        MuninnManager.Singleton.OnLeftRoomAction -= OnLeftRoom;
        MuninnManager.Singleton.OnLeftRoomAction += OnLeftRoom;

        //StartCoroutine(LeaveRoomTimeOutDetect());
        RoomManager.LeaveRoom();

    }

    IEnumerator LeaveRoomTimeOutDetect()
    {
        Logger.LogInfo("房间人数>=2,开始检测离开CustomRoom超时");
        yield return new WaitForSeconds(5f);
        Logger.LogInfo("房间人数>=2,离开CustomRoom超时");
        GameInitManagerLocal.MyInitState = InitState.None;
        MuninnManager.Singleton.OnLeftRoomAction -= OnLeftRoom;
        MuninnManager.Singleton.OnDisconnectAction -= OnDisconnectTriggerOnceMore;
        GameRouter.LoadHomeScene();
    }

    public async Task SetBattleScore()
    {
        MuninnMessage.OnCustomOnceMore.AddListener(OnReceiveCustomOnceMore);
        await TiersHelper.UpdatePlayerScore();
    }

    private GetItemParams itemParam;
    public void SetAndShowTier(TeamPlayer self,GetItemParams items)
    {
        itemParam = items;
        var addCount = self.currentScore - self.previousScore;

        ActiveTitle(addCount);
        var tournament = GameInitManagerLocal.PreviousBattleMode == BattleMode.TournamentOneOnOne;
        Badge.SetActive(!tournament);
        if (tournament)
        {
            StartCoroutine(ShowTournamentScoreEffect(self));
            Logger.LogInfo($"锦标赛:当前分数：{self.currentScore},当前等级：{self.currentTier}");
            
            return;
        }
        TiersBadge tiersBadge = Badge.GetComponent<TiersBadge>();
        if (addCount == 0)
        {
            Logger.LogInfo($"SetupBadge:当前分数：{self.currentScore},当前等级：{self.currentTier}");
            tiersBadge?.SetupBadge(true, self.currentScore, self.currentTier);
        }
        else
        {
            ButtonGroup.gameObject.SetActive(false);
            StartCoroutine(tiersBadge?.SetupBadgeWithEffect(self.previousScore, self.currentScore, self.previousTier, self.currentTier,OnCompletedAction));
            StartCoroutine(CheckButtonGroupActive());
        }

    }

    IEnumerator CheckButtonGroupActive()
    {
        yield return new WaitForSeconds(8f);
        Logger.LogInfo("CheckButtonGroupActive");
        ButtonGroup.gameObject.SetActive(true);
    }

    public GameObject EventSystemGO;

    private void OnCompletedAction()
    {
        ButtonGroup.gameObject.SetActive(true);
        ButtonGroup.DOFade(1, 0.5f).From(0);
        if (itemParam != null)
        {
#if UNITY_WEIXINMINIGAME
            if (!EventSystemGO.TryGetComponent<WXTouchInputOverride>(out WXTouchInputOverride inputOverride))
            {
                EventSystemGO.AddComponent<WXTouchInputOverride>();
            }
#endif
            UIManager.Instance.ShowGetItemPanel(itemParam);

        }
        //Debug.LogError("OnCompletedAction" + itemList.Count);
    }

    public Vector3 LabelPunchScale = new Vector3(0.2f, 0.2f, 0.2f);
    public float LabelDuration = 1f;
    IEnumerator ShowTournamentScoreEffect(TeamPlayer self)
    {
        var newScore = self.currentScore;
        var oldScore = self.previousScore;
        TournamentScoreText.text = oldScore.ToString();
        yield return new WaitForSeconds(.5f);
        
        if (newScore > oldScore)
        {
            TournamentScoreText.text = newScore.ToString();
            TournamentParticleSystem.Play();
            TournamentScoreText.transform.DOPunchScale(LabelPunchScale, LabelDuration);
        }
        else if(newScore < oldScore)
        {
            
            TournamentScoreText.DOFade(0, 1f).From(1).OnComplete((() =>
            {
                TournamentScoreText.text = newScore.ToString();
                TournamentScoreText.DOFade(1, 1).From(0);
            }));
        }
        else
        {
            TournamentScoreText.text = newScore.ToString();
        }
    }
    private void ActiveTitle(int addCount)
    {
        bool isPerfect = Math.Abs(addCount) > 1;
        if (BattleEffectManager.instance.IsDraw())
        {
            titleWin.SetActive(false);
            titleLose.SetActive(false);
            
            tagPerfectWin.SetActive(false);
            tagPerfectLose.SetActive(false);
            
            titleDraw.SetActive(true);
        }
        else
        {
            if (BattleEffectManager.instance.IfMyTeamWin() == true)
            {
                titleWin.SetActive(true);
                titleLose.SetActive(false);
            
                tagPerfectWin.SetActive(isPerfect);
                tagPerfectLose.SetActive(false);
                
                WinBG.gameObject.SetActive(true);
            }
            else
            {
                titleWin.SetActive(false);
                titleLose.SetActive(true);
            
                tagPerfectWin.SetActive(false);
                tagPerfectLose.SetActive(isPerfect);
                
                LoseBG.gameObject.SetActive(true);
            }
        }
        
    }

    public void SetPlayerName(string red, string blue)
    {
        redName.text = red;
        blueName.text = blue;
    }

    IEnumerator ShowResultPanel()
    {
        yield return null;

        var pId = Identity.persona.PersonaID;
        int resolvedCount = 0;
        float usedTime = 0;
        MuninnMessageData getData = MuninnMessage.BattleData;

        //count stats
        usedTime = 180 - getData.remainTime;

        int redScoreNum = getData.redTeamProgress.score;
        int blueScoreNum = getData.blueTeamProgress.score;
        resolvedCount = MuninnManager.MyTeamTag.Equals(TeamTag.RED) ? redScoreNum : blueScoreNum;

        //display stats
        redScore.text = redScoreNum.ToString();
        blueScore.text = blueScoreNum.ToString();

        redTime.text = redScoreNum != 0 ? Math.Round(usedTime / redScoreNum, 1).ToString() : infiniteTxt;
        blueTime.text = blueScoreNum != 0 ? Math.Round(usedTime / blueScoreNum, 1).ToString() : infiniteTxt;

        if (MuninnManager.MyTeamTag == TeamTag.RED)
        {
            Index01.SetActive(true);
        }
        else
        {
            Index02.SetActive(true);
        }

        RedPlayerInfo.Init(getData.redTeamProgress);
        RedPlayerInfo.AveTimeText.text = redTime.text;
        BluePlayerInfo.Init(getData.blueTeamProgress);
        BluePlayerInfo.AveTimeText.text = blueTime.text;
        UnResolvedInfo.InitUnResolved(getData.resolvedStatus);
        InfoContainer.gameObject.SetActive(true);
        StartCoroutine(ForceRebuildLayoutImmediate());        
        
        //update player stats
        //fetch remote
        Task updatePlayerStatsTask = UpdatePlayerStats(resolvedCount, usedTime);
        yield return new WaitUntil(() => updatePlayerStatsTask.IsCompleted);
        if (updatePlayerStatsTask.IsFaulted)
        {
            UIMessage.Show("玩家统计更新失败");
        }
    }
    
    IEnumerator ForceRebuildLayoutImmediate()
    {
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(InfoContainer);
        yield return new WaitForEndOfFrame();
        InfoContainer.gameObject.SetActive(false);
        InfoContainer.gameObject.SetActive(true);

    
    }

    async Task UpdatePlayerStats(int resolvedCount, float usedTime)
    {
        await PersonaPropertiesHelper.GetPersonaProperties();
        var resolvedCountRemote = PersonaPropertiesHelper.GetProperty(PersonaPropertyKeys.TotalSolvedCountKey, 0);
        var usedTimeRemote = PersonaPropertiesHelper.GetProperty(PersonaPropertyKeys.TotalTimeUsedKey, 0f);

        Debug.Log($"totalSolved:{resolvedCount}totalTime:{usedTime}");

        resolvedCountRemote += resolvedCount;
        usedTimeRemote += usedTime;

        Dictionary<string, string> updateParams = new Dictionary<string, string>(2);
        updateParams[PersonaPropertyKeys.TotalSolvedCountKey] = resolvedCountRemote.ToString();
        updateParams[PersonaPropertyKeys.TotalTimeUsedKey] = usedTimeRemote.ToString();

        //update
        await PersonaPropertiesHelper.SetPersonaProperties(updateParams);
    }

    [Header("测试")]
    public int testOldScore;
    public int testNewScore;
    public string testOldTier = "钻石";
    public string testNewTier = "钻石";

    public void Test()
    {
        gameObject.SetActive(true);
        TiersBadge tiersBadge = Badge.GetComponent<TiersBadge>();
        StartCoroutine(tiersBadge?.SetupBadgeWithEffect(testOldScore, testNewScore, testOldTier, testNewTier,OnCompletedAction));

    }

    public void TestTournament()
    {
        TournamentHint.SetActive(true);
        Badge.gameObject.SetActive(false);
        TeamPlayer a = new TeamPlayer();
        a.previousScore = testOldScore;
        a.currentScore = testNewScore;
        StartCoroutine(ShowTournamentScoreEffect(a));
    }
}
#if UNITY_EDITOR

[CustomEditor(typeof(BattleSettlementPanel))]
public class LaunchParamsHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BattleSettlementPanel myScript = (BattleSettlementPanel)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("测试"))
        {
            myScript.Test();
        }
        
        if (GUILayout.Button("测试锦标赛"))
        {
            myScript.TestTournament();
        }
        

    }
}
#endif

