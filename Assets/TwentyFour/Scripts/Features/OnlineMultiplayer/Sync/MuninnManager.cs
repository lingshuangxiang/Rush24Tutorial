using System;
using System.Collections;
using Unity.Muninn;
using Unity.Muninn.Model;
using Unity.Muninn.MuninnLobby;
using Unity.UOS.TwentyFour.UOSGateway;
using Unity.Muninn.Transport;
using System.Collections.Generic;
using System.Text;
using Passport;
using Unity.UOS.TwentyFour.Scripts.Battle.UI;
using Unity.UOS.Common;
using UnityEngine;
using Unity.UOS.TwentyFour.Model.Sync;
using Unity.UOS.TwentyFour.Robot;
using Unity.UOS.TwentyFour.Scripts.Battle.Model;
using UnityEngine.Events;
using Logger = Unity.UOS.TwentyFour.Common.Logger;
using TwentyFour.Scripts.RemoteConfig;

namespace Unity.UOS.TwentyFour.UOSGateway
{
    public partial class MuninnManager: MuninnBehaviour
    {
        private static bool applicationIsQuitting;
        private static bool isInitialized;
        public static MuninnManager Singleton
        {
            get
            {
                if (_singleton == null)
                {
                    if(applicationIsQuitting || !isInitialized)
                        return null;
                    // find the generic instance
                    _singleton = FindObjectOfType<MuninnManager>();

                    // if it's null again create a new object
                    // and attach the generic instance
                    if (_singleton == null)
                    {
                        var obj = new GameObject
                        {
                            name = nameof(MuninnManager)
                        };
                        _singleton = obj.AddComponent<MuninnManager>();
                        DontDestroyOnLoad(obj);
                    }
                }

                return _singleton;
            }
        }
        private static MuninnManager _singleton;
        public const int StageSize = 5;
        public static TeamTag MyTeamTag;
        public List<MuninnPlayer> Players;
        private const int MaxPlayerCount = 2;
        
        private static BattleMode currentBattleMode = BattleMode.None;

        [SerializeField] public string RoomId;

        public bool InRoom;

        //for Metrics
        public static bool IsBotRoom;

        
        public static void Initialize()
        {
            isInitialized = true;
            Singleton.Init();
        }
        void Init()
        {
            isInitialized = true;
            Application.runInBackground = true;
            // 请在加入房间前执行
            // 配置UOS App信息
            MuninnSettings.UosAppId = Settings.AppID;
            MuninnSettings.UosAppSecret = Settings.AppSecret;
 
            //MuninnSettings.RoomProfileUUID = "84094f17-64bb-4caf-9d45-6efd7620f37c";
            UosAppConfigs getConfig = Resources.Load<UosAppConfigs>("UosConfigs");
            MuninnSettings.RoomProfileUUID = getConfig.RoomProfileUUID;
 
            // 支持WebSocket/WebSocketSecure/KCP/UTP, 默认为UTP
            MuninnSettings.TransportType = MuninnTransportType.KCP;

            // 开启开发模式, 默认不开启。
            // 开启后屏蔽客户端&服务端的心跳检测, 此时建议Transport选择WebSocket/WebSocketSecure
            // MuninnSettings.Development = true;
 
            // 设置本地玩家的信息
            MuninnNetwork.PlayerInfo = new MuninnPlayerInfo()
            {
                Id = Identity.persona.PersonaID,
                Name = Identity.persona.DisplayName,
                Properties = PersonaPropertiesHelper.GetLocalProperties(),
            };

            MuninnLobbyService.DefaultApiTimeout = 15;
            Players = new List<MuninnPlayer>();
            InRoom = false;
            Logger.Log("完成初始化配置");
        }

        //保证整个游戏生命周期只有一个实例
        public void Dispose()
        {
            retryRoomId = string.Empty;
            tryJoinMode = BattleMode.None;
            retryCount = 0;
            isInitialized = false;
            Players?.Clear();
            OnLeftRoomAction = null;
            OnJoinRoomAction = null;
            OnPlayerJoinedAction = null;
            OnPlayerLeftAction = null;
            OnJoinRoomFailedAction = null;
            OnDisconnectAction = null;
            OnMasterClientChangedAction = null;
            currentBattleMode = BattleMode.None;
        }

