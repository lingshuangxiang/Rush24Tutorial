using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#endif
using Logger = TwentyFour.Scripts.Utilities.Logger;

namespace TwentyFour.Scripts.Utilities
{
    public static class CopyPasteUtil
    {
        public static void Copy(string content)
        {

#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        WX.SetClipboardData(new SetClipboardDataOption()
        {
            data = content,
            success = (res) =>
            {
                Logger.LogInfo("WX Copied to clipboard: " + content);
            }
        });
#else
            GUIUtility.systemCopyBuffer = content;
            Logger.Log("Copied to clipboard: " + content);
#endif
        }

        public static void Paste(InputField target)
        {

#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        WX.GetClipboardData(new GetClipboardDataOption()
        {
            success = (res) =>
            {
                target.text = res.data;
                Logger.LogInfo("WX Paste to clipboard: " +res.data);
            }
        });
#else
            target.text = GUIUtility.systemCopyBuffer;
            Logger.Log("Paste clipboard: " + GUIUtility.systemCopyBuffer);
#endif
        }
    }
}