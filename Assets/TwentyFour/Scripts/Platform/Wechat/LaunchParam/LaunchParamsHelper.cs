using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Muninn.Model;
using Unity.Passport.Runtime.UI;
using Unity.UOS.Common;
using Unity.UOS.TwentyFour;
using Unity.UOS.TwentyFour.Scripts.Battle.Model;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.SceneManagement;
using TwentyFour.Scripts.Wechat;
using Unity.UOS.TwentyFour.Common;
using UnityEditor;
using Logger = Unity.UOS.TwentyFour.Common.Logger;
using MessageType = Unity.Passport.Runtime.UI.MessageType;

namespace TwentyFour.Scripts.LaunchParam
{
    public class LaunchParamsHelper : SingletonBehaviour<LaunchParamsHelper>
    {
        public OneVSOneMatchPanel OneVSOneMatchPanel;
        private Dictionary<string, string> launchParams = new();

        string gameInfo = string.Empty;
        BattleMode battleMode = BattleMode.None;
        string fromPlayer = string.Empty;
        string fromPlayerName = string.Empty;
        string fromPlayerTier = string.Empty;
        string roomId = string.Empty;
        
        static bool IsLaunchParamParsed = false;

        [Header("支持仅填入RoomUuid或完整DeepLink")]
        public string TestDeepLink;
        
        int retryCount = 0;


        public void Init()
        {
            Clear();
            //ParseDeepLinkToLaunchParams();
            Share.AddWxOnShowHandler(ParseDeepLinkToOpenParams);
        }

        public void ParseLaunchParams()
        {
            AysncLoadingScenenEf.OnUnloadLoadingCompletedAction -= ParseDeepLinkToLaunchParams;
            AysncLoadingScenenEf.OnUnloadLoadingCompletedAction += ParseDeepLinkToLaunchParams;
        }

        private void ParseDeepLinkToLaunchParams()
        {
            AysncLoadingScenenEf.OnUnloadLoadingCompletedAction -= ParseDeepLinkToLaunchParams;
            if(IsLaunchParamParsed)
                return;
            Clear();
            Logger.LogInfo("执行 ParseDeepLinkToLaunchParams");
            launchParams = Share.GetLaunchOptionsSync().ToDictionary(x => x.Key, x => x.Value);
            Logger.LogInfo(launchParams.Count+"launch params");
            if (launchParams.Any())
            {
                launchParams.TryGetValue("battlemode", out string battleModeStr);
                if (!string.IsNullOrEmpty(battleModeStr))
                    battleMode = (BattleMode)Enum.Parse(typeof(BattleMode), battleModeStr, true);
                launchParams.TryGetValue("fromplayername", out fromPlayerName);
                launchParams.TryGetValue("roomid", out roomId);
                CheckLaunchParams();
            }
            IsLaunchParamParsed = true;
        }
        
        private void ParseDeepLinkToOpenParams(Dictionary<string, string> openParams)
        {
            Clear();
            Logger.LogInfo("执行 ParseDeepLinkToOpenParams");
            launchParams = openParams.ToDictionary(x => x.Key, x => x.Value);
            Logger.LogInfo(launchParams.Count+"open params");
            if (launchParams.Any())
            {
                launchParams.TryGetValue("battlemode", out string battleModeStr);
                if (!string.IsNullOrEmpty(battleModeStr))
                    battleMode = (BattleMode)Enum.Parse(typeof(BattleMode), battleModeStr, true);
                launchParams.TryGetValue("fromplayername", out fromPlayerName);
                launchParams.TryGetValue("roomid", out roomId);
                
                CheckLaunchParams();
            }
        }

