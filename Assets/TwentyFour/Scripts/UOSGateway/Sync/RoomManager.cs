using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Muninn;
using Unity.Muninn.Model;
using Unity.Muninn.MuninnLobby;
using Unity.UOS.TwentyFour.Scripts.Battle.Model;

namespace Unity.UOS.TwentyFour.UOSGateway
{
    public class RoomManager
    {
        public static bool LeaveRoomBySelf = false;
        public static void CreateRoom(BattleMode mode, 
            string roomName,
            string joinCode = "",
            bool updatePlayerInfo = true,
            Dictionary<string, string> customProperties = null
        )
        {
            if (updatePlayerInfo)
            {
                if (MuninnNetwork.PlayerInfo != null && MuninnNetwork.PlayerInfo.Properties != null)
                {
                    foreach (var property in PersonaPropertiesHelper.GetLocalProperties())
                    {
                        MuninnNetwork.PlayerInfo.Properties[property.Key] = property.Value;
                    }
                    
                }
            }

            var maxPlayers = 2;
            if(mode == BattleMode.TwoOnTwo)
                maxPlayers = 4;
            var req = new CreateRoomRequest()
            {
                Namespace = mode.ToString(),
                Name = roomName,
                MaxPlayers = maxPlayers,
                Visibility =
                    string.IsNullOrEmpty(joinCode) ? MuninnRoomVisibility.Public : MuninnRoomVisibility.Private,
                // JoinCode = joinCode,
                PlayerId = Identity.persona.PersonaID,
            };
            // 添加自定义属性
            if (customProperties != null)
            {
                req.CustomProperties = customProperties;
            }
            MuninnNetwork.CreateAndJoinRoom(req);
        }

        public static void JoinRoom(BattleMode mode, string roomUuid, string joinCode = "", bool updatePlayerInfo = true)
        {
            if (updatePlayerInfo)
            {
                if (MuninnNetwork.PlayerInfo != null && MuninnNetwork.PlayerInfo.Properties != null)
                {
                    foreach (var property in PersonaPropertiesHelper.GetLocalProperties())
                    {
                        MuninnNetwork.PlayerInfo.Properties[property.Key] = property.Value;
                    }
                }
            }
            var req = new CreateOrJoinRoomRequest()
            {
                Namespace = mode.ToString(),
                RoomUUID = roomUuid,
                JoinCode = joinCode,
                PlayerId = Identity.persona.PersonaID
            };
            MuninnNetwork.CreateOrJoinRoom(req);
        }
        
        public static void LeaveRoom()
        {
            LeaveRoomBySelf = true;
            MuninnNetwork.LeaveRoom();
        }
        
        public static void ListRooms(ListRoomRequest req ,Action<ListRoomResponse> callback = null)
        {
            MuninnNetwork.ListRooms(req, callback);
        }
        public static void UpdateRoomCustomProperties(Dictionary<string, string> customProperties,Action<MuninnUpdateRoomCustomPropertiesResponse> callback = null)
        {
            MuninnNetwork.UpdateRoomCustomProperties(customProperties, callback);
        }
    }
    
}