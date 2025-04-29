using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud;
using TwentyFour.Scripts.Tournament;
using Unity.UOS.Config;
using Unity.UOS.Config.Model;
using Unity.UOS.Config.Utility;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class RemoteConfigHelper
{
    static RemoteConfigData LocalRemoteConfigData;
    static GetPlayerSettingsRequest DefaultConfigsRequest;
    public static Dictionary<string,Setting> RemoteConfigs = new Dictionary<string, Setting>();
    public static void Init()
    {
        try
        {
            RemoteConfigSDK.Initialize();
            //LocalRemoteConfigData = Resources.Load<RemoteConfigData>("RemoteConfigData");
            DefaultConfigsRequest = new GetPlayerSettingsRequest()
            {
                // Keys = { LocalRemoteConfigData.Keys },
                // Types_ = { LocalRemoteConfigData.Types }
            };
        }
        catch (Exception e)
        {
            Logger.LogError($"RemoteConfigHelper Init Error:{e}");
        }
    }

    public static async Task<UosRemoteConfigResult<GetPlayerSettingsResponse>> GetDefaultRemoteConfig()
    {
        var settings =await RemoteConfigSDK.GetPlayerSettingsAsync(DefaultConfigsRequest);
        if (settings.IsSuccess())
        {
            GetPlayerSettingsResponse response = settings.data;
            // handle it
            RemoteConfigs = response.Settings.ToDictionary(x => x.Key, x => x.Value);
            var sb = new StringBuilder();
            foreach (var entry in response.Settings)
            {
                sb.AppendLine($"Remote Config: {entry.Key} - {entry.Value} ,type: {entry.Value.Type}");
            }
            Logger.Log(sb);
            
        }
        else
        {
            Logger.LogError($"RemoteConfigHelper GetDefaultRemoteConfig Error");
        }
        
        return settings;
    }

    public static async Task<UosRemoteConfigResult<GetPlayerOverridesResponse>> GetOverridesRemoteConfig()
    {
        var appAttributes = new RemoteAppAttributes()
        {
            appVersion = Application.version
        };
        var userAttributes = new RemoteUserAttributes()
        {
            score = (int)PersonaPropertiesHelper.MyLeaderboardScore.Score,
            tier = PersonaPropertiesHelper.MyLeaderboardScore.Tier,
            personaId = Identity.persona.PersonaID
            
        };
        var req = new GetPlayerOverridesRequest
        {
            UserId = Identity.persona.UserID,
            PersonaId = Identity.persona.PersonaID,
            IsDebugBuild = false,
            PackageVersion = Application.version,
            Attributes = ModelUtil.ConvertToAttributes(appAttributes, userAttributes)
        };
        var usersOverrides = await RemoteConfigSDK.GetPlayerSettingsOverridesAsync(req);
        if (usersOverrides.IsSuccess())
        {
            var sb = new StringBuilder();
            foreach (var entry in usersOverrides.data.Settings)
            {
                sb.AppendLine($"Overrides Remote Config: {entry.Key} - {entry.Value} ,type: {entry.Value.Type}");
            }
            Logger.Log(sb);
            RemoteConfigs = usersOverrides.data.Settings.ToDictionary(x => x.Key, x => x.Value);
        }
        return usersOverrides;
    }

    public static int GetInt(string key)
    {
        if (RemoteConfigs.TryGetValue(key, out var value) && value.Type == ConfigType.Int)
        {
            return int.Parse(value.Value);

        }
        return 0;
    }
    
    public static T GetJson<T>(string key) where T : new()
    {
        if (RemoteConfigs.TryGetValue(key, out var value) && value.Type == ConfigType.Json)
        {
            return JsonUtility.FromJson<T>(value.Value);
        }
        return new T();
    }
    
    
    public static T GetJsonSO<T>(string key) where T : ScriptableObject
    {
        if (RemoteConfigs.TryGetValue(key, out var value) && value.Type == ConfigType.Json)
        {
            T data = ScriptableObject.CreateInstance<T>(); // 先创建实例
            JsonUtility.FromJsonOverwrite(value.Value, data); // 覆盖字段
            return data;
        }

        return null;
    }
    
    public static string GetString(string key)
    {
        if (RemoteConfigs.TryGetValue(key, out var value) && value.Type == ConfigType.String)
        {
            return value.Value;
        }
        return string.Empty;
    }
}

public class RemoteUserAttributes
{
    public int score;
    public string tier;
    public string personaId;
}
public class RemoteAppAttributes
{
    public string appVersion;
}
