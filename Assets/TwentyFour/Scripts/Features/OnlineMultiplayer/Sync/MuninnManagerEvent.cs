using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Muninn;
using Unity.Muninn.Model;
using Unity.Muninn.MuninnLobby;
using Unity.UOS.TwentyFour.Scripts.Battle.Model;
using Unity.UOS.TwentyFour.Scripts.Battle.UI;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

namespace Unity.UOS.TwentyFour.UOSGateway
{
    public partial class MuninnManager : MuninnBehaviour
    {
        
        public Action<JoinRoomResponse> BeforeConnectRoomAction;
        public Action<LeaveRoomEvent> OnLeftRoomAction;
        public Action<MuninnRoomView> OnJoinRoomAction;
        public Action<MuninnPlayer> OnPlayerJoinedAction;
        public Action<MuninnPlayer> OnPlayerLeftAction;
        public Action<MuninnError> OnJoinRoomFailedAction;
        public Action<uint> OnMasterClientChangedAction;
        public Action OnDisconnectAction;
        public Action<Dictionary<string, string>> OnRoomCustomPropertiesUpdatedAction;
        public override void BeforeConnectRoom(JoinRoomResponse resp)
        {
            Logger.LogInfo($"BeforeConnectRoom: {resp.RoomUuid}");
            base.BeforeConnectRoom(resp);
            retryRoomId = resp.RoomUuid;
            BeforeConnectRoomAction?.Invoke(resp);
            
        }

        public override void OnJoinedRoom(MuninnRoomView roomView)
        {
           
            RoomId = roomView.Room.Id;
            // 1. 加入房间后需要处理的信息
            // 房间信息
            MuninnRoom curRoom = roomView.Room;
            // 玩家自己的信息
            MuninnPlayer owner = roomView.Self();
            // 房间的在线用户列表（包含自己）
            List<MuninnPlayer> players = roomView.Players;
            // 加入房间时，附带的CachedEvent列表，帮助游戏快速恢复快照等等
            List<MuninnCachedEvent> cachedEvents = roomView.CachedEvents;
 
            // 2. 其他信息
            // 房间给该玩家分配的Id (保证唯一)
            uint senderId = roomView.SenderId;
            // MasterClient对应的Id
            uint masterClientId = roomView.MasterClientId;
            // 快速判断自己是不是MasterClient
            roomView.IsMasterClient();
            
            Logger.LogInfo($"房间属性信息 ： {JsonUtility.ToJson(roomView.Room.Properties)}");
            
            //todo 根据NameSpace刷新本地CurrentBattleMode
            Logger.Log(curRoom.Namespace);

            OnJoinRoomAction?.Invoke(roomView);
            CheckStartGame();
            Logger.Log($"自己加入房间{owner.Name}");
            InRoom = true;
            // TomMatchMaking findMatchObjet=FindAnyObjectByType(typeof (TomMatchMaking)) as TomMatchMaking;
            // if(findMatchObjet!=null)
            //     findMatchObjet.currentRoom=roomView;    
            retryRoomId = string.Empty;
            tryJoinMode = BattleMode.None;
            retryCount = 0;
        }
        
        public override void OnPlayerEnteredRoom(MuninnPlayer player)
        {
            Logger.LogInfo($"[Muninn] Player {player.Id} entered room，玩家加入房间 {player.Name}，");
            OnPlayerJoinedAction?.Invoke(player);
            CheckStartGame();
            InGamePlayersUI.SetPlayers(GetMuninnRoomView().Players);
        }


        public override void OnJoinRoomFailed(MuninnError error)
        {
            Logger.Log($"JoinRoomFailed: {error.Description}, Code: {error.Code}, roomId: {error.RoomUuid}");
            if (!string.IsNullOrEmpty(retryRoomId) && retryCount < MaxTryJoinCount)
            {
                UIManager.Instance.StartCoroutine(RetryJoinRoom());
            }
            else
            {
                retryCount = 0;
                retryRoomId = string.Empty;
                tryJoinMode = BattleMode.None;
                InRoom = false;
                SetBattleMode(BattleMode.None);
                OnJoinRoomFailedAction?.Invoke(error);
                UIManager.Instance.HideCommonLoading();
            }
        }

        public override void OnRoomCustomPropertiesUpdated(Dictionary<string, string> customProperties)
        {
            base.OnRoomCustomPropertiesUpdated(customProperties);
            OnRoomCustomPropertiesUpdatedAction?.Invoke(customProperties);
            Logger.LogInfo($"OnRoomCustomPropertiesUpdated: {JsonUtility.ToJson(customProperties)}");
        }

        public override void OnMasterClientChanged(uint masterClientId)
        {
            base.OnMasterClientChanged(masterClientId);
            OnMasterClientChangedAction?.Invoke(masterClientId);
        }
        
        public override void OnLeftRoom(LeaveRoomEvent e)
        {
            InRoom = false;
            RoomId = GetMuninnRoomView()?.Room?.Id;
            SetBattleMode(BattleMode.None);
            MuninnNetwork.PlayerInfo = new MuninnPlayerInfo()
            {
                Id = Identity.persona.PersonaID,
                Name = Identity.persona.DisplayName,
                Properties = PersonaPropertiesHelper.GetLocalProperties(),
            };
            LeaveRoomReason reason = e.reason;
            Logger.LogInfo($"[Muninn]: OnLeftRoom() was called by Muninn. Reason : {reason}");
            switch (reason)
            {
                case LeaveRoomReason.DisconnectByClient:
                    break;
                case LeaveRoomReason.DisconnectByKick:
                    break;
                case LeaveRoomReason.DisconnectByTimeout:
                    break;
                case LeaveRoomReason.DisconnectByCloseRoom:
                    break;
            }
            OnLeftRoomAction?.Invoke(e);

            RoomManager.LeaveRoomBySelf = false;
        }
        public override void OnPlayerLeftRoom(MuninnPlayer player)
        {
            OnPlayerLeftAction?.Invoke(player);
            Logger.LogInfo($"[Muninn] Player {player.Id} left room");
        }
        public override void OnDisconnected()
        {
            InRoom = false;
            retryCount = 0;
            retryRoomId = string.Empty;
            tryJoinMode = BattleMode.None;
            SetBattleMode(BattleMode.None);
            base.OnDisconnected();
            OnDisconnectAction?.Invoke();
        }

        public override void OnEvent(MuninnEvent e)
        {
            // Debug.Log("触发 OnEvent");
            MuninnMessage.Distribute(e.Data);
        }
        
        // 收到Server Call请求
        public override void OnServerCall(MuninnEvent e)
        {
            // 只有 server 会收到
            // BattleManager.StartBattle(e.Data);
        }
        
        IEnumerator RetryJoinRoom()
        {
            UIManager.Instance.ShowCommonLoading("正在重试加入房间");
            Logger.LogInfo("RetryJoinRoom :" + retryRoomId + " retryCount:" + retryCount);
            yield return new WaitForSeconds(retryCount + 1);
            RoomManager.JoinRoom(tryJoinMode,retryRoomId);
            retryCount++;
        }

    }
}