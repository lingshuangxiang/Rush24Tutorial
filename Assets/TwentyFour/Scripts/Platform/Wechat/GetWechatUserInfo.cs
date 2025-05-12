using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#endif
using System;

namespace Unity.UOS.TwentyFour.Wechat
{
    /// <summary>
    /// 获取微信用户信息
    /// </summary>
    public class GetWechatUserInfo
    {
 #if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        private static WXUserInfoButton userInfoButton;
        /// <summary>
        /// 获取微信 userinfo
        /// </summary>
        public static Task<UserInfo> Get(GameObject getWechatUserInfoButton, 
            Action showWechatButtonCallback = null,
            Action<UserInfo> OnGetNewUserInfo = null)
        {
            var tcs = new TaskCompletionSource<UserInfo>();
            WX.GetSetting(new GetSettingOption()
            {
                fail = (err) =>
                {
                    Debug.Log("get setting fail");
                    Debug.Log(err.errMsg);
                    tcs.SetResult(null);
                },
                success = (res) =>
                {
                    if (res.authSetting.TryGetValue("scope.userInfo", out var hasUserInfo))
                    {
                        if (hasUserInfo)
                        {
                            // 已授权
                            WX.GetUserInfo(new GetUserInfoOption()
                            {
                                success = userInfoRes =>
                                {
                                    Debug.Log("get user info success. nickname:");
                                    Debug.Log(userInfoRes.userInfo.nickName);
                                    tcs.SetResult(userInfoRes.userInfo);
                                },
                                fail = err =>
                                {
                                    tcs.SetResult(null);
                                }
                            });
                        }
                    }

                    // 未授权
                    if (!hasUserInfo)
                    {
                        if (showWechatButtonCallback != null)
                        {
                            showWechatButtonCallback.Invoke();
                        }
                        var wechatButton = getWechatUserInfoButton.GetComponent<GetWechatUserInfoButton>();
                        var rect = wechatButton.GetScreenPosition();
                        // 绘制全屏区域按钮
                        userInfoButton =
 WX.CreateUserInfoButton((int)rect.x, Screen.height - (int)rect.y -  (int)rect.height, (int)rect.width, (int)rect.height, "", true);

                  
                        userInfoButton.OnTap((tapRes) =>
                        {
                            Debug.Log("press button");
                            // button.Hide();
                            WX.GetUserInfo(new GetUserInfoOption()
                            {
                                success = userInfoRes =>
                                {
                                    // 用户授权
                                    Debug.Log("get user info success. nickname:");
                                    Debug.Log(userInfoRes.userInfo.nickName);
                                    tcs.SetResult(userInfoRes.userInfo);
                                    if (OnGetNewUserInfo != null)
                                    {
                                        OnGetNewUserInfo.Invoke(userInfoRes.userInfo);
                                    }
                                },
                                fail = (err) =>
                                {
                                    // 用户未授权
                                    Debug.Log("get user info fail");
                                    Debug.Log(err.errMsg);
                                    tcs.SetResult(null);
                                }
                            });
                        });
                    }
                },
            });

            return tcs.Task;
        }

        public static void Hide()
        {
            if(userInfoButton != null)
                userInfoButton.Hide();
        }
#endif
    }
}