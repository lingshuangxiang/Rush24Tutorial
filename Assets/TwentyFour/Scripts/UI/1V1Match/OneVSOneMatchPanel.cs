using System;
using System.Collections;
using System.Collections.Generic;
using TwentyFour.Scripts.LaunchParam;
using TwentyFour.Scripts.PersonaProperty;
using Unity.Muninn.Model;
using Unity.UOS.TwentyFour.Scripts.Battle.Model;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.UI;
using TwentyFour.Scripts.Wechat;
using Unity.Muninn.MuninnLobby;
using Unity.Passport.Runtime.UI;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class OneVSOneMatchPanel : MonoBehaviour
{
    public GameObject RankPanel;

    public GameObject CustomPanel;

    public GameObject BlackCell;

    
    public List<PlayerPanelInfoUpdate> PlayerPanelInfoUpdates = new List<PlayerPanelInfoUpdate>();
    public Button InviteBtn;
    public Button StartBtn;
    

    public Text RoomHint;
    public Text RoomIDText;
    public Button TestCustomRoomBtn;


    private int checker = 0;
    string deepLink = string.Empty;
    string retryRoomId = string.Empty;
    int retryCount = 0;

    public UnityEvent OnShowPanel = new UnityEvent();
    
    public Button OneVsOneMatchBtn;
    public Button OneVsOneLowVITButton;
    
    private void Start()
    {
        if (BlackCell)
        {
            var playerCharator = BlackCell.GetComponent<PlayerCharatorManager>();
            playerCharator.InitPlayerAvatar(PersonaPropertiesHelper.GetLocalProperties());
        }
    }
    
    public void ShowRankPanel()
    {
        OnShowPanel.Invoke();
        gameObject.SetActive(true);
        CustomPanel.SetActive(false);
        BlackCell.SetActive(true);
        RankPanel.SetActive(true);
        RefreshMatchButtons();
        VitalityHelper.Instance.OnVitalityUpdated += RefreshMatchButtons;

    }

    void RefreshMatchButtons()
    {
        if (VitalityHelper.Instance.CurrentVitality >= VitalityHelper.MatchCost)
        {
            OneVsOneMatchBtn.gameObject.SetActive(true);
            OneVsOneLowVITButton.gameObject.SetActive(false);
        }
        else
        {
            OneVsOneMatchBtn.gameObject.SetActive(false);
            OneVsOneLowVITButton.gameObject.SetActive(true);
        }
    }
    public void ShowCustomPanel()
    {
        
        MuninnManager.Singleton.BeforeConnectRoomAction -= OnBeforeConnectRoom;
        MuninnManager.Singleton.BeforeConnectRoomAction += OnBeforeConnectRoom;
        //加入房间成功回调事件
        MuninnManager.Singleton.OnJoinRoomAction -= OnJoinCustomRoom;
        MuninnManager.Singleton.OnJoinRoomAction += OnJoinCustomRoom;
        
        MuninnManager.Singleton.OnJoinRoomFailedAction -= OnJoinRoomFailed;
        MuninnManager.Singleton.OnJoinRoomFailedAction += OnJoinRoomFailed;

        MuninnManager.Singleton.OnDisconnectAction -= OnCreateRoomDisconnect;
        MuninnManager.Singleton.OnDisconnectAction += OnCreateRoomDisconnect;

        RoomManager.CreateRoom(BattleMode.OneOnOneCustom,
            Identity.persona.PersonaID + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        UIManager.Instance.ShowCommonLoading("正在创建房间");
        
        TestCustomRoomBtn.onClick.RemoveAllListeners();
        TestCustomRoomBtn.onClick.AddListener(GetRoomDeepLink);
    }

    private void OnBeforeConnectRoom(JoinRoomResponse obj)
    {
        retryRoomId = obj.RoomUuid;
    }


    void GetRoomDeepLink()
    {
        checker++;
        if (checker >= 10)
        {
            checker = 0;
            UIMessage.Show("复制房间链接至剪切板");
            CopyToClipboard(deepLink);
        }
    }
    void OnJoinCustomRoom(MuninnRoomView roomView)
    {
        retryRoomId = string.Empty;
        MuninnManager.Singleton.BeforeConnectRoomAction -= OnBeforeConnectRoom;

        MuninnManager.Singleton.OnJoinRoomFailedAction -= OnJoinRoomFailed;

        if (roomView.Room.Properties.TryGetValue(CustomRoomPropertyKey.ROOM_STATE, out string state))
        {
            if (CustomRoomPropertyKey.GetCustomRoomState(state) == CustomRoomState.InBattle)
            {
                UIMessage.Show("房间已经失效",MessageType.Error);
                RoomManager.LeaveRoom();
                return;
            }
        }
        
        RefreshPanel();
        MuninnMessage.Init();
        MuninnManager.Singleton.SetBattleMode(BattleMode.OneOnOneCustom);

        //其他玩家加入房间事件
        MuninnManager.Singleton.OnPlayerJoinedAction -= OnPlayerJoined;
        MuninnManager.Singleton.OnPlayerJoinedAction += OnPlayerJoined;
        //其他玩家离开房间事件
        MuninnManager.Singleton.OnPlayerLeftAction -= OnPlayerLeft;
        MuninnManager.Singleton.OnPlayerLeftAction += OnPlayerLeft;
        //本玩家由于被动原因离开房间事件
        MuninnManager.Singleton.OnLeftRoomAction -= OnLeftRoom;
        MuninnManager.Singleton.OnLeftRoomAction += OnLeftRoom;
        
        MuninnManager.Singleton.OnDisconnectAction -= OnDisconnect;
        MuninnManager.Singleton.OnDisconnectAction += OnDisconnect;
        
        
    }

    private void OnDisconnect()
    {
        if(MuninnManager.Singleton == null) return;
        Hide();
        MuninnManager.Singleton.OnDisconnectAction -= OnDisconnect;
        UIManager.Instance.HideCommonLoading();

        UIMessage.Show("网络连接断开,正在重连...");
        MuninnManager.Singleton.OnJoinRoomAction -= OnJoinCustomRoom;
        MuninnManager.Singleton.OnJoinRoomAction += OnJoinCustomRoom;
        
        MuninnManager.Singleton.OnJoinRoomFailedAction -= OnJoinRoomFailed;
        MuninnManager.Singleton.OnJoinRoomFailedAction += OnJoinRoomFailed;
        
        var roomView = MuninnManager.Singleton.GetMuninnRoomView();
        MuninnManager.DelayJoinRoom(1f,BattleMode.OneOnOneCustom,roomView.Room.Id);
        UIManager.Instance.ShowCommonLoading("正在重连");
        MuninnManager.Singleton.OnDisconnectAction += OnDisconnect;

    }

    private void OnCreateRoomDisconnect()
    {
        if(MuninnManager.Singleton == null) return;
        if (!string.IsNullOrEmpty(retryRoomId) && retryCount < 3)
        {
            UIManager.Instance.StartCoroutine(RetryJoinRoom());
        }
        else
        {
            retryCount = 0;
            retryRoomId = string.Empty;
            UIMessage.Show("网络连接断开",MessageType.Error);
            Hide();
            UIManager.Instance.HideCommonLoading();
        }
        
    }
    private void OnJoinRoomFailed(MuninnError obj)
    {
        if(MuninnManager.Singleton == null) return;
        UIMessage.Show("加入房间失败",MessageType.Error);
        Hide();
        UIManager.Instance.HideCommonLoading();
    }

    private void OnLeftRoom(LeaveRoomEvent room)
    {
        Hide();
    }

    IEnumerator RetryJoinRoom()
    {
        UIManager.Instance.ShowCommonLoading("正在加入房间");
        Logger.LogInfo("RetryJoinRoom :" + retryRoomId + " retryCount:" + retryCount);
        yield return new WaitForSeconds(retryCount);
        RoomManager.JoinRoom(BattleMode.OneOnOneCustom,retryRoomId);
        retryCount++;
    }

    /// <summary>
    /// 根据当前房间人数刷新界面显示
    /// </summary>
    public void RefreshPanel()
    {
        UIManager.Instance.HideCommonLoading();
        Logger.Log("RefreshPanel");
        OnShowPanel?.Invoke();

        StartBtn.gameObject.SetActive(false);
        InviteBtn.gameObject.SetActive(true);
        StartBtn.onClick.RemoveAllListeners();
        gameObject.SetActive(true);
        CustomPanel.SetActive(true);
        BlackCell.SetActive(false);
        RankPanel.SetActive(false);
        RoomHint.text = string.Empty;
        var room = MuninnManager.Singleton.GetMuninnRoomView();
        RoomIDText.text = $"房间 ID: {room.Room.Id?.Substring(0, 8)}";
        Logger.LogInfo("RoomID:" + room.Room.Id);
        InviteBtn.onClick.RemoveAllListeners();
        InviteBtn.onClick.AddListener((() =>
        {
            Share.ShareApp(room.Room.Id,BattleMode.OneOnOneCustom);
            deepLink = LaunchParamsHelper.Instance.GenerateDeepLink(BattleMode.OneOnOneCustom,
                Identity.persona.DisplayName, room.Room.Id);
#if UNITY_EDITOR
            CopyToClipboard(deepLink);
            Logger.Log(deepLink);
#endif
        }));

        for (int i = 0; i < PlayerPanelInfoUpdates.Count; i++)
        {
            var panelInfo = PlayerPanelInfoUpdates[i];
            
            panelInfo.gameObject.SetActive(false);
            if (i < room.Players.Count)
            {
                var player = room.Players[i];
                panelInfo.gameObject.SetActive(true);
                panelInfo.ShowPlayerInfo(player);
            }
        }

        var textEffect = RoomHint.GetComponent<TextEllipsisEffect>();
        if (room.Players.Count == 1)
        {
            var hint = "等待玩家加入";
            RoomHint.text = "等待玩家加入";
            textEffect?.SetContent(hint);
        }
        if (room.Players.Count >= 2)
        {
            
            InviteBtn.gameObject.SetActive(false);

            if (MuninnManager.Singleton.IsMasterClient())
            {
                StartBtn.gameObject.SetActive(true);
                StartBtn.onClick.AddListener(() =>
                {
                    MuninnManager.Singleton.StartGame();
                });
                RoomHint.text = string.Empty;
                textEffect?.SetContent(string.Empty);
            }
            else
            {
                var hint = "等待房主开始游戏";
                RoomHint.text = hint;
                textEffect?.SetContent(hint);

            }
            
        }
    }

    private void OnMasterClientChanged(uint obj)
    {
        MuninnManager.Singleton.OnMasterClientChangedAction -= OnMasterClientChanged;
        RefreshPanel();
    }

    void CopyToClipboard(string text)
    {
        CopyPasteUtil.Copy(text);
    }
    private void OnPlayerJoined(MuninnPlayer player)
    {
        retryCount = 0;
        retryRoomId = string.Empty;
        RefreshPanel();
    }
    private void OnPlayerLeft(MuninnPlayer player)
    {
        RefreshPanel();
    }

    private void OnDestroy()
    {
        if(VitalityHelper.Inited && VitalityHelper.Instance != null)
            VitalityHelper.Instance.OnVitalityUpdated -= RefreshMatchButtons;
        if(LaunchParamsHelper.Instance == null) return;
        LaunchParamsHelper.Instance.OneVSOneMatchPanel = null;
        if(MuninnManager.Singleton == null) return;
        LaunchParamsHelper.Instance.RegisterMuninnEvents(false);
        MuninnManager.Singleton.OnJoinRoomFailedAction -= OnJoinRoomFailed;
        MuninnManager.Singleton.OnPlayerJoinedAction -= OnPlayerJoined;
        MuninnManager.Singleton.OnPlayerLeftAction -= OnPlayerLeft;
        MuninnManager.Singleton.OnJoinRoomAction -= OnJoinCustomRoom;
        MuninnManager.Singleton.OnLeftRoomAction -= OnLeftRoom;
        MuninnManager.Singleton.OnDisconnectAction -= OnDisconnect;
        MuninnManager.Singleton.OnDisconnectAction -= OnCreateRoomDisconnect;
        MuninnManager.Singleton.BeforeConnectRoomAction -= OnBeforeConnectRoom;
        MuninnManager.Singleton.OnMasterClientChangedAction -= OnMasterClientChanged;


    }

    private void OnDisable()
    {
        if(VitalityHelper.Inited && VitalityHelper.Instance != null)
            VitalityHelper.Instance.OnVitalityUpdated -= RefreshMatchButtons;
    }

    public void Hide()
    {
        retryRoomId = string.Empty;
        retryCount = 0;
        checker = 0;
        deepLink = string.Empty;
        gameObject.SetActive(false);
        if(MuninnManager.Singleton == null) return;
        MuninnManager.Singleton.OnJoinRoomFailedAction -= OnJoinRoomFailed;
        MuninnManager.Singleton.OnPlayerJoinedAction -= OnPlayerJoined;
        MuninnManager.Singleton.OnPlayerLeftAction -= OnPlayerLeft;
        MuninnManager.Singleton.OnJoinRoomAction -= OnJoinCustomRoom;
        MuninnManager.Singleton.OnLeftRoomAction -= OnLeftRoom;
        MuninnManager.Singleton.OnDisconnectAction -= OnDisconnect;
        MuninnManager.Singleton.OnDisconnectAction -= OnCreateRoomDisconnect;
        MuninnManager.Singleton.OnMasterClientChangedAction -= OnMasterClientChanged;
        MuninnManager.Singleton.BeforeConnectRoomAction -= OnBeforeConnectRoom;

        RoomManager.LeaveRoom();



        
    }
}
