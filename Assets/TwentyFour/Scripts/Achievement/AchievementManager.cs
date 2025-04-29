using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Achievement;
using Unity.Passport.Runtime;
using Unity.UOS.TwentyFour;
using Unity.UOS.TwentyFour.Common;
using UnityEngine;
using Action = Achievement.Action;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class AchievementManager : Singleton<AchievementManager>
{

    public static List<AchievementInfoExpanded> LocalAchievements = new List<AchievementInfoExpanded>();
    public static async Task<FetchAchievementResponse> FetchAchievement()
    {
        var achievements = await PassportFeatureSDK.Achievement.FetchAchievement();
        LocalAchievements = achievements.Achievements.ToList();
        var sb = new StringBuilder();
        foreach (var achievement in LocalAchievements)
        {
            sb.AppendLine($" {achievement.SlugName} {achievement.DisplayName} {achievement.AchievedValue} / {achievement.ThresholdValue}");
        }
        Logger.Log(sb);
        return achievements;
    }

    public static async Task<ListPersonaAchievementsResponse> ListPersonaAchievements()
    {
        try
        {
            var achievements = await PassportFeatureSDK.Achievement.ListPersonaAchievements();
            LocalAchievements = achievements.Achievements.ToList();
            var sb = new StringBuilder();
            foreach (var achievement in LocalAchievements)
            {
                sb.AppendLine($" {achievement.SlugName} {achievement.DisplayName} {achievement.AchievedValue} / {achievement.ThresholdValue}");
            }
            Logger.Log(sb);
            return achievements;
        }
        catch (Exception e)
        {
            Logger.LogError(e.Message);
            throw;
        }
        
    }
    public static async Task<AchievementInfo> UpdatePersonaAchievement(string slugName, Action action, uint value,
        Dictionary<string, string> customData = null,System.Action failedAction = null)
    {
        Logger.Log($"UpdatePersonaAchievement {slugName} {action} {value}");
        try
        {
            var update = await PassportFeatureSDK.Achievement.UpdatePersonaAchievement(slugName, action, value, customData);
            return update;
        }
        catch (Exception e)
        {
            Logger.LogError(e);
            failedAction?.Invoke();
            throw;
        }


    }

    public static async Task RedeemAchievementRewards(string slugName,System.Action<Exception> failedAction = null)
    {
        try
        {
            await PassportFeatureSDK.Achievement.RedeemAchievementRewards(slugName);
        }
        catch (Exception e)
        {
            failedAction?.Invoke(e);
            Logger.LogError($"{e.Message} {e.Data}");
        }
        
    }
    public static async Task InitAchievements()
    {
        await StageManager.UpLoadStageAchievementAsync();
    }

    public static void Dispose()
    {
        LocalAchievements?.Clear();
    }
}
