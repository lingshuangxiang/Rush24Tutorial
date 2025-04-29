using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Passport;
using TwentyFour.Scripts.RemoteConfig;
using TwentyFour.Scripts.Tournament;
using Unity.Muninn;
using Unity.Muninn.Model;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.Model;
using Unity.UOS.Matchmaking;
using Unity.UOS.Matchmaking.Exception;
using Unity.UOS.Matchmaking.Model;
using Unity.UOS.Auth;
using Unity.UOS.Security;
using Unity.UOS.TwentyFour.Scripts.Battle.Model;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine.Events;
using Unity.Passport.Runtime.UI;
using UnityEngine;
using Logger = Unity.UOS.TwentyFour.Common.Logger;
using Unity.UOS.TwentyFour.Robot;
using Random = UnityEngine.Random;

public class MatchMakingManager : MonoBehaviour
{
    public string TournamentSlugName;
    public UnityEvent<string> MatchStateEvent = new UnityEvent<string>();
    public UnityEvent OnCancelEvent = new UnityEvent();
    public UnityEvent OnMatchSuccessEvent = new UnityEvent();
    public UnityEvent OnAwaitingAssignment = new UnityEvent();

    
    public UnityEvent OnJoinedRoomEvent = new UnityEvent();
    CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource();
    CancellationToken m_cancellationToken;
    string ticketId = string.Empty;
    public RectTransform matchEventHint;
    public static bool IsMatchedAndInRoom = false;

    private readonly YieldInstruction getTicketIntervalSeconds = new WaitForSeconds(1f);
    private readonly YieldInstruction joinRoomTimeoutSeconds = new WaitForSeconds(10f);
    
    private bool enablePolling = false;

    public int MatchMakingScore;
    private BattleMode tempBattleMode;
    void RegisterMuninnEvent()
    {
        MuninnManager.Singleton.OnLeftRoomAction += OnLeftRoomEvent;
        MuninnManager.Singleton.OnDisconnectAction += OnDisconnectEvent;
        MuninnManager.Singleton.OnPlayerLeftAction += OnPlayerLeftAction; 
    }

    
    void UnRegisterMuninnEvent()
    {
        //MuninnMessage.Clear();
        if(MuninnManager.Singleton == null) return;
        MuninnManager.Singleton.OnLeftRoomAction -= OnLeftRoomEvent;
        MuninnManager.Singleton.OnJoinRoomAction -= OnJoinRoomEvent;
        MuninnManager.Singleton.OnJoinRoomFailedAction -= OnJoinRoomFailedEvent;
        MuninnManager.Singleton.OnDisconnectAction -= OnDisconnectEvent;
        MuninnManager.Singleton.OnDisconnectAction -= OnCreateRoomDisconnect;
        MuninnManager.Singleton.OnPlayerLeftAction -= OnPlayerLeftAction; 
    }

    private void OnDestroy()
    {
        MatchMakingConfigID = string.Empty;
        tempBattleMode = BattleMode.None;
        UnRegisterMuninnEvent();
        
    }


    void OnApplicationQuit()
    {
        TryDeleteTicket();
    }


    #region 匹配相关

    /// <summary>
    /// 从开始匹配到匹配成功的全部逻辑 
    /// </summary>
    public void MatchMaking()
    {
        ScreenOption.IsKeepScreenOn = true;
        IsMatchedAndInRoom = false;
        retryCount = 0;
        WaitMatchMaking();
    }

    void WaitMatchMaking()
    {
        MatchStateEvent?.Invoke("开始匹配"); //返回空的物体
        UnRegisterMuninnEvent();
        RoomManager.LeaveRoom();
        StopCoroutine(BattleDataTimeoutDetect());
        m_cancellationTokenSource = new CancellationTokenSource();
        m_cancellationToken = m_cancellationTokenSource.Token;
        
        // 生成本次匹配的机器人随机创建时间
        RobotHelper.GenRandomCreateRobotTime();
        
        CreateTicket();
    }

