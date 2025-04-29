using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#endif
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class VibrateHelper
{
    static string[] vibratePattern = new string[] {"heavy","medium","light"};
    public static void Vibrate(int pattern = 1)
    {
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        if (pattern < 0 || pattern > 2)
        {
            pattern = 0;
        }
        var options = new VibrateShortOption();
        options.success = result =>
        {
            Logger.Log("Vibrate success");
        };
        options.fail = result =>
        {
            Logger.LogError($"Vibrate failed,{result.errMsg}");
        };
        options.complete = result =>
        {
            Logger.Log("Vibrate complete");
        };
        options.type = vibratePattern[pattern];
        WX.VibrateShort(options);
#endif
    }
    
    public static void VibrateLong()
    {
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        var options = new VibrateLongOption();
        options.success = result =>
        {
            Logger.Log("VibrateLong success");
        };
        options.fail = result =>
        {
            Logger.LogError($"VibrateLong failed,{result.errMsg}");
        };
        options.complete = result =>
        {
            Logger.Log("VibrateLong complete");
        };
        WX.VibrateLong(options);
#endif
    }
}
