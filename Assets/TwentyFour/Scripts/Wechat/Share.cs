#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#endif
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using Unity.UOS.TwentyFour.Scripts.Battle.Model;


namespace TwentyFour.Scripts.Wechat
{
    public class Share
    {
        /// <summary>
        /// 主动转发
        /// </summary>
        public static void ShareApp(string roomId , BattleMode battleMode)
        {
            var image = Path.Combine(Application.streamingAssetsPath, "ShareCover.png");
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
            WX.ShareAppMessage(new ShareAppMessageOption()
            {
                title = $"{Identity.persona.DisplayName}说算24点没服过谁，点击迎战！",
                query =
 $"roomid={roomId}&fromplayername={Identity.persona.DisplayName}&battlemode={battleMode.ToString().ToLower()}",
                imageUrl = image,
            });
#endif
        }

        public static Dictionary<string, string> GetLaunchOptionsSync()
        {
            Debug.Log("GetLaunchOptionsSync");
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
            var option = WX.GetLaunchOptionsSync();
            return option.query;
#endif
            return new Dictionary<string,string>();
        }

        public static void AddWxOnShowHandler(Action<Dictionary<string,string>> handler)
        {
            
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
            var image = Path.Combine(Application.streamingAssetsPath, "DefaultShare.png");
            WX.OnShow((res) =>
            {
                handler(res.query);
            });
            WX.OnShareAppMessage(new WXShareAppMessageParam()
            {
                imageUrl = image,
                title = "快来进行一场24点速算大比拼！"
            });
#endif
        }
    }
}

