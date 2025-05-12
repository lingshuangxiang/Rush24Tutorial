using System;
using System.Collections;
using System.Collections.Generic;
using TwentyFour.Scripts.Common;
using Unity.UOS.TwentyFour;
using Unity.UOS.TwentyFour.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#endif
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class StreamDataCheckHelper : SingletonBehaviour<StreamDataCheckHelper>
{
    DateTime OffShowDateTime;

    public void Init()
    {
        OffShowDateTime = DateTime.Now;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        WX.OffShow(OnShow);
        WX.OnShow(OnShow);
        WX.OffHide(OnHide);
        WX.OnHide(OnHide);
#endif
    }
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
    private void OnHide(GeneralCallbackResult obj)
    {
        OffShowDateTime = DateTime.Now;
    }
#endif
    public void Dispose()
    {
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        WX.OffHide(OnHide);
        WX.OffShow(OnShow);
#endif
    }
   
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
    private void OnShow(OnShowListenerResult obj)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == GameRouter.StartScene)
        {
            Logger.LogInfo("当前场景是启动场景，不做处理");
            return;
        }
        var now = DateTime.Now;
        var updateTime = TimeConverter.GetDataUpdateTime(OffShowDateTime);
        var nowSeconds = TimeConverter.DateTimeToTotalSeconds(now);
        var updateSeconds = TimeConverter.DateTimeToTotalSeconds(updateTime);
        Logger.LogInfo($"当前时间：{now}，下次更新数据时间：{updateTime}");
        if (nowSeconds >= updateSeconds)
        {
            PushHelper.Disconnect();
            Logger.LogInfo($"当前秒数：{nowSeconds}大于更新数据时间秒数：{updateSeconds},更新数据");
            UIManager.Instance.ShowPopUp("提示", "每日数据已更新", (ClientInitHelper.Logout), null, false);
        }
        else
        {
            Logger.LogInfo($"当前秒数：{nowSeconds}小于更新数据时间秒数：{updateSeconds}，不做处理");
        }
        OffShowDateTime = DateTime.Now;
    }
#endif
}