    // Start is called before the first frame update
    public static void StartInitializeMatchMaking()
    {
        try
        {
            // 默认与 UOS Launcher 中填写的 UOS APP 关联
            // 如需与其他 UOS APP 关联，可以使用 MatchmakingSDK.Initialize(string appId, string appSecret, string usePassportIdentity) 方法
            // 当使用 UOS App Passport 作为登录系统时，传入true，否则传入false
            MatchmakingSDK.Initialize(true);
        }
        catch (MatchmakingClientException e)
        {
            Logger.LogError($"StartInitializeMatchMaking:failed to initialize sdk, clientEx: {e.Message}");
            throw;
        }
    }

    public string MatchMakingConfigID;
    public UnityEvent OnCreateTicketSucceedEvent;
    public async void CreateTicket()
    {
        StopCoroutine("PollingTicketStatus");
        
        UosAppConfigs getConfig = UosAppConfigs.GetUosAppConfigs();
        string configId = getConfig.MatchConfigId; // configId 即你在网页上创建匹配配置Id, configId决定了该ticket使用的匹配规则
        ticketId = string.Empty;
        if(!string.IsNullOrEmpty(MatchMakingConfigID))
        {
            configId = MatchMakingConfigID;
            MatchMakingScore = (int)PersonaPropertiesHelper.MyLeaderboardScore.Score;
        }

        tempBattleMode = MuninnManager.Singleton.GetBattleMode();
        Player player = new Player
        {
            // player 即玩家信息，单人匹配携带一个玩家，组队匹配携带多个玩家
            // 每个玩家还可以携带匹配规则需要的段位信息，游戏模式，游戏地图数据信息等
            // 亦可携带更多自定义数据，所有玩家数据将传到游戏服务端
            id = Identity.persona.PersonaID,
            attributes = new Dictionary<string, string>
            {
                {"score", MatchMakingScore.ToString()},
                {"battlemode",$"{(int)MuninnManager.Singleton.GetBattleMode()}"}
            }
        };
        try
        {
            var timestamp = EncryptManager.GetUnixTimeStampSeconds(DateTime.UtcNow);
            Logger.LogInfo("CreateTicket: " + timestamp);
            ticketId = await MatchmakingSDK.Instance.CreateTicketAsync(configId, new List<Player> { player });
            enablePolling = true;
            OnCreateTicketSucceedEvent?.Invoke();
            
            StartCoroutine("PollingTicketStatus");
        }
        catch (MatchmakingClientException e)
        {
            ResolveException(e, "CreateTicket");
            CancelTicket();
        }
        catch (MatchmakingServerException e)
        {
            ResolveException(e, "CreateTicket", "serverException");
            CancelTicket();
        }
    }

    
    private Ticket currentTicket;

    private async Task GetTicket()
    {
        try
        {
            currentTicket = await MatchmakingSDK.Instance.GetTicketAsync(ticketId);
        }
        catch (MatchmakingClientException e)
        {
            ResolveException(e, "MatchmakingManager");
            // Logger.LogError(e.Message);
        }
        catch (MatchmakingServerException e)
        {
            ResolveException(e, "MatchmakingManager","serverException");
        }
    }

    private async Task TryDeleteTicket()
    {
        try
        {
            if (string.IsNullOrEmpty(ticketId))
            {
                return;
            }

            await MatchmakingSDK.Instance.DeleteTicketAsync(ticketId);
            Logger.LogInfo("TryDeleteTicket: " + ticketId);
            ticketId = string.Empty;
            currentTicket = null;
        }
        catch (MatchmakingClientException e)
        {
            Logger.LogError($"failed to delete ticket, clientEx: {e.Message}");
            throw;
        }
        catch (MatchmakingServerException e)
        {
            Logger.LogError($"failed to delete ticket, serverEx: {e.Message}");
            throw;
        }
    }