        void CheckLaunchParams()
        {

            Scene scene = SceneManager.GetActiveScene();
            //Logger.Log($"当前活跃场景 {scene.name}");
            switch (scene.name)
            {
                case GameRouter.StartScene:
                    break;
                case GameRouter.MainScene:
                    Debug.Log("执行 JoinCustomRoom");
                    MuninnManager.FetchRoomInfo(roomId,(response =>
                    {
                        if (response.Status is MuninnLobbyRoomStatus.Closed or MuninnLobbyRoomStatus.Unknown 
                            or MuninnLobbyRoomStatus.AllocatedFailed)
                        {
                            UIMessage.Show("房间已关闭");
                            Clear();
                        }
                        else
                        {
                            if (response.CustomProperties!= null && 
                                response.CustomProperties.TryGetValue(CustomRoomPropertyKey.ROOM_STATE, out string state))
                            {
                                if (CustomRoomPropertyKey.GetCustomRoomState(state) == CustomRoomState.InBattle)
                                {
                                    UIMessage.Show("房间已开始对战，无法加入");
                                    RoomManager.LeaveRoom();
                                    Clear();
                                }
                                else
                                {
                                    //房间状态为未开始，可加入
                                    JoinCustomRoom();
                                }
                            }
                            else
                            {
                                JoinCustomRoom();
                            }
                        }
                    }));
                    break;
                case GameRouter.BattleScene:
                    if(roomId == MuninnManager.Singleton.GetMuninnRoomView().Room.Id)
                        UIMessage.Show("已在房间！");
                    else
                        UIMessage.Show("对战中，无法加入其他房间！");
                    break;
                case GameRouter.StageScene:
                    MuninnManager.FetchRoomInfo(roomId,(response =>
                    {
                        if (response.Status is MuninnLobbyRoomStatus.Closed or MuninnLobbyRoomStatus.Unknown 
                            or MuninnLobbyRoomStatus.AllocatedFailed)
                        {
                            UIMessage.Show("房间已关闭");
                            Clear();
                        }
                        else
                        {
                            if (response.CustomProperties != null && 
                                response.CustomProperties.TryGetValue(CustomRoomPropertyKey.ROOM_STATE, out string state))
                            {
                                if (CustomRoomPropertyKey.GetCustomRoomState(state) == CustomRoomState.InBattle)
                                {
                                    UIMessage.Show("房间已开始对战，无法加入");
                                    RoomManager.LeaveRoom();
                                    Clear();
                                }
                                else
                                {
                                    //房间状态为未开始，可加入
                                    JoinCustomRoom();
                                }
                            }
                            else
                            {
                                UIManager.Instance.ShowPopUp("检测到邀请","是否加入房间？",ConfirmRouterToMainScene,Clear);
                            }
                        }
                    }));
                    break;
                default:
                    break;
            }
        }

        public void SetBattleMode(BattleMode mode)
        {
            battleMode = mode;
        }
        void ConfirmRouterToMainScene()
        {
            AysncLoadingScenenEf.OnUnloadLoadingCompletedAction -= OnRouterToMainScene;
            AysncLoadingScenenEf.OnUnloadLoadingCompletedAction += OnRouterToMainScene;
            GameRouter.LoadHomeScene();
        }
        
        void OnRouterToMainScene()
        {
            AysncLoadingScenenEf.OnUnloadLoadingCompletedAction -= OnRouterToMainScene;
            JoinCustomRoom();
        }

        void JoinCustomRoom()
        {
            //OneVSOneMatchPanel?.ModelChoosePanel.SetActive(false);
            if (string.IsNullOrEmpty(roomId))
            {
                Logger.Log("房间id为空");
                return;
            }
            var room = MuninnManager.Singleton.GetMuninnRoomView();
            if (MuninnManager.Singleton.InRoom)
            {
                if (room != null && room.Room.Id == roomId)
                {
                    UIMessage.Show("已加入该房间");
                    
                }
                else
                {
                    MuninnManager.Singleton.OnLeftRoomAction += LeftAndJoinRoom;
                    RoomManager.LeaveRoom();
                }
                return;
            }
            
            RegisterMuninnEvents(false);
            RegisterMuninnEvents(true);

            Logger.LogInfo(GenerateDeepLink(battleMode,fromPlayerName,roomId));
            RoomManager.JoinRoom(battleMode,roomId);
            UIManager.Instance.ShowCommonLoading($"正在加入{fromPlayerName}的房间");
        }


        public void ReJoinRoom(string roomid,BattleMode mode)
        {
            RegisterMuninnEvents(false);
            RegisterMuninnEvents(true);
            battleMode = mode;
            roomId = roomid;
            Logger.LogInfo(GenerateDeepLink(battleMode,fromPlayerName,roomId));
            RoomManager.JoinRoom(mode,roomId);
            UIManager.Instance.ShowCommonLoading($"正在加入房间");
        }
        private void LeftAndJoinRoom(LeaveRoomEvent obj)
        {
            MuninnManager.Singleton.OnLeftRoomAction -= LeftAndJoinRoom;
            JoinCustomRoom();
        }

        private void OnPlayerJoined(MuninnPlayer obj)
        {
            OneVSOneMatchPanel?.RefreshPanel();
        }

        private void OnLeftRoom(LeaveRoomEvent msg)
        {
            if (msg.reason == LeaveRoomReason.DisconnectByKick)
            {
                RoomManager.LeaveRoom();
                UIMessage.Show("被房主踢出房间");
                OneVSOneMatchPanel?.Hide();
            }
            UIManager.Instance.HideCommonLoading();
            RegisterMuninnEvents(false);
        }

