using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TwentyFour.Scripts.LaunchParam;
using Unity.Passport.Runtime.UI;
using Unity.UOS.TwentyFour.Scripts.Battle.Model;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum InitState
{
    None,
    OpponentQuits,
    MatchAgain,
    ShowTournamentPanel
}

public class GameInitManagerLocal : MonoBehaviour
{
    public static InitState MyInitState = InitState.None;
    public static BattleMode PreviousBattleMode;
    public static bool IsCustomOnceMoreSender;
    public static bool ReceiveCustomOnceMoreResponse;
    public static string PreviousRoomId;
    public GameObject matchCanvas;
    public Button matchButton;

    public TournamentPanel TournamentPanel;
    public PlayerInfoPanel PlayerInfoPanel;
    public Button BattleButton;
    public UnityEvent OnStartEvent;

    void Awake()
    {
    }
    private void Start()
    {
        OnStartEvent?.Invoke();
        IsCustomOnceMoreSender = false;
        LaunchParamsHelper.Instance.OneVSOneMatchPanel = matchCanvas.GetComponent<OneVSOneMatchPanel>();
        LaunchParamsHelper.Instance.ParseLaunchParams();
        UIManager.Instance.PlayerInfoPanelInstance = PlayerInfoPanel;
    }

    public void OnEnable()
    {
        if (MyInitState == InitState.MatchAgain)
        {
            MuninnManager.Singleton.SetBattleMode(PreviousBattleMode);
            if (PreviousBattleMode == BattleMode.OneOnOne)
            {
                matchCanvas.GetComponent<OneVSOneMatchPanel>().ShowRankPanel();
                matchButton.onClick.Invoke();
            }
            else if(PreviousBattleMode == BattleMode.OneOnOneCustom)
            {
                MuninnMessage.Clear();
                LaunchParamsHelper.Instance.ReJoinRoom(PreviousRoomId, BattleMode.OneOnOneCustom);
                PreviousRoomId = string.Empty;
                ReceiveCustomOnceMoreResponse = false;
                IsCustomOnceMoreSender = false;
            }
            else if(PreviousBattleMode == BattleMode.TournamentOneOnOne)
            {
                StartCoroutine(Tournament(true));
            }

            PreviousBattleMode = BattleMode.None;
            MyInitState = InitState.None;
        }
        else if (MyInitState == InitState.OpponentQuits)
        {
            UIMessage.Show("对手退出！");
            //StartCoroutine(myMatchMakingManager.ShowHint("对手退出！"));
            MyInitState = InitState.None;
        }
        else if (MyInitState == InitState.ShowTournamentPanel)
        {
            StartCoroutine(Tournament());
            MyInitState = InitState.None;
            PreviousBattleMode = BattleMode.None;
        }
    }

    IEnumerator Tournament(bool matchAgain = false)
    {
        yield return null;
        TournamentPanel.gameObject.SetActive(true);
        if (matchAgain)
            TournamentPanel.MatchMakingButton.onClick.Invoke();
    }
}