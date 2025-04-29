using System.Collections;
using System.Collections.Generic;
using System;

namespace Unity.UOS.TwentyFour.Model.Sync
{
    public enum TeamTag
    {
        RED,
        BLUE
    }
    [Serializable]
    public class TeamPlayer
    {
        public string uniqueId;
        public string previousTier; // 之前的等级
        public int previousScore; // 之前的分数
        public string currentTier; // （比赛结束后）的等级
        public int currentScore; // （比赛结束后）的等级
        public bool isRobot;
        public string displayName; // 玩家名称
        public int battleResult; // 玩家的分数变化： 0 平局，-1 -2 失败，+1 +2 成功，-2 完败，+2 完胜
    }

    [Serializable]
    public class TeamProgress
    {
        public int score;
        public List<bool> resolved;
        public List<TeamPlayer> teamPlayers;
    }

    [Serializable]
    public class ResolvedStatus
    {
        public bool resolved; // 是否已经完成
        public string resolvedPersonaID; // 第一个完成的玩家的 PersonaID
        public TeamTag resolvedTeam; // 完成的玩家所在的队伍
        public double costTime; // 完成题目花费的时间（秒）
    }
}