        public void RegisterMuninnEvents(bool v)
        {
            if (v)
            {
                MuninnManager.Singleton.OnJoinRoomAction += OnJoinedRoom;
                MuninnManager.Singleton.OnJoinRoomFailedAction += OnJoinRoomFailed;
                MuninnManager.Singleton.OnPlayerLeftAction += OnPlayerLeft;
                MuninnManager.Singleton.OnDisconnectAction += OnDisconnect;
                MuninnManager.Singleton.OnLeftRoomAction += OnLeftRoom;
                MuninnManager.Singleton.OnPlayerJoinedAction += OnPlayerJoined;
            }
            else
            {
                MuninnManager.Singleton.OnJoinRoomAction -= OnJoinedRoom;
                MuninnManager.Singleton.OnJoinRoomFailedAction -= OnJoinRoomFailed;
                MuninnManager.Singleton.OnPlayerLeftAction -= OnPlayerLeft;
                MuninnManager.Singleton.OnDisconnectAction -= OnDisconnect;
                MuninnManager.Singleton.OnLeftRoomAction -= OnLeftRoom;
                MuninnManager.Singleton.OnPlayerJoinedAction -= OnPlayerJoined;
            }
        }

        private void OnDisconnect()
        {
            if(IsQuitting) return;
            if (!string.IsNullOrEmpty(roomId) && retryCount < 3)
            {
                Instance.StartCoroutine(RetryJoinRoom());
            }
            else
            {
                retryCount = 0;
                UIMessage.Show("网络连接断开",MessageType.Error);
                OneVSOneMatchPanel?.Hide();
                Clear();
                UIManager.Instance.HideCommonLoading();
            }
        }
        IEnumerator RetryJoinRoom()
        {
            UIMessage.Show("网络连接断开,正在重连...");
            UIManager.Instance.ShowCommonLoading("正在加入房间");
            Logger.LogInfo("RetryJoinRoom :" + roomId + " retryCount:" + retryCount);
            yield return new WaitForSeconds(retryCount + 1);
            JoinCustomRoom();
            retryCount++;
        }

        private void OnPlayerLeft(MuninnPlayer obj)
        {
            OneVSOneMatchPanel?.RefreshPanel();
        }

        private void OnJoinRoomFailed(MuninnError msg)
        {
            Logger.LogError($"OnJoinRoomFailed code:{msg.Code}, desc:{msg.Description}");
            if (msg.Code == (uint)MuninnCode.LobbyCreateRoomFailed)
            {
                UIMessage.Show("房间已失效",MessageType.Error);
            }
            MuninnManager.Singleton.OnJoinRoomFailedAction -= OnJoinRoomFailed;
            UIManager.Instance.HideCommonLoading();
        }

        public void OnJoinedRoom(MuninnRoomView roomView)
        {
            MuninnManager.Singleton.OnJoinRoomFailedAction -= OnJoinRoomFailed;
            MuninnManager.Singleton.OnJoinRoomAction -= OnJoinedRoom;
            UIManager.Instance.HideCommonLoading();
            if (roomView.Room.Properties != null && 
                roomView.Room.Properties.TryGetValue(CustomRoomPropertyKey.ROOM_STATE, out string state))
            {
                if (CustomRoomPropertyKey.GetCustomRoomState(state) == CustomRoomState.InBattle)
                {
                    UIMessage.Show("房间已失效",MessageType.Error);
                    RoomManager.LeaveRoom();
                    return;
                }
            }
            MuninnManager.Singleton.SetBattleMode(battleMode);
            
            OneVSOneMatchPanel?.RefreshPanel();
            MuninnMessage.Init();
        }
        public string GenerateDeepLink(BattleMode mode,string playerName, string currentRoomId)
        {
            var deepLink =
                $"battlemode={mode.ToString().ToLower()}&fromplayername={playerName}&roomid={currentRoomId}";
            return deepLink;
        }

        public void Clear()
        {
            AysncLoadingScenenEf.OnUnloadLoadingCompletedAction -= OnRouterToMainScene;
            launchParams?.Clear();
            gameInfo = string.Empty;
            battleMode = BattleMode.None;
            fromPlayer = string.Empty;
            fromPlayerName = string.Empty;
            fromPlayerTier = string.Empty;
            roomId = string.Empty;
        }

        public void Test()
        {
            if (!string.IsNullOrEmpty(TestDeepLink))
            {
                if (!TestDeepLink.StartsWith("battlemode="))
                {
                    TestDeepLink = GenerateDeepLink(BattleMode.OneOnOneCustom, "Test", TestDeepLink);
                }
                var p = TestDeepLink.Split("&");
                var dic = p.ToDictionary(x => x.Split("=")[0], x => x.Split("=")[1]);
                ParseDeepLinkToOpenParams(dic);
            }
        }
        
    }
    
#if UNITY_EDITOR

    [CustomEditor(typeof(LaunchParamsHelper))]
    public class LaunchParamsHelperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LaunchParamsHelper myScript = (LaunchParamsHelper)target;
            base.OnInspectorGUI();
            if (GUILayout.Button("测试"))
            {
                myScript.Test();
            }
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("手动断连"))
            {
                MuninnManager.Singleton.Disconnect();
            }

        }
    }
#endif
}