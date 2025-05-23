using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TwentyFour.Scripts.Gameplay.HomePage;
using TwentyFour.Scripts.Utilities;

namespace TwentyFour.Scripts.Features.Player
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
        
        // Start is called before the first frame update
        void Start()
        {
            //Play initBGM as login BGM
            BGMManager.Instance.Play(BGMManager.BackgroundMusic.InitBGM);
            
            StartCoroutine(InitLogin());
        }
        
        IEnumerator InitLogin()
        {
            //Resolve Logout Navigation
            if (GameRouter.isLoggingOut)
            {
                GameRouter.isLoggingOut = false;
            }
            
            CoverPageHintText.text = "正在...感应身份...";
            yield return 100;
            
            //Init Passport Login UI 
            Login();
            GotoLoadingPage();

        }

        public void Login()
        {
            var personaIDKey = "personaID";
            var personaNameKey = "personaName";
            var personaID = PlayerPrefs.GetString(personaIDKey);
            var personaName = PlayerPrefs.GetString(personaNameKey);
            if (String.IsNullOrEmpty(personaID))
            {
                personaID = Guid.NewGuid().ToString();
                PlayerPrefs.SetString(personaIDKey, personaID);
            }
            
            Identity.persona = new Persona()
            {
                PersonaID = personaID,
                DisplayName = personaName
            };
        }
        
        
        

        // 登出
        public void Logout()
        {
            GameRouter.BackAndLogout();
        }
        
        void GotoLoadingPage()
        {
            PlayerDataLoadingPage.SetActive(true);
        }
        
    }
}