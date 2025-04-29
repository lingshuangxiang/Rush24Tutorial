using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TwentyFour.Scripts.Common;
using Unity.Push.Model;
using Unity.UOS.Push;
using Unity.UOS.Push.Exception;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#endif
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public static class PushHelper
{
    static YieldInstruction waitForSeconds = new WaitForSeconds(3f);
    private static bool inited = false;
    public static async Task Initialize()
    {
        if(inited) return;
        try
        {
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
            WX.OnShow(OnWXShow);
#endif
            Logger.LogInfo("[Push]Initializing...");
            await PushSDK.InitializeAsync(onConnected, onMessage, onDisconnected, onSubscribe, onRemoteLogin);
            inited = true;
        }
        catch (PushSDKClientException ex)
        {
            Logger.LogError($"[Push] Failed to init sdk. client ex: {ex}");
            throw;
        }
        catch (PushSDKServerException ex)
        {
            Logger.LogError($"[Push] Failed to init sdk. server ex: {ex}");
            throw;
        }
    }
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
    private static void OnWXShow(OnShowListenerResult obj)
    {
        CoroutineUtil.Instance.StartCoroutine(CheckAndReconnect());
    }
#endif

    public static void Disconnect()
    {
        inited = false;
        if (PushSDK.Status == PushSDK.StatusDisconnected)
        {
            return;
        }
        Logger.Log($"[Push]Manul Disconnecting...");
        PushSDK.Instance.Disconnect();
    }
    static IEnumerator CheckAndReconnect()
    {
        yield return waitForSeconds;
        if (PushSDK.Status == PushSDK.StatusConnectFailure || PushSDK.Status == PushSDK.StatusDisconnected)
        {
            Logger.LogInfo("[Push] Trying to reconnect to server after 3 seconds.");
            ConnectAsync();
        }
        else if(PushSDK.Status == PushSDK.StatusConnecting)
        {
            Logger.LogInfo("[Push] PushSDK is connecting");
        }
    }

    public static async Task ConnectAsync()
    {
        try
        {
            await PushSDK.Instance.ConnectAsync(Identity.persona.PersonaID);
        }
        catch (PushSDKClientException ex)
        {
            Logger.LogError($"[Push] Failed to connect server. clientEx {ex}");
        }
        catch (PushSDKServerException ex)
        {
            Logger.LogError($"[Push] Failed to connect server. serverEx {ex}");
        }
    }

    private static async void onConnected(ConnectResult connectResult)
    {
        if (connectResult.Success)
        {
            Logger.LogInfo("[Push] player connected");
            // 如果连接成功，订阅需要订阅的频道

            // 获取当前公共频道信息
            var publicChannelInfos = await PushSDK.Instance.ListPublicChannelInfoAsync();
            foreach (var channelInfo in publicChannelInfos)
            {
                // 查看当前所有公共频道在线人数信息
                Logger.Log(
                    $"public channel name: {channelInfo.Name}, online player count: {channelInfo.OnlinePlayerCount}");
            }

            // 这里直接默认订阅了所有公共频道，实际过程中你可以选择根据需要订阅哪些
            var publicChannels = publicChannelInfos.Select(channelInfo => channelInfo.Name);
            PushSDK.Instance.SubscribeChannels(publicChannels, true);

            // // 订阅一些其他需要订阅的频道信息
            // var channels = new List<string> { "team-001", "guild-002" };
            // PushSDK.Instance.SubscribeChannels(channels, false);
        }
        else
        {
            // 连接失败，打印失败原因，可重新调用Connect再次尝试连接
            Logger.LogError($"player connect failure, msg: {connectResult.Msg}");
            ConnectAsync();
        }
    }

    private static void onMessage(PushMessage message)
    {
        // 打印消息类型，查看是频道消息还是玩家消息
        // Debug.Log("收到消息的类型: " + message.MessageType);
        // if (message.MessageType == MessageType.PublicChannelMessage)
        // {
        //     Debug.Log($"收到消息为公共频道消息，其频道名称为:{message.ChannelName}");
        // }
        //
        // Debug.Log("消息内容: " + System.Text.Encoding.UTF8.GetString(message.Data));
    }

    private static void onDisconnected(DisconnectResult disconnectResult)
    {
        
        if (!disconnectResult.IsNormalClosed)
        {
            Logger.LogError($"[Push] player disconnected");
            Debug.Log("[Push] Player abnormally disconnect, use Connect method to reconnect");
            // 该方法可重新找到一个正常工作的服务器供连接
            ConnectAsync();
        }
    }

    private static void onRemoteLogin()
    {
        Logger.LogError("[Push] 该账户被异地登陆");
        // 退出程序
        PushSDK.Instance.Disconnect();
        UIManager.Instance.ShowPopUp("提示", "该账户已在其他地方登录", (Unity.UOS.TwentyFour.Common.Utils.ExitGame), null, false);
    }

    private static void onSubscribe(SubscribeResult result)
    {
        Logger.Log($"[Push] 订阅结果：{result.ResultCode}");
        Logger.Log($"[Push] 订阅成功的频道: {JsonConvert.SerializeObject(result.SuccessfulChannelNames)}");
    }
}