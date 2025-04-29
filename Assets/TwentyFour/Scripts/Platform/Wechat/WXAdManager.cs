using System;
using UnityEngine;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#endif
namespace TwentyFour.Scripts.Wechat
{
    public static class WXAdManager
    {
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        private static WXRewardedVideoAd rewardedVideoAd;
#endif
        public static Action OnRewardedADSucceed;
        public static void Init()
        {
            OnRewardedADSucceed = null;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR

            rewardedVideoAd = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam {
                adUnitId = "adunit-7b220ad9f0bd4e8f", // 替换为实际ID
                multiton = true
            });
            rewardedVideoAd.OnClose(RewardAdClose); // 绑定关闭事件
#endif
        }
      

        public static void ShowRewardAd() {
#if UNITY_EDITOR
            OnRewardedADSucceed?.Invoke();
#elif UNITY_WEIXINMINIGAME && !UNITY_EDITOR
            if (rewardedVideoAd != null) {
                rewardedVideoAd.Show();
            }
#endif
        }
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        static void RewardAdClose(WXRewardedVideoAdOnCloseResponse res) {
            if (res == null || res.isEnded) {
                Debug.LogError("视频播放完成");
                OnRewardedADSucceed?.Invoke();
                // 正常播放完成，发放奖励（如复活、金币）
            } else {
                Debug.LogError("视频播放中途退出");
                // 用户中途退出，提示未完成
            }
        }
#endif

    }
}