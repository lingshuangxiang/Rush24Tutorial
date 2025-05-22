using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.UOS.TwentyFour;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
   #if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
using Unity.UOS.TwentyFour.Wechat;
#endif
using Logger =TwentyFour.Scripts.Utilities.Logger;


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
            if ((notExternalLogin) || externalLoginAndGotWechatName)
            {
                var startTime = DateTime.Now;
                yield return StartCoroutine(SelectPersonaAndInit());
                var endTime = DateTime.Now;
                var elapsedTime = endTime - startTime;
                
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
            yield return StartCoroutine(Init());
        }
        
        // Start is called before the first frame update
        IEnumerator Init()
        {
            yield return StartCoroutine(InitStage());
            yield return StartCoroutine(InitSave());
            GameRouter.LoadHomeSceneFirst();
            
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
            UOSSave.Init();
            yield break;
        }
        


        // Update is called once per frame
        void Update()
        {
           
        }
    }
}