    IEnumerator PollingTicketStatus()
    {
        // Debug.Log("myTicket:"+ticketId.);

        while (enablePolling)
        {
            //wait
            yield return getTicketIntervalSeconds;

            Task getTicketTask = GetTicket();
            yield return new WaitUntil(() => getTicketTask.IsCompleted);
            if (getTicketTask.IsFaulted)
            {
                continue;
            }

            if(currentTicket == null) continue;
            MatchStateEvent?.Invoke(currentTicket?.status);
            

            // MatchStateEvent?.Invoke(currentTicket.status);
            switch (GetMatchMakingTicketStatus(currentTicket.status))
            {
                case MatchMakingTicketStatus.Created:
                    RandomCreateRobot();
                    break;
                case MatchMakingTicketStatus.Matched:
                    OnMatchSuccessEvent?.Invoke();
                    Logger.LogInfo("TicketId" + currentTicket.id + "玩家数量 ："+currentTicket.players.Count);
                    var sb = new StringBuilder();
                    foreach (var pl in currentTicket.players)
                    {
                        sb.AppendLine($"玩家 {pl.id} 信息如下：");
                        foreach (var kv in pl.attributes)
                        {
                            sb.AppendLine($"{kv.Key} = {kv.Value}");
                        }
                    }
                    Logger.LogInfo(sb);
                    //reset muninn message and ready to receive battle questions;
                    MuninnMessage.Init();
                    MuninnManager.Singleton.OnJoinRoomAction -= OnJoinRoomEvent;
                    MuninnManager.Singleton.OnJoinRoomAction += OnJoinRoomEvent;
                    MuninnManager.Singleton.OnJoinRoomFailedAction -= OnJoinRoomFailedEvent;
                    MuninnManager.Singleton.OnJoinRoomFailedAction += OnJoinRoomFailedEvent;
                    MuninnManager.Singleton.OnDisconnectAction -= OnCreateRoomDisconnect;
                    MuninnManager.Singleton.OnDisconnectAction += OnCreateRoomDisconnect;
                    MuninnManager.JoinRoom(BattleMode.OneOnOne, currentTicket.assignment.roomId);
                    Logger.Log($"ticketAssignment:{currentTicket.assignment.gamePorts}，{currentTicket.assignment.gamePorts}");
                    
                    yield break;
                //  Debug.Log("TicketallPlayers"+ticket.);
                case MatchMakingTicketStatus.Error:
                    if (MuninnManager.Singleton.GetBattleMode() == BattleMode.TournamentOneOnOne)
                    {
                        UIMessage.Show("匹配超时！");
                        CancelTicket();
                    }
                    else
                    {
                        CreateRobot();
                    }
                    yield break;

                case MatchMakingTicketStatus.AwaitingAssignment:
                    OnAwaitingAssignment?.Invoke();
                    break;

                default:

                    break;
            }
            
        }
    }

    int retryCount = 0;
    private void OnCreateRoomDisconnect()
    {
        if (retryCount < 3)
        {
            StartCoroutine(RetryJoinRoom());
        }
        else
        {
            retryCount = 0;
            UnRegisterMuninnEvent();
            tempBattleMode = BattleMode.None;
            OnCancelEvent?.Invoke();
        }
        
        
    }
    IEnumerator RetryJoinRoom()
    {
        if (currentTicket is { assignment: not null })
        {
            UIManager.Instance.ShowCommonLoading("正在加入房间");
        
            yield return new WaitForSeconds(retryCount + 1);
            MuninnManager.Singleton.SetBattleMode(tempBattleMode);
            Logger.LogInfo("RetryJoinRoom Ticket RetryID:" + currentTicket.assignment.roomId + " retryCount:" + retryCount);
            RoomManager.JoinRoom(tempBattleMode,currentTicket.assignment.roomId);
            retryCount++;
        }
        else if (!string.IsNullOrEmpty(MuninnManager.retryRoomId))
        {
            UIManager.Instance.ShowCommonLoading("正在加入房间");
            yield return new WaitForSeconds(retryCount + 1);
            MuninnManager.Singleton.SetBattleMode(tempBattleMode);
            Logger.LogInfo("RetryJoinRoom MuninnRetryID:" + MuninnManager.retryRoomId + " retryCount:" + retryCount);
            RoomManager.JoinRoom(tempBattleMode,MuninnManager.retryRoomId);
            retryCount++;
        }
        else
        {
            retryCount = 0;
            UnRegisterMuninnEvent();
            tempBattleMode = BattleMode.None;
            OnCancelEvent?.Invoke();
        }
        
    }
    private void OnJoinRoomFailedEvent(MuninnError obj)
    {
        UnRegisterMuninnEvent();
        OnCancelEvent?.Invoke();
    }
    
