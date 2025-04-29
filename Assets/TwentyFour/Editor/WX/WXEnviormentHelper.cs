using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Purchasing.MiniJSON;

namespace Unity.UOS.TwentyFour.Editor
{
    public class WXEnvironmentHelper
    {
        public static string WXSymbol = ";UNITY_WEIXINMINIGAME";
        [MenuItem("Tools/WX/导入微信环境")]
        public static void AddWXPackage()
        {
            
            string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL);
            if (!currentSymbols.Contains(WXSymbol))
            {
                Debug.Log("未添加微信宏定义，即将添加微信微信宏定义");
                currentSymbols += $";{WXSymbol}";
                Debug.Log(currentSymbols);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL, currentSymbols);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            string manifestContent = File.ReadAllText(manifestPath);
            if (!manifestContent.Contains("weixin"))
            {
                Debug.Log("未添加微信小游戏转换工具，即将添加微信小游戏转换工具");
                var jsonDict = Json.Deserialize(manifestContent) as Dictionary<string, object>;
                Dictionary<string,object> dependencies = jsonDict["dependencies"] as Dictionary<string, object>;
                if (dependencies != null)
                {
                    dependencies["com.qq.weixin.minigame"] = "https://gitee.com/wechat-minigame/minigame-tuanjie-transform-sdk.git";
                }
                var c = JsonConvert.SerializeObject(jsonDict, Formatting.Indented);
                File.WriteAllText(manifestPath, c);
                Debug.Log("Package added to manifest.json. Please restart Unity if needed.");
                
                Client.Resolve();
                
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
                
            }
        }

        public static void CheckWXEnvironment()
        {
            string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.WebGL);
            if (!currentSymbols.Contains(WXSymbol))
            {
                Debug.Log("未添加微信环境，即将添加微信环境");
                AddWXPackage();
            }
            Debug.Log("微信环境已添加");
        }
    }
}