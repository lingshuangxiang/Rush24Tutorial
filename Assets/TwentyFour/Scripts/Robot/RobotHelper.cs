using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Passport.Runtime;
using Unity.UOS.TwentyFour.UOSGateway;
using Passport;
using Unity.Muninn.Model;
using TwentyFour.Scripts.RemoteConfig;
using Random = UnityEngine.Random;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

namespace Unity.UOS.TwentyFour.Robot
{
    [Serializable]
    public class RobotServerConfig
    {
        // 机器人服务端参数
        public int MinRandomTime = -3;
        public int MaxRandomTime = 20;
        public int LowerTimeBound = 5; // 最少的完成一道题目的时间
        public int UpperTimeBound = 50; // 最长的完成（或放弃）一道题目的时间
        public int ResolutionRateBound = 50; // 解决率低于 ResolutionRateBound 的题目放弃
        public string TimeFormula;
        public string ResolutionRateFormula;
        public List<AbilityItem> Ability;
        public double AbilityCoefficient;
        public int UseRandomOrderToAnserQuestionRate = 100; // 机器人使用随机顺序答题的概率
    }

    [Serializable]
    public class RobotClientConfig
    {
        // 机器人客户端参数
        public int MinTimeToCreateRobot = 10;
        public int MaxTimeToCreateRobot = 20;
    }

    [Serializable]
    public class AbilityItem
    {
        public double From;
        public double To;
        public double Coefficient;
    }
    
    public class RobotHelper
    {
        private const string Flag = "ISROBOT";
        // 机器人参数配置
        private const string ConfigFlag = "ROBOTCONFIG";
        private static Persona _robot;
        public static Persona Robot => _robot;
        private static MuninnPlayer _player;
        public static MuninnPlayer Player => _player;
        
        // 随机创建机器人的时间
        private static float _randomCreateRobotTime = 0;
        private static bool _shouldCreateRobot = false;

        private static async Task<double> GetPlayerAbilityCoefficient()
        {
            Leaderboard.GetMemberScoreResponse myScoreResponse =
                await TiersHelper.GetMyLeaderboardScore(TiersHelper.TiersLeaderboardSlugName);
            var score = 0;
            if (myScoreResponse.Scores.Any() && myScoreResponse.Scores.ToList()[0].Score > 0)
            {
                score = (int)myScoreResponse.Scores.ToList()[0].Score;
            }
            var coefficient = GetCoefficientByScore(score);
            Logger.Log($"玩家当前段位分数: {score} 玩家当前能力系数: {coefficient}");
            return coefficient;
        }

        private static double GetCoefficientByScore(int score)
        {
            var config = GetRobotServeConfig();
            var needDowngrade = DowngradeHelper.NeedDowngrade();

            if (needDowngrade)
            {
                for (var i = 0; i < config.Ability.Count; i += 1)
                {
                    var item = config.Ability[i];
                    if ((score >= item.From || item.From == 0) && (score < item.To || item.To == 0))
                    {
                        var downgradeIndex = i + 1;
                        if (downgradeIndex >= config.Ability.Count) downgradeIndex -= 1;
                        Logger.Log($"【降级】降级前的系数：{config.Ability[i].Coefficient}");
                        Logger.Log($"【降级】降级后的系数: {config.Ability[downgradeIndex].Coefficient}");
                        return config.Ability[downgradeIndex].Coefficient;
                    }
                }
            }
            

            foreach (var item in config.Ability)
            {
                if ((score >= item.From || item.From == 0) && (score < item.To || item.To == 0))
                {
                    return item.Coefficient;
                }
            }

            return 0.5;
        }

        private static RobotServerConfig GetRobotServeConfig()
        {
            return RemoteConfigHelper.GetJson<RobotServerConfig>(RemoteConfigKeys.RobotServerConfig);
        }
        
        public static void GenRandomCreateRobotTime()
        {
            var clientConfig = RemoteConfigHelper.GetJson<RobotClientConfig>(RemoteConfigKeys.RobotClientConfig);
            _shouldCreateRobot = false; // 重置变量
            _randomCreateRobotTime = Random.Range(clientConfig.MinTimeToCreateRobot, clientConfig.MaxTimeToCreateRobot);
            Debug.Log("本轮机器人诞生时间: " + _randomCreateRobotTime);
        }

        public static bool ShouldCreateRobot()
        {
            if (_shouldCreateRobot) return true;
            // 防止查询时 SimpleMatchTimer.CurrentTime 变量已被重置
            _shouldCreateRobot = SimpleMatchTimer.CurrentTime > _randomCreateRobotTime;
            return _shouldCreateRobot;
        }
        
        public static async Task AddRobotProperties(Dictionary<string, string> customProperties)
        {
            customProperties.Add(Flag, "true");
            var serverConfig = GetRobotServeConfig();
            
            // 添加能力系数
            var coefficient = await GetPlayerAbilityCoefficient();
            serverConfig.AbilityCoefficient = coefficient;
            
            var config = JsonUtility.ToJson(serverConfig);
            customProperties.Add(ConfigFlag, config);
            
            // 获取随机身份信息
            var realmID = await Identity.GetRealmID();
            var friendList = await PassportFeatureSDK.Friends.FindFriendsRandomly(realmID, 30);

            var filtered = friendList.Personas.ToList().Where(u => !string.IsNullOrEmpty(u.DisplayName)).ToList();
            if (filtered.Any())
            {
                var randomIndex = Random.Range(0, filtered.Count);
                var persona = filtered[randomIndex];
                _robot = persona;
                _player = new MuninnPlayer()
                {
                    Id = persona.PersonaID,
                    Name = persona.DisplayName,
                    Properties = persona.Properties.ToDictionary(x => x.Key, x => x.Value)
                };
            }
        }

        public static bool CheckIsRobot(Dictionary<string, string> customProperties)
        {
            var isRobot = "false";
            customProperties.TryGetValue(Flag, out isRobot);
            return isRobot == "true";
        }
    }
}