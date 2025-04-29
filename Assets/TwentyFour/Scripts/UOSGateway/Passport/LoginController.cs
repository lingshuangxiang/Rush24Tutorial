using System;
using System.Collections;
using System.Collections.Generic;
using Passport;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.UI;
using Unity.UOS.Common;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using Unity.UOS.Auth;
using System.Threading.Tasks;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using CloudService;
using WeChatWASM;
#endif
using Unity.UOS.Networking;
using UnityEngine.UI;

namespace Unity.UOS.TwentyFour
{
    public class LoginController : MonoBehaviour
    {
        public GameObject PlayerDataLoadingPage;
        public Text CoverPageHintText;
        
        // [SerializeField] public GameObject LoadingPage;
        // [SerializeField] public GameObject CreatePersonaDialog;
        // [SerializeField] private LoadGameData gameData;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        public static UserInfo WechatUserInfo;
#endif
        public delegate void CreatePersonaCompleteCallback(Persona persona);
        
        // sdk 配置（Config 是 SDK 初始化时的配置）
        private readonly PassportUIConfig _config = new()
        {
            AutoRotation = true, // 是否开启自动旋转，默认值为 false。
            InvokeLoginManually = false, // 是否通过自行调用 Login 函数启动登录面板，默认值为 false。
            Theme = PassportUITheme.Dark, // 风格主题配置。
            #if TUANJIE_1_0_OR_NEWER
            UnityContainerId = "tuanjie-container" // WebGL 场景下 Tuanjie 实例容器 Id。
            #else
            UnityContainerId = "unity-container" // WebGL 场景下 Unity 实例容器 Id。
            #endif
        };
 
        // sdk 回调
        private void _callback(PassportEvent e)
        {
            // event: 不同情况下的回调事件，详情可以参考下面的回调类型。
            switch (e)
            {
                case PassportEvent.RejectedTos:
                    Debug.Log("用户拒绝了协议");
                    
                    //Quit game if TOS rejected
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;
                case PassportEvent.LoggedIn:
                    Debug.Log("完成登录");
                    break;
                case PassportEvent.Completed:
                    Debug.Log("完成所有流程");
                    // await CheckPersona();
                    GotoLoadingPage();
                    break;
                case PassportEvent.LoggedOut:
                    Debug.Log("用户登出");
                    break;
            }

        }

        // Start is called before the first frame update
        async void Start()
        {
            //Play initBGM as login BGM
            BGMManager.Instance.Play(BGMManager.BackgroundMusic.InitBGM);
            
            if (string.IsNullOrEmpty(Settings.AppID))
            {
                Debug.LogError("Empty App Info! Please open the menu in editor: UOS -> Open Launcher, and enter UOS App info. For more tutorial, open the menu in editor: Tutorial -> Show Tutorial.");
            }
            
            StartCoroutine(InitLogin());
        }
        
        IEnumerator InitLogin()
        {
            //Resolve Logout Navigation
            if (GameRouter.isLoggingOut)
            {
                PassportLoginSDK.Identity.Logout();
                GameRouter.isLoggingOut = false;
            }
            
            CoverPageHintText.text = "正在...感应身份...";
            yield return 100;
            
            //Init Passport Login UI 
            Login();
        }

        public async void Login()
        {
            await PassportFeatureSDK.Initialize();
            // 调用 SDK
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
            // external login
            
            // 检查 token 是否已经存在
            // try
            // {
            //     await AuthTokenManager.GetTokenInfo();
            //     _callback(PassportEvent.Completed);
            //     return;
            // }
            // catch (Exception e)
            // {
            //     // token 获取失败，继续原登录流程
            //     Debug.Log(e.Message);
            // }
            
            // UIMessage.Show("微信登录中...");
            var externalLoginResponse = await WechatLogin();
             
            if (externalLoginResponse == null)
            {
                var exp = "微信登录失败，请稍后重试";
                UIMessage.Show(exp, MessageType.Error);
                throw new Exception(exp);
            }
            TokenInfo tokeninfo = new TokenInfo();
            tokeninfo.AccessToken = externalLoginResponse.personaAccessToken;
            tokeninfo.RefreshToken = externalLoginResponse.personaRefreshToken;
            tokeninfo.UserId = externalLoginResponse.persona.userID;
            AuthTokenManager.SaveToken(tokeninfo);
            
            _callback(PassportEvent.Completed);
#else
            // passport login
            await PassportSDK.Initialize();
            await PassportUI.Init(_config, _callback);
#endif
        }

#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        /// <summary>
        /// 微信登录
        /// </summary>
        public static async Task<ExternalLoginResponse?> WechatLogin()
        {
            var tcs = new TaskCompletionSource<ExternalLoginResponse?>();
            WX.Login(new LoginOption()
            {
                success = async (res) =>
                {
                    var wechatApi = new WechatAPI();
                    Debug.Log("微信获取 code 成功");
                    Debug.Log(res.code);
                    var externalLoginResponse = await wechatApi.WechatLogin(res.code);
                    tcs.SetResult(externalLoginResponse);
                    WXSubscribe.Init(externalLoginResponse.openid);
                    //Debug.LogError(externalLoginResponse.openid+"OpenID");
                },
                fail = (err) =>
                {
                    Debug.Log("微信获取 code 失败");
                    Debug.Log(err.errMsg);
                    tcs.SetResult(null);
                }
            });

            return await tcs.Task;
        }
#endif
        
        

        // 登出
        public void Logout()
        {
            GameRouter.BackAndLogout();
            PassportUI.Logout();
        }
        
        void GotoLoadingPage()
        {
            PlayerDataLoadingPage.SetActive(true);
        }
        
    }
}