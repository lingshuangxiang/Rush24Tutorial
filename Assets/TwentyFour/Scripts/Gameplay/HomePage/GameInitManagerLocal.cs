using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TwentyFour.Scripts.Utilities;

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
    public static bool IsCustomOnceMoreSender;
    public static bool ReceiveCustomOnceMoreResponse;
    public static string PreviousRoomId;
    public GameObject matchCanvas;
    public Button matchButton;

    public Button BattleButton;
    public UnityEvent OnStartEvent;

    void Awake()
    {
    }
    private void Start()
    {
        OnStartEvent?.Invoke();
        IsCustomOnceMoreSender = false;
    }

    public void OnEnable()
    {
        if (MyInitState == InitState.MatchAgain)
        {
 
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
        }
    }

    IEnumerator Tournament(bool matchAgain = false)
    {
        yield return null;
    }
}