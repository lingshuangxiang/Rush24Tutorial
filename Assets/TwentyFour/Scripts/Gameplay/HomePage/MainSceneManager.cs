using System;
using System.Collections;
using System.Collections.Generic;
using Unity.UOS.TwentyFour;
using Unity.UOS.TwentyFour.UOSGateway;
using Unity.VisualScripting;
using UnityEngine;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#endif

namespace Unity.UOS.TwentyFour
{
    public class MainSceneManager : MonoBehaviour
    {

        public void Logout()
        {
            ClientInitHelper.Logout();
        }

        private void OnEnable()
        {
#if UNITY_WEIXINMINIGAME
            if (!TryGetComponent<WXTouchInputOverride>(out WXTouchInputOverride inputOverride))
            {
                gameObject.AddComponent<WXTouchInputOverride>();
            }
#endif
        }

        // Start is called before the first frame update
        void Start()
        {
          
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
