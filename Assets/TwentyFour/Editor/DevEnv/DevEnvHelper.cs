using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.UOS.TwentyFour.Editor.DevEnv;
using UnityEditor;
using UnityEngine;
using Unity.UOS.Common;


public class DevEnvHelper : EditorWindow
{
    private const string APPID_PROD = "cbe05e3b-6d6f-4e08-b0d6-80ba62cd790d";
    private const string APP_SECRET_PROD = "bb4430ad661a4642a33af17acf5b833a";
    private const string APP_SERVICE_SECRET_PROD = "91cf11bce9ae400281c27452b7cf61fb";
    
    // [MenuItem("/Tools/切换环境/测试")]
    // private static void ShowBuildWXRelease()
    // {
    //     
    // }
    
    private static int selectedOptionIndex = 0; // 当前选中的选项索引
    private static List<string> options = new List<string>();

    private static List<DevEnvInfo> devEnvInfos = new List<DevEnvInfo>();
    [MenuItem("Tools/切换环境")]
    public static void ShowWindow()
    {
        devEnvInfos = DevEnvData.GetDevEnvData().DevEnvInfos;
        options?.Clear();
        foreach (var info in devEnvInfos)
        {
            options.Add(info.name);
        }
        GetWindow<DevEnvHelper>("切换开发环境");
    }

    private void OnGUI()
    {
        if(Application.isPlaying) return;
        
        if (options == null || options.Count == 0) return;
        GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.label);
        boldLabelStyle.fontStyle = FontStyle.Bold; // 设置字体样式为加粗
        EditorGUILayout.LabelField("当前环境 ：" + UosAppConfigs.GetUosAppConfigs().CurrentEnv, boldLabelStyle);
        EditorGUILayout.LabelField("APPID ：" + Settings.AppID);

        GUILayout.Space(10);
        // 创建下拉框
        selectedOptionIndex = EditorGUILayout.Popup("选择想要切换的环境", selectedOptionIndex, options.ToArray());

        GUILayout.Space(10);


        if (GUILayout.Button("切换环境"))
        {
            SwitchEnv(selectedOptionIndex);
        }
    }

    public static void SwitchEnv(int index)
    {
        devEnvInfos = DevEnvData.GetDevEnvData().DevEnvInfos;
        Settings.AppID = devEnvInfos[index].app_id;
        Settings.AppSecret = devEnvInfos[index].app_secret;
        Settings.AppServiceSecret = devEnvInfos[index].app_service_secret;
        var config = UosAppConfigs.GetUosAppConfigs();
        if (config != null)
        {
            config.MatchConfigId = devEnvInfos[index].match_config_id;
            config.RoomProfileUUID = devEnvInfos[index].room_profile_UUID;
            config.CurrentEnv = devEnvInfos[index].name;
            config.AccoplishStatelessUrl = devEnvInfos[index].accoplish_stateless_url;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }
        Debug.Log($"切换成功 : {devEnvInfos[index].name}");
    }


    public enum DevEnv
    {
        Prod,
        Dev,
    }
}
