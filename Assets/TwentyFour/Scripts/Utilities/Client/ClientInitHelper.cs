using System.Collections;
using System.Collections.Generic;
using TwentyFour.Scripts.LaunchParam;
using TwentyFour.Scripts.Wechat;
using Unity.UOS.Common;
using Unity.UOS.TwentyFour;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;

public static class ClientInitHelper
{
    public static void Init()
    {
        ScreenOption.IsKeepScreenOn = false;
        LaunchParamsHelper.Instance.Init();
        Application.targetFrameRate = 60;
        UIManager.Instance.Init();
        WXAdManager.Init();
    }

    public static void Logout()
    {
        //清除旧帐号数据
        UOSSave.Dispose();
        PersonaPropertiesHelper.Dispose();
        AchievementManager.Dispose();
        MuninnManager.Singleton.Dispose();
        TiersHelper.Dispose();
        WXSubscribe.Dispose();
        StreamDataCheckHelper.Instance.Dispose();
        MetricsHelper.Dispose();

        //退出登录
        PushHelper.Disconnect();
        GameRouter.BackAndLogout();
    }
    
}