        public override void OnDestroy()
        {
            applicationIsQuitting = true;
            base.OnDestroy();
        }

        public override void OnApplicationQuit()
        {
            applicationIsQuitting = true;
            base.OnApplicationQuit();
        }

        

        /// <summary>
        /// 设置玩家队伍信息
        /// </summary>
        /// <param name="teamTag"></param>
        public static void SetPlayerTeam(TeamTag teamTag)
        {
            MuninnNetwork.PlayerInfo = new MuninnPlayerInfo()
            {
                Id = Identity.persona.PersonaID,
                Name = Identity.persona.DisplayName,
                Properties = new Dictionary<string, string>
                {
                    {"teamTag", teamTag.ToString()},
                },
            };
        }
        public void SetBattleMode(int battleMode)
        {
            SetBattleMode((BattleMode)battleMode);
        }
        public void SetBattleMode(BattleMode battleMode)
        {
            currentBattleMode = battleMode;
            Logger.Log($"SetBattleMode: {battleMode}");
        }

        public BattleMode GetBattleMode()
        {
            return currentBattleMode;
        }
        
        public static void FetchRoomInfo(string roomId,Action<QueryRoomResponse> callback = null)
        {
            Singleton.StartCoroutine(MuninnLobbyService.AsyncQueryRoom(roomId, callback));
        }

        static BattleMode tryJoinMode = BattleMode.None;
        public static string retryRoomId = string.Empty;
        const int MaxTryJoinCount = 3;
        static int retryCount = 0;
        public static void CreateRoom(BattleMode mode, string roomName, string joinCode = "",
            bool updatePlayerInfo = true)
        {
            RoomManager.CreateRoom(mode, roomName, joinCode, updatePlayerInfo);
            tryJoinMode = mode;
        }
        public static void JoinRoom(BattleMode mode, string uuid, string joinCode = "",
            bool updatePlayerInfo = true)
        {
            RoomManager.JoinRoom(mode, uuid, joinCode, updatePlayerInfo);
            tryJoinMode = mode;
        }

        public static void DelayJoinRoom(float delay, BattleMode mode, string uuid, string joinCode = "",
            bool updatePlayerInfo = true)
        {
            Logger.LogInfo($"DelayJoinRoom: {delay} {mode} {uuid} {joinCode} {updatePlayerInfo}");
            Singleton.StopCoroutine("_DelayJoinRoom");
            Singleton.StartCoroutine(Singleton._DelayJoinRoom(delay,mode, uuid, joinCode, updatePlayerInfo));
        }
        IEnumerator _DelayJoinRoom(float delay,BattleMode mode, string uuid, string joinCode = "",
            bool updatePlayerInfo = true)
        {
            yield return new WaitForSeconds(delay);
            JoinRoom(mode, uuid, joinCode, updatePlayerInfo);
        }
        
        private void CheckStartGame()
        {
            Logger.LogInfo("CheckStartGame");
            var roomView = GetMuninnRoomView();
            var isMasterClient = roomView.IsMasterClient();
            
            // // 设置队伍信息
            // MyTeamTag = isMasterClient ? TeamTag.RED : TeamTag.BLUE;
            
            // 先用 master 模拟
            if (isMasterClient)
            {   
                Logger.Log("isMaster");
                
                if (roomView.Players.Count == MaxPlayerCount && (currentBattleMode == BattleMode.OneOnOne || currentBattleMode == BattleMode.TournamentOneOnOne))
                {
                    Logger.LogInfo("人数已满，房主开始游戏");
                    StartGame();
                } else if (IsRobotRoom())
                {
                    Logger.LogInfo("机器人房间，开始游戏");
                    StartGame();
                }
            }
        }

        public void StartGame()
        {
            //SetBattleMode(BattleMode.None);
            var roomView = GetMuninnRoomView();
            MuninnNetwork.RaiseEvent(
                Encoding.UTF8.GetBytes(StartGameData(roomView.Players)),
                new RaiseEventOptions() {Target = RaiseEventTarget.TO_PLUGIN}
            );
            Dictionary<string, string> properties = MuninnManager.GetRoom().Room.Properties;;
            properties[CustomRoomPropertyKey.ROOM_STATE] = CustomRoomState.InBattle.ToString();
            RoomManager.UpdateRoomCustomProperties(properties, callback =>
            {
                Logger.Log("UpdateRoomCustomProperties");
            });
        }

