using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFour.Scripts.Features.Player
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
        public void Create()
        {
            if (string.IsNullOrEmpty(PersonaNameText.text))
            {
                Debug.LogError("Empty persona name");
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}