using System;
using System.Diagnostics;
using System.IO;
using Unity.UOS.Common;
using Unity.UOS.TwentyFour.Editor.DevEnv;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;


#if UNITY_WEIXINMINIGAME
using WeChatWASM;
#endif

namespace Unity.UOS.TwentyFour.Editor
{
    public class BuildHelper
    {
        const string PROJECT_NAME = "Rush24";
        
        const string TARGET_DIR = "Build";

        static string CDN_PATH =
            $"https://a.unity.cn/client_api/v1/buckets/{BUCKETID}/release_by_badge/latest/content/";

        static string UOSAPPID => Settings.AppID;
        static string AppServiceSecret => Settings.AppServiceSecret;
        static string BUCKETID => GetCDNBUCKETID();
        
        [MenuItem("/Tools/Build/WXMINIGAME_RELEASE")]
        private static void ShowBuildWXRelease()
        {
            bool result = EditorUtility.DisplayDialog("一键Release出包&上传", "即将一键打包并上传资源至CDN&发布CDN_Release，请确认是否执行", "OK", "Cancel");
            if (result)
            {
                Debug.Log("开始一键Release出包&上传");
                BUILD_WXMINGAME_RELEASE();
            }
        }
        [MenuItem("/Tools/Build/WXMINIGAME")]
        static void BUILD_WXMINGAME_MENU()
        {
            BUILD_WXMINGAME();
        }
        [MenuItem("/Tools/Build/WXMINIGAME_DEV")]
        static void BUILD_WXMINGAME_MENU_DEV()
        {
            DevEnvHelper.SwitchEnv((int)DevEnvHelper.DevEnv.Dev);
            BUILD_WXMINGAME(false,(() =>
            {
                UPLOAD_CDN();
            }));
        }
        
        [MenuItem("/Tools/Build/WXMINIGAME_DEBUG")]
        static void BUILD_WXMINGAME_DEBUG()
        {
            BUILD_WXMINGAME(true);
        }
        static void BUILD_WXMINGAME_RELEASE()
        {
            DevEnvHelper.SwitchEnv((int)DevEnvHelper.DevEnv.Prod);
            BUILD_WXMINGAME(false,(() =>
            {
                UPLOAD_CDN(UPLOAD_WX_MP);
            }));
        }
        
        static void BUILD_WXMINGAME(bool debug = false ,Action succeed = null)
        {
            WXEnvironmentHelper.CheckWXEnvironment();
#if UNITY_WEIXINMINIGAME
            Debug.Log("开始打包微信小游戏");
            WXEditorScriptObject config = UnityUtil.GetEditorConf();
            Debug.Log(config.ProjectConf.CDN);
            config.ProjectConf.CDN = CDN_PATH;
            config.CompileOptions.DevelopBuild = debug;
            config.CompileOptions.AutoProfile = debug;
            config.CompileOptions.Il2CppOptimizeSize = true;
            
            string dir = $"{Directory.GetCurrentDirectory()}\\{TARGET_DIR}\\{PROJECT_NAME}_WXMINIGAME";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            Debug.Log(dir);
            
            config.ProjectConf.DST = dir;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            if (WXConvertCore.DoExport(true) == WXConvertCore.WXExportError.SUCCEED)
            {
                succeed?.Invoke();
            }
            
#endif
        }

        public static string GetWXProjectPath()
        {
            string dir = $"{Directory.GetCurrentDirectory()}\\{TARGET_DIR}\\{PROJECT_NAME}_WXMINIGAME\\minigame";
            if (!Directory.Exists(dir))
            {
                return string.Empty;
            }
            return dir;
        }
        static void UPLOAD_CDN(Action<string> onCompleted = null)
        {
            string uploaddir = $"{Directory.GetCurrentDirectory()}\\{TARGET_DIR}\\{PROJECT_NAME}_WXMINIGAME\\webgl\\";
            string cli = $"{Directory.GetCurrentDirectory()}\\Tools\\uas.exe";
            Debug.Log($"UOS : appid :{UOSAPPID}  service_secret: {AppServiceSecret}  {PlayerSettings.bundleVersion}");
            var commitHash = RunProcess("git","log -1 --format=%H")?.Substring(0, 8);
            RunProcess(cli, $"auth login --uos_app_id {UOSAPPID} --uos_app_secret {AppServiceSecret}");
            RunProcess(cli,$"entries sync --bucket {BUCKETID}  {uploaddir}");
            RunProcess(cli,$"releases create --bucket {BUCKETID} --notes {UosAppConfigs.GetUosAppConfigs().CurrentEnv}_{PlayerSettings.bundleVersion}_{commitHash}");
            onCompleted?.Invoke(commitHash);
        }

        [MenuItem("/Tools/WX/上传小游戏至微信MP后台")]
        static void MANUAL_UPLOAD_WXMP()
        {
            bool result = EditorUtility.DisplayDialog("上传小游戏至微信MP后台", $"上传当前版本{PlayerSettings.bundleVersion}小游戏至微信MP后台，请确认是否执行", "OK", "Cancel");
            if (result)
            {
                Debug.Log("MANUAL_UPLOAD_WXMP");
                var commitHash = RunProcess("git","log -1 --format=%H")?.Substring(0, 8);
                UPLOAD_WX_MP(commitHash);
            }
            
        }
        static void UPLOAD_WX_MP(string commitInfo)
        {
            var js = WXUploadHelper.GenerateJSString(GetWXProjectPath(), commitInfo);
            if (string.IsNullOrEmpty(js))
            {
                Debug.LogError("生成JS文件失败");
                return;
            }
            RunProcess("node", js);
            File.Delete(js);

        }
        static string RunProcess(string cmd, string args)
        {
#if UNITY_EDITOR_WIN
            Process process = new Process();
            process.StartInfo.FileName = cmd;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd(); 
            process.WaitForExit();
            Debug.Log(output);
            return output;
#endif
            return string.Empty;
        }

        static string GetCDNBUCKETID()
        {
            var env = UosAppConfigs.GetUosAppConfigs().CurrentEnv;
            foreach (var infos in DevEnvData.GetDevEnvData().DevEnvInfos)
            {
                if (infos.name == env)
                {
                    Debug.Log($"ENV:{env} CDN_BUCKET:{infos.cdn_bucket}");
                    return infos.cdn_bucket;
                }
            }
            return string.Empty;
        }
    }
    
    
}