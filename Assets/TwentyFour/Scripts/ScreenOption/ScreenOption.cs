using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class ScreenOption
{
    private static bool isKeepScreenOn = false;

    public static bool IsKeepScreenOn
    {
        get => isKeepScreenOn; 
        set
        {
            isKeepScreenOn = value; 
            KeepScreenOn(value); 
        } 
    }

    static void KeepScreenOn(bool v)
    {
        Screen.sleepTimeout = v ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        WXSetKeepScreenOn(v);
        if(v)
            WeChatWASM.WX.OnShow(OnWXShowCallBack);
        else
            WeChatWASM.WX.OffShow(OnWXShowCallBack);
            
#endif
    }
    
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
    private static void OnWXShowCallBack(WeChatWASM.OnShowListenerResult result)
    {
        WXSetKeepScreenOn();
    }

    private static void WXSetKeepScreenOn(bool v = true)
    {
        WeChatWASM.SetKeepScreenOnOption onKeepScreenOnOption = new WeChatWASM.SetKeepScreenOnOption()
        {
            keepScreenOn = v,
            fail = (result) => { Logger.LogError(result.errMsg); }
        };
        WeChatWASM.WX.SetKeepScreenOn(onKeepScreenOnOption);
        Logger.Log("WXSetKeepScreenOn");
    }
#endif
}
