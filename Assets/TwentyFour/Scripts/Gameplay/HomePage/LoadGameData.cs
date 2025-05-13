using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cloud;
using Passport;
using TMPro;
using TwentyFour.Scripts.Accomplishment;
using TwentyFour.Scripts.Category;
using TwentyFour.Scripts.Metrics;
using TwentyFour.Scripts.Quest;
using TwentyFour.Scripts.RemoteConfig;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.UI;
using Unity.UOS.Common;
using Unity.UOS.Config;
using Unity.UOS.TwentyFour;
using Unity.UOS.TwentyFour.Model;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
   #if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
using Unity.UOS.TwentyFour.Wechat;
#endif
using Logger = Unity.UOS.TwentyFour.Common.Logger;


namespace Unity.UOS.TwentyFour
{
    public class LoadGameData : MonoBehaviour
    {
        [SerializeField] public GameObject CreatePersonaDialog;
        [SerializeField] public Text ProgressTextTmp;

        [SerializeField] private GameObject getWechatUserInfoButton;
        
        void Start()
        {
            //ClientInitHelper.Init();
            StartCoroutine(CheckPersona());
            // await CheckPersona();
        }
        
        IEnumerator CheckPersona()
        {
            yield return StartCoroutine(InitMetrics());
            string realmID = String.Empty;
            Task<string> task = Identity.GetRealmID();
            yield return new WaitUntil(()=>task.IsCompleted);
            if (!string.IsNullOrEmpty(task.Result))
                realmID = task.Result;
            Task<Persona> getPersonaTask = PassportSDK.Identity.GetPersonaByRealm(realmID);
            yield return new WaitUntil(()=>getPersonaTask.IsCompleted);

            var externalLoginAndGotWechatName = false;
            var notExternalLogin = true;
   #if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
            // 微信，检查是否已经有用户名
            if (getPersonaTask.Result != null && !string.IsNullOrEmpty(getPersonaTask.Result.DisplayName))
            {
                externalLoginAndGotWechatName = true;
            }
            notExternalLogin = false;
#endif
            // 非 external login 且已有 persona
            // 或者是 external login 且 persona 已有 display name
            if ((notExternalLogin && getPersonaTask.Result != null) || externalLoginAndGotWechatName)
            {
                Identity.persona = getPersonaTask.Result;

                var startTime = DateTime.Now;
                yield return StartCoroutine(SelectPersonaAndInit());
                var endTime = DateTime.Now;
                var elapsedTime = endTime - startTime;
                MetricsHelper.TrackEvent(MetricsKeys.EVENT_LAUNCH_GAME, new Dictionary<string, object>()
                {
                    {MetricsKeys.PARAM_TIME_COST, elapsedTime.TotalSeconds}
                });
                
                // Task selectPersonaTask = PassportSDK.Identity.SelectPersona(Identity.persona.PersonaID);
                // yield return new WaitUntil(()=>selectPersonaTask.IsCompleted);
                // await PassportSDK.Identity.SelectPersona(Identity.persona.PersonaID);
                Logger.Log($"当前角色为：{Identity.persona.DisplayName}");
                //goto loading page
                // Init();
            } else
            {
                Logger.Log("当前域无角色，请创建新角色");
                //open create persona dialog
                var createPersonaController = CreatePersonaDialog.GetComponent<CreatePersona>();
                createPersonaController.OnCreatePersonaComplete = OnCreatePersonaComplete;
                CreatePersonaDialog.SetActive(true);
                
   #if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
                // 微信平台，且还没有 display name
                createPersonaController.SetUserName(RandomName.Get(getPersonaTask.Result.UserID));
                var wechatUserinfoTask = GetWechatUserInfo.Get(getWechatUserInfoButton);
                yield return new WaitUntil(()=>wechatUserinfoTask.IsCompleted);
                var wechatUserinfo = wechatUserinfoTask.Result;
                if (!string.IsNullOrEmpty(wechatUserinfo.nickName))
                {
                    createPersonaController.SetUserName(wechatUserinfo.nickName);
                }
#endif
            }
            
            
        }
        

        void OnCreatePersonaComplete(Persona p)
        {
            Identity.persona = p;
            Logger.LogInfo($"创建角色成功，角色ID：{Identity.persona.PersonaID}");
            StartCoroutine(SelectPersonaAndInit());
  #if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
            GetWechatUserInfo.Hide();
#endif
        }

        IEnumerator SelectPersonaAndInit()
        {
            Task selectPersonaTask = PassportSDK.Identity.SelectPersona(Identity.persona.PersonaID);
            yield return new WaitUntil(()=>selectPersonaTask.IsCompleted);
            var personaPropertiesTaskAwaiter = PersonaPropertiesHelper.GetPersonaProperties().GetAwaiter();
            yield return new WaitUntil(() => personaPropertiesTaskAwaiter.IsCompleted);
            PersonaPropertiesHelper.Init();
            var tierAwaiter = TiersHelper.GetMyLeaderboardScore(TiersHelper.TiersLeaderboardSlugName).GetAwaiter();
            yield return new WaitUntil(() => tierAwaiter.IsCompleted);
            yield return StartCoroutine(Init());
        }
        
