using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace TwentyFour.Scripts.Accomplishment
{


// 最外层响应结构
    [Serializable]
    public class AccomplishmentData
    {
        public List<BattleData> data;
        public Pagination pagination;
    }

// 分页信息
    [Serializable]
    public class Pagination
    {
        public int totalPages;
        public int currentPage;
        public int total;
        public int pageSize;
    }

// 单场对战数据
    [Serializable]
    public class BattleData
    {
        public int battleMode;
        public TeamProgress blueTeamProgress;
        public TeamProgress redTeamProgress;
        public List<Question> questions;
        public List<ResolvedHistory> resolvedHistory;
        public string matchId;
        public string roomId;
        public string startTime;
        public string endTime;
        public string playerTeam;
    }

// 队伍进度
    [Serializable]
    public class TeamProgress
    {
        public int score;
        public List<TeamPlayer> teamPlayers;
        public List<bool> resolved;
    }

// 玩家信息
    [Serializable]
    public class TeamPlayer
    {
        [JsonProperty("uniqueId")]
        public long uniqueId;
        public string displayName;
        public int currentIndex;
        public bool isRobot;
        public int currentScore;
        public string previousTier;
        public int previousScore;
        public string currentTier;
    }

// 题目信息
    [Serializable]
    public class Question
    {
        public int index;
        public QuestionData question;
    }

    [Serializable]
    public class QuestionData
    {
        public List<Card> cards;
    }

// 卡牌数据
    [Serializable]
    public class Card
    {
        public int index;
        public int number;
        public int suit;
    }

// 解题历史
    [Serializable]
    public class ResolvedHistory
    {
        public int resolveTime;
        public int questionIndex;
        public string resolvePersonaId;
        public bool resolved;
    }
}