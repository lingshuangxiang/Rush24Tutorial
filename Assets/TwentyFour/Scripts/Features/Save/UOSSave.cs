using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = TwentyFour.Scripts.Utilities.Logger;
using TwentyFour.Scripts.Features.Player;
using TwentyFour.Scripts.Gameplay.GameMode.StageMode;

namespace TwentyFour.Scripts.Features.Save
{
    public class UOSSave
    {
        
        public const string SAVE_NAME_STAGE_SCORES = "Stage Scores";
        
        public const string SAVE_NS_STAGE_SCORES = "StageScores";
        public const string SAVE_NS_BATTLE_RESULTS = "BattleResults";
        
        static List<int> ConvertStringToList(string str)
        {
            List<int> list = new List<int>();
        
            foreach (char c in str)
            {
                // 将字符转换为整数并添加到列表中
                int number = int.Parse(c.ToString());
                list.Add(number);
            }
        
            return list;
        }
        
        public static void Init()
        {
            // CloudSaveSDK.Instance 包含了用户的 userId 信息
            // ListAllAsync 会根据 options 列出该用户的存档，按时间倒序排列，最新的存档在前
            var savedScores = PlayerPrefs.GetString(SAVE_NS_STAGE_SCORES);
            if (!String.IsNullOrEmpty(savedScores))
            {
                FetchPlayerProgress();
            }
            else
            {
                Logger.Log("not found existed score save");
                StageManager.LoadEmptyStageScore();
            }

        }

        private static void FetchPlayerProgress()
        {
            var scores = PlayerPrefs.GetString(SAVE_NS_STAGE_SCORES);
            StageManager.LoadStageScores(ConvertStringToList(scores));
        }
        
        
        public static void SavePlayerProgress(List<int> scores)
        {
            PlayerPrefs.SetString(SAVE_NS_STAGE_SCORES, string.Join("", scores));
        }

        public static void Dispose()
        {
        }
    }
}