        // Start is called before the first frame update
        IEnumerator Init()
        {
            MetricsHelper.SetUser();
            MuninnManager.Initialize();
            yield return StartCoroutine(InitPush());
            yield return StartCoroutine(InitRemoteConfig());
            yield return StartCoroutine(InitStage());
            yield return StartCoroutine(InitSave());
            yield return StartCoroutine(InitAchievement());
            yield return StartCoroutine(InitQuest());
            yield return StartCoroutine(InitCategory());
            //yield return AccomplishmentHelper.GetData(Identity.persona.PersonaID);
            yield return StartCoroutine(FetchLeaderboard());
            StreamDataCheckHelper.Instance.Init();
            if (PersonaPropertiesHelper.ShowStageTutorial)
            {
                MetricsHelper.TrackEvent(MetricsKeys.EVENT_CREATE_NEW_PRESONA);
                StageManager.selectedStage = 0;
                GameRouter.LoadStageGameScene();
            }
            else
            {
                GameRouter.LoadHomeSceneFirst();
            }
            // StageManager.LoadAllStagesFromCSV();
            // UOSSave.Init();
            // GameRouter.instance.LoadHomeScene();
        }

        IEnumerator InitStage()
        {
            ProgressTextTmp.text = "正在...构建世界...";
            // Task task = StageManager.LoadAllStagesFromCSV(); 
            // // UIMessage.Show("加载游戏数据...");
            // yield return new WaitUntil(()=>task.IsCompleted);
            StageManager.LoadAllStagesFromRemoteConfig();
            yield return null;
        }
        
        IEnumerator InitSave()
        {
            ProgressTextTmp.text = "正在...了解过去...";
            Task task = UOSSave.Init();
            // UIMessage.Show("加载玩家数据...");
            yield return new WaitUntil(()=>task.IsCompleted);
        }

        IEnumerator InitRemoteConfig()
        {
            RemoteConfigHelper.Init();
            var t = RemoteConfigHelper.GetDefaultRemoteConfig();

            var overridesConfigAwaiter = RemoteConfigHelper.GetOverridesRemoteConfig().GetAwaiter();
            yield return new WaitUntil(()=>t.IsCompleted && overridesConfigAwaiter.IsCompleted);
            // var currentTournament = RemoteConfigHelper.GetString(RemoteConfigKeys.CurrentTournamentSlug);
            // var data = RemoteConfigHelper.GetJsonSO<TournamentData>(currentTournament);
            // Debug.LogError(data.DisplayName+" 当前锦标赛");
            yield return null;
        }

        IEnumerator InitAchievement()
        {
            ProgressTextTmp.text = "正在...构建指令...";
            var fetch = AchievementManager.FetchAchievement();
            yield return new WaitUntil(()=>fetch.IsCompleted);
            var initAchievement =  AchievementManager.InitAchievements();
            yield return new WaitUntil(()=>initAchievement.IsCompleted);
        }

        IEnumerator InitQuest()
        {
            var quest = QuestHelper.FetchPersonaQuests();
            var defaultQuests = QuestHelper.SearchPersonaQuests();
            yield return new WaitUntil(()=>defaultQuests.IsCompleted && quest.IsCompleted);
        }
        
        IEnumerator InitCategory()
        {
            ProgressTextTmp.text = "正在...加载货舱...";
            CategoryHelper.Init();
            var categories = CategoryHelper.ListCategories();
            yield return new WaitUntil(()=>categories.IsCompleted);
            CategoryHelper.CheckDefaultCategoryUpdated();
            var defalut = CategoryHelper.ListProducts(CategoryHelper.DefaultGOLDCategory);
            var vit = CategoryHelper.ListProducts(CategoryHelper.DefaultVITCategory);
            yield return new WaitUntil(()=>defalut.IsCompleted && vit.IsCompleted);
        }

        IEnumerator InitMetrics()
        {
            var init = MetricsHelper.Init().GetAwaiter();
            yield return new WaitUntil(() => init.IsCompleted);
        }
        IEnumerator FetchLeaderboard()
        {
            ProgressTextTmp.text = "正在...校准文明...";
            var leaderboardCount = RemoteConfigHelper.GetInt(RemoteConfigKeys.DefaultRankCount);
            var count = leaderboardCount == 0 ? 20 : leaderboardCount;
            var leaderboardlistAwaiter =
                TiersHelper.ListTierLeaderBoard(count).GetAwaiter();

            var personaPropertyAwaiter = PersonaPropertiesHelper.GetPersonaProperties().GetAwaiter();
            var leaderBoardAwaiter =
                TiersHelper.GetMyLeaderboardScore(TiersHelper.TiersLeaderboardSlugName).GetAwaiter();

            yield return TiersHelper.GetTierUserScoreData(Identity.persona.PersonaID);

            yield return new WaitUntil(() =>
                leaderboardlistAwaiter.IsCompleted && personaPropertyAwaiter.IsCompleted &&
                leaderBoardAwaiter.IsCompleted);
        }

        IEnumerator InitPush()
        {
            ProgressTextTmp.text = "正在...链接中枢...";
            var init = PushHelper.Initialize();
            yield return new WaitUntil(()=>init.IsCompleted);
            var connect = PushHelper.ConnectAsync();
            yield return new WaitUntil(()=>connect.IsCompleted);
        }
        // Update is called once per frame
        void Update()
        {
           
        }
    }
}