        public void CustomOnceMoreRequest()
        {
            var serverMessage = new MuninnMessageData()
            {
                type = MuninnMessageData.Type.CustomOnceMoreRequest.ToString(),
            };
            Logger.Log($"发给服务器的消息 : {JsonUtility.ToJson(serverMessage)}");
            var data =  JsonUtility.ToJson(serverMessage);
            MuninnNetwork.RaiseEvent(
                Encoding.UTF8.GetBytes(data),
                new RaiseEventOptions() {Target = RaiseEventTarget.TO_PLUGIN}
            );
        }

      

        /// <summary>
        /// 客户端之间同步进度
        /// </summary>
        /// <param name="index"></param>
        public static void SyncStatus(int index)
        {
            Logger.Log(" SyncOthersWorkProgress: " + index);
            var msg = new MuninnMessageData()
            {
                type = MuninnMessageData.Type.SyncStatus.ToString(),
                currentIndex = index,
                personaID = Identity.persona.PersonaID
            };
            MuninnNetwork.RaiseEvent(
                Encoding.UTF8.GetBytes(JsonUtility.ToJson(msg)),
                new RaiseEventOptions() { Target = RaiseEventTarget.TO_PLUGIN }
            );
        }
        
        /// <summary>
        /// 消息：服务器开始游戏
        /// </summary>
        /// <returns></returns>
        private static string StartGameData(List<MuninnPlayer> players)
        {
            // master 作为红队，对方作为蓝队
            var redTeamPlayers = new List<TeamPlayer>();
            var blueTeamPlayers = new List<TeamPlayer>();
            
            foreach (var player in players)
            {
                var p = new TeamPlayer()
                {
                    uniqueId = player.Id,
                    displayName = player.Name
                };
                if (player.Id == Identity.persona.PersonaID)
                {
                    // 自己队伍
                    redTeamPlayers.Add(p);
                }
                else
                {
                    blueTeamPlayers.Add(p);
                }
            }

            IsBotRoom = false;
            // 添加蓝队机器人信息（此时房间已创建完成，机器人信息已生成）
            if (IsRobotRoom())
            {
                IsBotRoom = true;
                var robotPlayer = RobotHelper.Player;
                blueTeamPlayers.Add(new TeamPlayer()
                {
                    displayName = robotPlayer.Name,
                    uniqueId = "2025" + robotPlayer.Id,
                    isRobot = true
                });
            }
            
            Debug.Log("blueTeamPlayers "+ JsonUtility.ToJson(blueTeamPlayers));

            var useAdvanceQuestionsRate = RemoteConfigHelper.GetInt(RemoteConfigKeys.UseAdvanceQuestionsRate);
            var serverMessage = new MuninnMessageData()
            {
                type = MuninnMessageData.Type.StartGame.ToString(),
                // 上传初始分队信息
                redTeamProgress = new TeamProgress()
                {
                    teamPlayers = redTeamPlayers,
                    resolved = new List<bool>(new bool[StageSize]),
                },
                blueTeamProgress = new TeamProgress()
                {
                    teamPlayers = blueTeamPlayers,
                    resolved = new List<bool>(new bool[StageSize]),
                },
                resolvedStatus = new List<ResolvedStatus>(new ResolvedStatus[StageSize]),
                //customTime = 25,
                useAdvanceQuestionsRate = useAdvanceQuestionsRate,
                battleMode = currentBattleMode
            };
            Logger.Log($"发给服务器的消息 : {JsonUtility.ToJson(serverMessage)}");
            return JsonUtility.ToJson(serverMessage);
        }
        
        public static MuninnRoomView GetRoom()
        {
            return _singleton.GetMuninnRoomView();
            
        }

        /// <summary>
        /// 是否为机器人房间
        /// </summary>
        /// <returns></returns>
        public static bool IsRobotRoom()
        {
            var room = GetRoom();
            return RobotHelper.CheckIsRobot(room.Room.Properties);
        }

        
        //if the input player is masterclient
        public bool IsMasterClient(MuninnPlayer player)
        {
            if (player == null)
            {
                return false;
            }
            return GetRoom()?.MasterClientId == player.SenderId;
        }
    }
}