using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TwentyFour.Scripts.Accomplishment;
using TwentyFour.Scripts.Common;
using TwentyFour.Scripts.Metrics;
using TwentyFour.Scripts.Vitality;
using Unity.UOS.TwentyFour.Common;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.Networking;
using Logger = Unity.UOS.TwentyFour.Common.Logger;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#endif

public class VitalityHelper : SingletonBehaviour<VitalityHelper>
{
    public static int MatchCost;
    public int intervalSeconds;
    public int CurrentVitality;
    public int MaxVitality;
    
    public Action OnVitalityUpdated;
    public int lastUpdated;
    CoroutineBridge coroutineBridge;
    public WaitForSecondsRealtime OneSeconds = new WaitForSecondsRealtime(1f);

    public Action OnVitalityCheck;
    public int NextTimeRemain;
    public static bool Inited = false;
    
    private void OnEnable()
    {
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        WX.OnShow(CheckVITData);
#endif
    }

    private void OnDisable()
    {
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        WX.OffShow(CheckVITData);
#endif
    }
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
    void CheckVITData(WeChatWASM.OnShowListenerResult result)
    {
        //UIManager.Instance.ShowCommonLoading("正在检查数据");
        StartCoroutine(GetVitalityData(Identity.persona.PersonaID,true));
    }
#endif
    public void Init()
    {
        Inited = true;
        var vit = RemoteConfigHelper.GetJson<VitalityConfig>(nameof(VitalityConfig));
        if (vit != null)
        {
            //vit.interval = 10;
            intervalSeconds = vit.interval;
            MatchCost = vit.matchCost;
        }
        else
        {
            intervalSeconds = 300;
            MatchCost = 5;
        }
    }

    public void ConsumeVitality(string personaId, uint count)
    {
        Reset();
        coroutineBridge.StartCoroutine(ConsumeVitalityIEnumerator(personaId, count));
    }
    public IEnumerator ConsumeVitalityIEnumerator(string personaId, uint count)
    {
        var url =
            $"{UosAppConfigs.GetBaseStatelessUrl()}vitality/consume?uniqueId={personaId}&consumeQuantity={count}";
        //Debug.LogError(url);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Logger.LogError("Error: " + webRequest.error);
            }
            else
            {
                string jsonString = webRequest.downloadHandler.text;
                var data = JsonConvert.DeserializeObject<VitalityResourceQuery>(jsonString);
                SetParams(data);
                StartCheck();
                MetricsHelper.TrackEvent(MetricsKeys.EVENT_CONSUME_VITALITY,new Dictionary<string, object>()
                {
                    {MetricsKeys.PARAM_VIT_MATCH_COST,count},
                    {MetricsKeys.PARAM_BATTLE_MODE,MuninnManager.Singleton.GetBattleMode()},
                    { MetricsKeys.PARAM_ROOM_ID, MuninnManager.Singleton.GetMuninnRoomView().Room.Id },
                });
            }
        }
    }
    public IEnumerator GetVitalityData(string personaId,bool checkVitality = false)
    {
        var url =
            $"{UosAppConfigs.GetBaseStatelessUrl()}vitality/query?uniqueId={personaId}";
        //Debug.LogError(url);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Logger.LogError("Error: " + webRequest.error);
            }
            else
            {
                string jsonString = webRequest.downloadHandler.text;
                var data = JsonConvert.DeserializeObject<VitalityResourceQuery>(jsonString);
                SetParams(data);
                if (checkVitality)
                    StartCheck();
            }
        }
    }

    private VitalityResourceQuery Data;
    void SetParams(VitalityResourceQuery data)
    {
        Data = data;
        //Logger.LogError(data.LastUpdateTime);
        var now = DateTime.UtcNow;
        var currentTime = TimeConverter.DateTimeToTotalSeconds(now);
        lastUpdated = (int)TimeConverter.ToTotalSeconds(data.LastUpdateTime);
        Logger.LogInfo($"VIT count:{Data.CurrentQuantity} LastUpdate{data.LastUpdateTime} lastUpdated : {lastUpdated}, currentTime : {currentTime},now{now.ToString("G")} interval : {intervalSeconds}");
        CurrentVitality = data.CurrentQuantity;
        MaxVitality = data.Resource.MaxValue;
        OnVitalityUpdated?.Invoke();
    }

    public void Reset()
    {
        if (coroutineBridge != null)
        {
            coroutineBridge.Checking = false;
            coroutineBridge.StopAllCoroutines();
            DestroyImmediate(coroutineBridge.gameObject);
        }
        coroutineBridge = new GameObject("VitalityBridge").AddComponent<CoroutineBridge>();
        coroutineBridge.gameObject.transform.SetParent(transform);
        StopAllCoroutines();
        coroutineBridge.Checking = false;
    }

    public void StartCheck(bool reset = true)
    {
        if (reset)
            Reset();
        coroutineBridge.StartCheck();
    }

    public void RefreshVITCount(int deltaTime)
    {
        var times = (int)Math.Round((double)(deltaTime / intervalSeconds));
        if(CurrentVitality + times <= MaxVitality)
            CurrentVitality = CurrentVitality + times;
        lastUpdated += times * intervalSeconds;
        OnVitalityUpdated?.Invoke();
        Logger.Log($"Vitality: {CurrentVitality} now {DateTime.UtcNow.ToString("G")}");
    }

    public void Dispose()
    {
        Inited = false;
        Reset();
        OnVitalityUpdated = null;
    }
    
    public void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        PushHelper.Disconnect();
    }
    
    
}

