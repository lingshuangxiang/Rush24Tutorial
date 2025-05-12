using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Passport;
using Unity.Passport.Runtime;
using Unity.UOS.TwentyFour.UOSGateway;
using TwentyFour.Scripts.PersonaProperty;
using Leaderboard;
using Unity.Passport.Runtime.Network;
using Unity.UOS.TwentyFour;
using UnityEngine;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public static class PersonaPropertiesHelper
{
    
    static Dictionary<string, string> localPersonaPropertyDictionary = new Dictionary<string, string>();
    public static LeaderboardScore MyLeaderboardScore = null;

    public static Action<Persona> OnPersonaUpdatedAction;
    public static bool ShowStageTutorial = false;
    public static bool ShowBattleTutorial = false;
    
    public static void Init()
    {
        TiersHelper.GetMineReponse.AddListener(OnGetMyLeaderboardScore);
        //是否完成闯关教程
        ShowStageTutorial = GetProperty(PersonaPropertyKeys.ShowStageTutorialKey, true);
        //是否完成对战教程
        ShowBattleTutorial = GetProperty(PersonaPropertyKeys.ShowBattleTutorialKey, true);
    }

    public static Dictionary<string, string> GetLocalProperties()
    {
        return localPersonaPropertyDictionary.ToDictionary(entry => entry.Key, entry => entry.Value);
    }
    
    public static async Task<Persona> GetPersona()
    {
        string id = await Identity.GetRealmID();
        
        return await PassportSDK.Identity.GetPersonaByRealm(id);
    }
    
    public static async Task<Persona> UpdatePersona(string displayName, string iconUrl = "",
        Dictionary<string, string> properties = null, List<string> waitToRemove = null,
        Action<Persona> callback = null)
    {
        var persona = await PassportSDK.Identity.UpdatePersona(displayName, iconUrl, properties, waitToRemove);
        Identity.persona = persona;
        localPersonaPropertyDictionary?.Clear();
        localPersonaPropertyDictionary = persona.Properties.ToDictionary(x => x.Key, x => x.Value);
        callback?.Invoke(persona);
        OnPersonaUpdatedAction?.Invoke(persona);
        return persona;
    }
    
    public static async Task<Persona> GetCurrentPersona()
    {
        var persona = await PassportClient.GetCurrentPersona();
        return persona.Persona;
    }

    public static async Task<Dictionary<string, string>> GetPersonaProperties()
    {
        Persona persona = await GetPersona();
#if UNITY_EDITOR
        var sb = new StringBuilder($"[PersonaPropertiesHelper] GetPersonaProperties,Count: {persona.Properties.Count}");
        foreach (var property in persona.Properties)
            sb.Append($"\n{property.Key}:{property.Value}");
        Logger.Log(sb);
#endif
        localPersonaPropertyDictionary?.Clear();
        localPersonaPropertyDictionary = persona.Properties.ToDictionary(x => x.Key, x => x.Value);
        
        return localPersonaPropertyDictionary;
    }

    public static async Task SetPersonaProperties(Dictionary<string, string> personaProperties,
        List<string> waitToRemove = null, Action failedcallback = null)
    {
        try
        {
            Persona persona = await GetPersona();
#if UNITY_EDITOR
            var sb = new StringBuilder($"[PersonaPropertiesHelper] SetPersonaProperties,Count: {persona.Properties.Count}");
            foreach (var property in persona.Properties)
                sb.Append($"\n{property.Key}:{property.Value}");
            Logger.Log(sb);
#endif
            await UpdatePersona(persona.DisplayName, persona.IconUrl, personaProperties, waitToRemove);
        }
        catch (Exception e)
        {
            Logger.LogError(e);
            failedcallback?.Invoke();
            throw;
        }

    }
    
    public static string GetProperty(string propertyName)
    {
        if (localPersonaPropertyDictionary.TryGetValue(propertyName, out var value))
        {
            return value;
        }
        return string.Empty;
    }
    
    public static bool GetProperty(string propertyName, bool defaultValue = false)
    {
        if (localPersonaPropertyDictionary.TryGetValue(propertyName, out var value))
        {
            return bool.Parse(value);
        }

        return defaultValue;
    }
    
    public static int GetProperty(string propertyName, int defaultValue = 0)
    {
        if (localPersonaPropertyDictionary.TryGetValue(propertyName, out var value))
        {
            return int.Parse(value);
        }
            
        return defaultValue;
    }
    
    public static float GetProperty(string propertyName, float defaultValue = 0f)
    {
        if (localPersonaPropertyDictionary.TryGetValue(propertyName, out var value))
        {
            return float.Parse(value);
        }
            
        return defaultValue;
    }
    
    /// <summary>
    /// 拉取个人分数时更新PersonaProperty
    /// </summary>
    /// <param name="scoreList"></param>
    static void OnGetMyLeaderboardScore(GetMemberScoreResponse scoreList)
    {
        if (scoreList.Scores.Any())
        {
            SetScoreAndTierAsync(scoreList.Scores[0]);
        }
        else
        {
            LeaderboardScore score = new LeaderboardScore();
            score.Score = 0;
            score.Tier = "石头";
            SetScoreAndTierAsync(score);
        }
    }

    public async static Task SetScoreAndTierAsync(LeaderboardScore score)
    {
        MyLeaderboardScore = score;
        Dictionary<string, string> data = new Dictionary<string, string>();
        
        // 拉取我的排行榜分数时更新最高分
        var (highestScore,_) = GetBattleHighestScoreAndTier();
        if (MyLeaderboardScore.Score > highestScore)//更新最高分
        {
            data.Add(PersonaPropertyKeys.BattleHighestScoreKey, MyLeaderboardScore.Score.ToString());
            data.Add(PersonaPropertyKeys.BattleHighestTierKey, MyLeaderboardScore.Tier);
        }
        //存一份Tier & Score 至 PersonaProperty
        data.Add(PersonaPropertyKeys.BattleCurrentScoreKey, MyLeaderboardScore.Score.ToString());
        data.Add(PersonaPropertyKeys.BattleCurrentTierKey, MyLeaderboardScore.Tier);
        
        await SetPersonaProperties(data);
    }
    
    public static (int,string) GetBattleHighestScoreAndTier()
    {
        if (localPersonaPropertyDictionary.TryGetValue(PersonaPropertyKeys.BattleHighestScoreKey, out var score))
        {
            if(localPersonaPropertyDictionary.TryGetValue(PersonaPropertyKeys.BattleHighestTierKey, out var tier))
                return (int.Parse(score), tier);
        }
        return (0, "石头");
    }
    
    public static void Dispose()
    {
        MyLeaderboardScore = null;
        TiersHelper.GetMineReponse.RemoveListener(OnGetMyLeaderboardScore);
        localPersonaPropertyDictionary?.Clear();
        ShowStageTutorial = false;
        ShowBattleTutorial = false;
    }
}