    private void OnDisconnectEvent()
    {
        UnRegisterMuninnEvent();
        RoomManager.LeaveRoom();
        OnCancelEvent?.Invoke();
    }
    private void OnPlayerLeftAction(MuninnPlayer obj)
    {
        UnRegisterMuninnEvent();
        OnCancelEvent?.Invoke();
    }

    private void OnJoinRoomEvent(MuninnRoomView roomView)
    {
        OnJoinedRoomEvent?.Invoke();
        if (!string.IsNullOrEmpty(TournamentSlugName) && TournamentData.Current)
        {
            Dictionary<string, string> properties = MuninnManager.GetRoom().Room.Properties;;
            properties[CustomRoomPropertyKey.TournamentSlugName] = TournamentSlugName;
            foreach (var referenceData in TournamentData.Current.ReferenceSlugs)
            {
                if (referenceData.SlugType == TournamentDataReferenceSlugType.Leaderboards)
                {
                    foreach (var kv in referenceData.Datas.ToDictionary())
                    {
                        properties[kv.Key] = kv.Value;
                    }
                }
            }
            RoomManager.UpdateRoomCustomProperties(properties, callback =>
            {
                Logger.Log("UpdateRoomCustomProperties");
            });
        }
        
        Logger.LogInfo("MatchMakingManager::OnJoinRoomEvent");
        MuninnManager.Singleton.OnJoinRoomAction -= OnJoinRoomEvent;
        RegisterMuninnEvent();
        StopCoroutine("BattleDataTimeoutDetect");
        StartCoroutine("BattleDataTimeoutDetect");
    }


    /// <summary>
    /// 如果匹配成功后。
    /// </summary>
    /// <returns></returns>
    IEnumerator BattleDataTimeoutDetect()
    {
        yield return joinRoomTimeoutSeconds;
        //如果双方成功进入房间，将收到服务器发牌
        //因此，如果在Timeout时间内未收到发牌，则视作双方未能成功连接进游戏，
        //此时退出房间回到匹配页面
        Logger.LogInfo("MatchMakingManager::BattleDataTimeoutDetect");
        if (MuninnMessage.BattleData.stages == null)
        {
            OnCancelEvent?.Invoke();
            UnRegisterMuninnEvent();
            MuninnManager.Singleton.SetBattleMode(BattleMode.None);
            Logger.LogInfo("MatchMakingManager::BattleDataTimeoutDetect 对手已退出！");
            UIMessage.Show("对手已退出！");
            RoomManager.LeaveRoom();//离开房间
            MuninnMessage.Clear();
        }
    }

    /// <summary>
    /// 检查是否可以被取消
    /// </summary>
    /// <returns></returns>
    private bool IsMatched()
    {
        return "awaitingAssignment".Equals(currentTicket?.status) || "matched".Equals(currentTicket?.status);
    }

    /// <summary>
    /// 链接失败的时候退出房间。。
    /// </summary>
    /// <param name="e"></param>
    void OnLeftRoomEvent(LeaveRoomEvent e)
    {
        OnCancelEvent?.Invoke();
        UnRegisterMuninnEvent();
        LeaveRoomReason reason = e.reason;
        Logger.LogInfo($"[Muninn]: OnLeftRoom() was called by Muninn. Reason : {reason}");
        MuninnManager.Singleton.SetBattleMode(BattleMode.None);
        switch (reason)
        {
            case LeaveRoomReason.DisconnectByClient:
                if(!RoomManager.LeaveRoomBySelf)
                    UIMessage.Show("断连", MessageType.Error);
                //StartCoroutine(ShowHint("断连"));
                break;
            case LeaveRoomReason.DisconnectByKick:
                UIMessage.Show("被踢出房间", MessageType.Error);
                //StartCoroutine(ShowHint("被踢出房间"));
                break;
            case LeaveRoomReason.DisconnectByTimeout:
                UIMessage.Show("匹配超时", MessageType.Error);
                //StartCoroutine(ShowHint("匹配超时"));
                break;
            case LeaveRoomReason.DisconnectByCloseRoom:
                UIMessage.Show("房间关闭", MessageType.Error);
                //StartCoroutine(ShowHint("房间关闭"));
                break;
        }

        

    }

