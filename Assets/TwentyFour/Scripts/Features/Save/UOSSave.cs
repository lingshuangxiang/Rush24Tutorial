using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Logger = TwentyFour.Scripts.Utilities.Logger;
using TwentyFour.Scripts.Features.Player;

namespace Unity.UOS.TwentyFour.UOSGateway
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
            Logger.Log("执行 UOS Save Init");
            // 使用 UOS Launcher 方式初始化SDK, 更多SDK初始化方式见 sdk package sample目录
   
            // 默认与 UOS Launcher 中填写的 UOS APP 关联
            // 如需与其他 UOS APP 关联，可以使用 CloudSaveSDK.Initialize(string appId, string appSecret, string userId) 方法
            Logger.Log("初始化 Cloud Save");
            Logger.Log("获取 Persona ID");
            string personaId = Identity.persona.PersonaID;
            Logger.Log($"Persona ID: {personaId}");
            
            // list saves
            
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