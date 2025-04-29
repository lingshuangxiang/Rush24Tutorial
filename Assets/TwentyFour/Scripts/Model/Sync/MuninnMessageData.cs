using System;
using System.Collections.Generic;
using Unity.UOS.TwentyFour.UOSGateway;
using Unity.UOS.TwentyFour.Scripts.Battle.Model;

namespace Unity.UOS.TwentyFour.Model.Sync
{
    [Serializable]
    public class MuninnMessageData
    {
        public enum Type
        {
            StartGame,
            Distribute,
            SubmitAnswer,
            JudgeResult,
            SyncProgress,
            CountDown,
            SyncStatus, // 客户端之间同步状态
            AllBattleData, // 对局信息（用于断线重连）
            CustomOnceMoreRequest, // 自定义模式，玩家请求再来一局
            CustomOnceMoreResponse, // 自定义模式，玩家请求再来一局的响应
            EndGame,
        }
        // 消息类型
        public string type;
        
        // type: StartGame
        public int size = MuninnManager.StageSize;
        public int customTime = -1;
        public int useAdvanceQuestionsRate; // 使用进阶题库的概率
        public BattleMode battleMode; // 对局模式
        public string tournamentSlugName; // 锦标赛唯一标识
        public string tournamentConfig; // 锦标赛其他配置

        // type: Distribute
        public List<Stage> stages;
        
        // type: SubmitAnswer
        public List<Expression> answerExpressions;
        public int answerIndex;
        public string personaID;
        
        // type: JudgeResult
        public bool judgeResult;
        
        // type: SyncProgress
        public TeamProgress redTeamProgress;
        public TeamProgress blueTeamProgress;
        public List<ResolvedStatus> resolvedStatus; // 抢答模式，题目完成状态
        public TeamTag currentResolvedTeam; // 做出当前题目的队伍
        public string currentResolvedPersonaID; // 做出当前题目的玩家ID
        public int currentResolvedQuestionIndex; // 当前题目序号
        public bool allResolved; // 是否所有题目已经完成

        // type: CountDown
        public int remainTime; // 单位：秒
        
        // type: SyncStatus
        public int currentIndex;
        
        // type: CustomOnceMoreResponse
        public string senderId;
        
    }
}