    public void HideCommonLoading()
    {
        UIManager.Instance.HideCommonLoading();
    }
    


    
    public async void CancelTicket()
    {
        CancelCurrentTicket();
        OnCancelEvent?.Invoke();
    }

    /// <summary>
    /// 取消匹配并且删除ticket
    /// </summary>
    void CancelCurrentTicket()
    {
        ScreenOption.IsKeepScreenOn = false;
        tempBattleMode = BattleMode.None;
        enablePolling = false;
        m_cancellationTokenSource?.Cancel();
        StopAllCoroutines();
        TryDeleteTicket();
    }

    #endregion

    public void ResolveException(Exception e, string debugTitle = "",  string type="clientException")
    {
        // OnCancelEvent?.Invoke();
        // enablePolling = false;
        // StopCoroutine(PollingTicketStatus());
        // TryDeleteTicket();
        // currentTicket = null;
        // string[] needToCancel = { "CreateTicket" };
        Logger.LogError(type + ":" + debugTitle
                       + ":" + e);
        
        UIMessage.Show(type == "clientException"?"客户端异常":"服务器异常");
        
        // if (needToCancel.Contains(debugTitle))
        // {
        //     OnCancelEvent?.Invoke();
        // }
        
        // if (clientException != "")
        // {
        //     Debug.LogError("clientException" + ":" + debugTitle
        //                    + ":" + clientException);
        //     
        //     UIMessage.Show();
        //     // ShowHint("客户端匹配错误");
        // }
        //
        // if (serverException != "")
        // {
        //     Debug.LogError("serverException" + ":" + debugTitle
        //                    + ":" + serverException);
        //     // ShowHint("服务端匹配错误");
        // }
        //
        //
        // clientException = "";
        // serverException = "";
    }

    public MatchMakingTicketStatus GetMatchMakingTicketStatus(string status)
    {
        if (Enum.TryParse(typeof(MatchMakingTicketStatus), currentTicket.status, true, out var currentTicketStatus))
        {
            return (MatchMakingTicketStatus)currentTicketStatus;
        }

        return MatchMakingTicketStatus.Error;
    }

    public void SetBattleMode(int battleMode)
    {
        MuninnManager.Singleton.SetBattleMode(battleMode);
    }

    /// <summary>
    /// 随机创建机器人
    /// </summary>
    private void RandomCreateRobot()
    {
        // 锦标赛模式不使用机器人
        if (MuninnManager.Singleton.GetBattleMode() == BattleMode.TournamentOneOnOne) return;
        
        if (RobotHelper.ShouldCreateRobot())
        {
            // 取消匹配
            CancelCurrentTicket();
            // 创建机器人
            CreateRobot();
        }
    }

    /// <summary>
    /// 创建机器人
    /// </summary>
    private async void CreateRobot()
    {
        // 匹配成功：
        OnAwaitingAssignment?.Invoke();

        MuninnMessage.Init();
        MuninnManager.Singleton.OnJoinRoomAction -= OnJoinRoomEvent;
        MuninnManager.Singleton.OnJoinRoomAction += OnJoinRoomEvent;
        MuninnManager.Singleton.OnJoinRoomFailedAction -= OnJoinRoomFailedEvent;
        MuninnManager.Singleton.OnJoinRoomFailedAction += OnJoinRoomFailedEvent;
        MuninnManager.Singleton.OnDisconnectAction -= OnCreateRoomDisconnect;
        MuninnManager.Singleton.OnDisconnectAction += OnCreateRoomDisconnect;

        // 创建并加入机器人房间
        var customProperties = new Dictionary<string, string>();
        try
        {
            await RobotHelper.AddRobotProperties(customProperties);
            RoomManager.CreateRoom(BattleMode.OneOnOne, "CustomRobot", "", true, customProperties);
        }
        catch (Exception e)
        {
            Logger.LogError($"CreateRobot: {e.Message}");
            UIMessage.Show("对手已退出！");
            OnCancelEvent?.Invoke();
            throw;
        }
        
    }
}

public enum MatchMakingTicketStatus
{
    Created,
    Error,
    Matched,
    AwaitingAssignment,
}