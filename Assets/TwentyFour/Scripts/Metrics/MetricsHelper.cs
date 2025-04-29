using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThinkingData.Analytics;
using TwentyFour.Scripts.Metrics;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public static class MetricsHelper
{
    static string CURRENT_ENVIRONMENT_VARIABLE = string.Empty;
    static bool _isInit = false;
    public static async Task Init()
    {
        try
        {
            CURRENT_ENVIRONMENT_VARIABLE = UosAppConfigs.GetUosAppConfigs().CurrentEnv;
            TDConfig tDConfig = new TDConfig();
            tDConfig.mode = TDMode.Normal;
            tDConfig.timeZone = TDTimeZone.Asia_Shanghai;
            await TDAnalytics.Init(tDConfig);
            _isInit = true;
        }
        catch (Exception e)
        {
            Logger.LogError($"Metrics Init Error: {e}!");
            throw;
        }
        
    }

    public static void SetUser()
    {
        TDAnalytics.Login(Identity.persona.PersonaID);
        var useSet = new Dictionary<string, object>()
        {
            { MetricsKeys.USER_ID, Identity.persona.UserID },
            { MetricsKeys.REALM_ID, Identity.persona.RealmID },
            { MetricsKeys.PERSONA_ID, Identity.persona.PersonaID },
            { MetricsKeys.DISPLAY_NAME, Identity.persona.DisplayName },
            { MetricsKeys.ENVIRONMENT_VARIABLE, UosAppConfigs.GetUosAppConfigs().CurrentEnv }
        };
#if UNITY_EDITOR
        useSet[MetricsKeys.UNITY_EDITOR] = true;
#else
        useSet[MetricsKeys.UNITY_EDITOR] = false;
#endif
        TDAnalytics.UserSet(useSet);
        TDAnalytics.SetSuperProperties(useSet);
        Logger.LogInfo($"Metrics Set User Success! UserID: {Identity.persona.UserID},PersonaID: {Identity.persona.PersonaID}");
    }

    public static void TrackEvent(string eventName, Dictionary<string, object> properties = null)
    {
        if (!_isInit)
        {
            Logger.LogError($"MetricsHelper is not initialized!,Track Event [{eventName}] Failed!");
            return;
        }
        try
        {
            TDAnalytics.Track(eventName,properties,DateTime.Now, TimeZoneInfo.Local);
        }
        catch (Exception e)
        {
            Logger.LogError($"Metrics Track Event {eventName} Error: {e}!");
            throw;
        }
    }

    public static void Dispose()
    {
        _isInit = false;
    }
}
