using System.Collections;
using System.Collections.Generic;
using Passport;
using TMPro;
using Unity.Passport.Runtime;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.UOS.TwentyFour
{
    public class CreatePersona : MonoBehaviour
    {
        [SerializeField]public InputField PersonaNameText;
        [SerializeField] private GameObject getWechatUserInfoButton;
        [SerializeField] private Text personaNameTextPlaceholder;
        [SerializeField] private GameObject confirmButton;

        public LoginController.CreatePersonaCompleteCallback OnCreatePersonaComplete;
        
        // Start is called before the first frame update
        void Start()
        {
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
            getWechatUserInfoButton.SetActive(true);
            PersonaNameText.enabled = false;
            personaNameTextPlaceholder.text = "请授权微信昵称";
#endif

        }

        public void SetUserName(string userName)
        {
            PersonaNameText.text = userName;
        }

        /// <summary>
        /// 创建 persona 或者更新 persona 信息
        /// </summary>
        public async void Create()
        {
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
            // 更新 passport 中角色信息
            var updatedPersona  = await PassportSDK.Identity.UpdatePersona(PersonaNameText.text, "", new Dictionary<string, string>
            {
                // { "uuid", externalLoginResponse.openid }
            });
            OnCreatePersonaComplete(updatedPersona);
            return;            
#endif
            if (string.IsNullOrEmpty(PersonaNameText.text))
            {
                Debug.LogError("Empty persona name");
            }
            try
            {
                var realmID = await Identity.GetRealmID();
                Persona persona =  await PassportSDK.Identity.CreatePersona(PersonaNameText.text, realmID);
                Debug.Log("成功创建角色");
                
                OnCreatePersonaComplete(persona);
            }
            catch (PassportException e)
            {
                Debug.Log(e.Code);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}