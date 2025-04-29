using System;
using Unity.UOS.TwentyFour.Common;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Text;
using Unity.Muninn.Model;
using Unity.Passport.Runtime.UI;
using Unity.UOS.TwentyFour.Model;
using UnityEngine;
using Unity.UOS.TwentyFour.Model.Sync;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

namespace Unity.UOS.TwentyFour.UOSGateway
{
    [Serializable]
    public class MuninnMessage
    {
        private static MuninnMessageData _battleData = new MuninnMessageData();
        public static MuninnMessageData BattleData => _battleData;
        
        // 服务器下发牌组数据
        public static UnityEvent<List<Stage>> OnStages = new UnityEvent<List<Stage>>();
        // 服务器下发结果判定
        public static UnityEvent<bool> OnJudgeResult = new UnityEvent<bool>();
        // 服务器下发团队进度
        public static UnityEvent<MuninnMessageData> OnProgress = new UnityEvent<MuninnMessageData>();
        // 服务器下发倒计时进度
        public static UnityEvent<int> OnCountDown = new UnityEvent<int>();
        // 客户端之间同步进度
        public static UnityEvent<int, string> OnSyncStatus = new UnityEvent<int, string>();
        
        public static UnityEvent<MuninnPlayer> OnCustomOnceMore = new UnityEvent<MuninnPlayer>();
        
        public static UnityEvent<MuninnMessageData> OnEndGame = new UnityEvent<MuninnMessageData>();

        /// <summary>
        /// 游戏是否已经结束
        /// </summary>
        /// <returns></returns>
        public static bool GameOver()
        {
            // 游戏结束：时间结束或者所有题目已经完成
            return BattleData.remainTime <= 0 || BattleData.allResolved;
        }
        
        public static void Init()
        {
            //reset all
            OnStages.RemoveAllListeners();
            OnJudgeResult.RemoveAllListeners();
            OnProgress.RemoveAllListeners();
            OnCountDown.RemoveAllListeners();
            OnSyncStatus.RemoveAllListeners();
            OnCustomOnceMore.RemoveAllListeners();
            OnEndGame.RemoveAllListeners();
            _battleData = new MuninnMessageData();

            Logger.LogInfo("MuninnMessage Init");
            ReadyToReceiveBattleStages();
        }

        public static void Clear()
        {
            OnStages.RemoveAllListeners();
            OnJudgeResult.RemoveAllListeners();
            OnProgress.RemoveAllListeners();
            OnCountDown.RemoveAllListeners();
            OnSyncStatus.RemoveAllListeners();
            OnCustomOnceMore.RemoveAllListeners();
            OnEndGame.RemoveAllListeners();

        }

        public static void ReadyToReceiveBattleStages()
        {
            OnStages.AddListener(InGameManager.OnReceiveBattleStages);
        }
        
        private static void SetTeamData(MuninnMessageData serverMessage)
        {
            _battleData.blueTeamProgress = serverMessage.blueTeamProgress;
            _battleData.redTeamProgress = serverMessage.redTeamProgress;
            _battleData.resolvedStatus = serverMessage.resolvedStatus;
            //根据服务器消息标记队伍
            var pId = Identity.persona.PersonaID;
            MuninnManager.MyTeamTag = serverMessage.redTeamProgress.teamPlayers.Find(
                player => player.uniqueId.Equals(pId)) == null ? TeamTag.BLUE : TeamTag.RED;
        }

        /// <summary>
        /// 分发消息
        /// </summary>
        /// <param name="data"></param>
        public static void Distribute(byte[] data)
        {
            var str = Encoding.UTF8.GetString(data);
            Logger.Log(str);
            var serverMessage = JsonUtility.FromJson<MuninnMessageData>(str);

            if (serverMessage.type == MuninnMessageData.Type.Distribute.ToString())
            {
                SetTeamData(serverMessage);
                Debug.Log("Stage+"+serverMessage.stages.Count);
                OnStages.Invoke(serverMessage.stages);
                _battleData.stages = serverMessage.stages;
            }

            if (serverMessage.type == MuninnMessageData.Type.JudgeResult.ToString())
            {
                OnJudgeResult.Invoke(serverMessage.judgeResult);
                _battleData.judgeResult = serverMessage.judgeResult;
            }

            if (serverMessage.type == MuninnMessageData.Type.SyncProgress.ToString())
            {
                Debug.Log("收到服务器进度同步");
                SetTeamData(serverMessage);
                _battleData.battleMode = serverMessage.battleMode;
                OnProgress.Invoke(serverMessage);
                _battleData.allResolved = serverMessage.allResolved;
            }

            if (serverMessage.type == MuninnMessageData.Type.CountDown.ToString())
            {
                // Debug.Log("收到服务器倒计时");
                OnCountDown.Invoke(serverMessage.remainTime);
                _battleData.remainTime = serverMessage.remainTime;
                _battleData.battleMode = serverMessage.battleMode;

            }

            if (serverMessage.type == MuninnMessageData.Type.SyncStatus.ToString())
            {
                OnSyncStatus.Invoke(serverMessage.currentIndex, serverMessage.personaID);
            }
            
            // 同步所有数据
            if (serverMessage.type == MuninnMessageData.Type.AllBattleData.ToString())
            {
                // Debug.Log("收到所有对局信息");
                SetTeamData(serverMessage);
                OnStages.Invoke(serverMessage.stages);
                _battleData.stages = serverMessage.stages;
                _battleData.battleMode = serverMessage.battleMode;

                OnProgress.Invoke(serverMessage);
                _battleData.allResolved = serverMessage.allResolved;

                OnCountDown.Invoke(serverMessage.remainTime);
                _battleData.remainTime = serverMessage.remainTime;
                UIMessage.Show("重连成功");
            }

            if (serverMessage.type == MuninnMessageData.Type.CustomOnceMoreResponse.ToString())
            {
                Logger.Log($"收到再来一局的响应 {serverMessage.senderId}");
                MuninnPlayer p = MuninnManager.GetRoom().Players.
                    Find(player => player.SenderId.ToString() == serverMessage.senderId);
                Logger.Log(p.Name);
                OnCustomOnceMore?.Invoke(p);

            }

            if (serverMessage.type == MuninnMessageData.Type.EndGame.ToString())
            {
                Logger.Log($"收到结束游戏的响应");
                _battleData.battleMode = serverMessage.battleMode;
                _battleData.redTeamProgress = serverMessage.redTeamProgress;
                _battleData.blueTeamProgress = serverMessage.blueTeamProgress;
                _battleData.resolvedStatus = serverMessage.resolvedStatus;
                OnEndGame?.Invoke(serverMessage);
            }
            
            
#if UNITY_EDITOR
            //Debug.Log("Only for the Anim test");
            //OnProgress.Invoke(serverMessage);
            //OnCountDown.Invoke(0);
#endif
        }
    }
}