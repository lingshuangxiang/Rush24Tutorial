using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Unity.UOS.CloudSave;
using Unity.UOS.CloudSave.Exception;
using Unity.UOS.CloudSave.Model.Files;
using Unity.VisualScripting;
using UnityEngine;
using Logger = Unity.UOS.TwentyFour.Common.Logger;
using SaveOptions = Unity.UOS.CloudSave.Model.Files.ListOptions;

namespace Unity.UOS.TwentyFour.UOSGateway
{
    public class UOSSave
    {
        
        public const string SAVE_NAME_STAGE_SCORES = "Stage Scores";
        
        public const string SAVE_NS_STAGE_SCORES = "StageScores";
        public const string SAVE_NS_BATTLE_RESULTS = "BattleResults";
        
        private static string stageScoresSaveId;
        
        
        
        public static async Task Init()
        {
            Logger.Log("执行 UOS Save Init");
            // 使用 UOS Launcher 方式初始化SDK, 更多SDK初始化方式见 sdk package sample目录
            try
            {
                // 默认与 UOS Launcher 中填写的 UOS APP 关联
                // 如需与其他 UOS APP 关联，可以使用 CloudSaveSDK.Initialize(string appId, string appSecret, string userId) 方法
                Logger.Log("初始化 Cloud Save");
                await CloudSaveSDK.Initialize(true);
                Logger.Log("获取 Persona ID");
                string personaId = Identity.persona.PersonaID;
                Logger.Log($"Persona ID: {personaId}");
                
                // list saves
                ListOptions options = new ListOptions()
                {
                    PersonaId = personaId,
                    Namespaces = new List<string>(){ SAVE_NS_STAGE_SCORES },
                }; // list选项， 非必填项
                
                // CloudSaveSDK.Instance 包含了用户的 userId 信息
                // ListAllAsync 会根据 options 列出该用户的存档，按时间倒序排列，最新的存档在前
                List<SaveItem> saveItems = await CloudSaveSDK.Instance.Files.ListAllAsync(options);
                if (saveItems.Count > 0)
                {
                    stageScoresSaveId = saveItems[0].SaveId;
                    await FetchPlayerProgress();
                }
                else
                {
                    Logger.Log("not found existed score save");
                    StageManager.LoadEmptyStageScore();
                }
            }
            catch (CloudSaveClientException e)
            {
                Logger.LogError($"failed to initialize SAVE sdk, clientEx: {e.Message}");
            }
            catch (CloudSaveServerException e)
            {
                Logger.LogError($"failed to initialize SAVE sdk, serverEx: {e.Message}");
            }
        }

        private static async Task FetchPlayerProgress()
        {
            try
            {
                byte[] bytes =  await CloudSaveSDK.Instance.Files.LoadBytesAsync(stageScoresSaveId);
                StageManager.LoadStageScores(bytes);
            }
            catch (CloudSaveClientException e)
            {
                Logger.LogError($"failed to load file, saveId {stageScoresSaveId}, clientEx: {e.Message}");
                throw;
            }
            catch (CloudSaveServerException e)
            {
                Logger.LogError($"failed to load file, saveId {stageScoresSaveId}, serverEx: {e.Message}");
                throw;
            }
        }
        
        
        public static async Task SavePlayerProgress(List<int> scores)
        {
            byte[] bytes = SerializeList.Serialize(scores);
            if (string.IsNullOrEmpty(stageScoresSaveId))
            {
                // 创建新存档
                string name = SAVE_NS_STAGE_SCORES; // 存档名称
                try
                {
                    CreateOptions  options = new CreateOptions ()
                    {
                        PersonaId = Identity.persona.PersonaID,
                        Namespace = SAVE_NS_STAGE_SCORES
                    };
                    stageScoresSaveId = await CloudSaveSDK.Instance.Files.CreateAsync(name, bytes, options);
                    Logger.Log("CreateAsync");
                }
                catch (CloudSaveClientException e)
                {
                    Logger.LogError($"failed to save file, name {name}, clientEx: {e.Message}");
                }
                catch (CloudSaveServerException e)
                {
                    Logger.LogError($"failed to save file, name {name}, serverEx: {e.Message}");
                }
            }
            else
            {
                // 更新已有存档
                try
                {
                    // 更新存档选项， 可以通过该方法更新存档文件，也可以仅更新存档配置
                    UpdateOptions options = new UpdateOptions()
                    {
                        File = new FileOptions()
                        {
                            FileBytes = bytes,
                            UpdateFileWay = UpdateFileWay.ByFileBytes
                        },
                    }; 
                    await CloudSaveSDK.Instance.Files.UpdateAsync(stageScoresSaveId, options);
                    Logger.Log("UpdateAsync");
                }
                catch (CloudSaveClientException e)
                {
                    Logger.LogError($"failed to update file, saveId {stageScoresSaveId}, clientEx: {e.Message}");
                }
                catch (CloudSaveServerException e)
                {
                    Logger.LogError($"failed to update file, saveId {stageScoresSaveId}, serverEx: {e.Message}");
                }
            }
        }

        public static void Dispose()
        {
            stageScoresSaveId = string.Empty;
        }
    }
}