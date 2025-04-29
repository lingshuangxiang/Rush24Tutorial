using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quest;
using TwentyFour.Scripts.Metrics;
using Unity.Passport.Runtime;
using UnityEngine;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

namespace TwentyFour.Scripts.Quest
{
    public static class QuestHelper
    {
        public static Dictionary<string,PersonaQuest> PersonaQuests = new Dictionary<string,PersonaQuest>();
        public static Dictionary<string,PersonaQuestItem> AllQuestsMap = new Dictionary<string,PersonaQuestItem>();
        public static Action OnSearchPersonaQuests;
        public static async Task<List<PersonaQuest>>  FetchPersonaQuests()
        {
            var personaQuests = await PassportFeatureSDK.Quest.FetchPersonaQuests();
            AllQuestsMap.Clear();
            foreach (var personaQuest in personaQuests)
            {
                foreach (var item in personaQuest.Items)
                {
                    AllQuestsMap[item.SlugName] = item;
                }
            }
            PersonaQuests = personaQuests.ToDictionary(x => x.SlugName, x => x);
            return personaQuests;
        }

        public static async Task<SearchPersonaQuestsResponse> SearchPersonaQuests()
        {
            var personaQuests = await PassportFeatureSDK.Quest.SearchPersonaQuests();
            AllQuestsMap.Clear();
            foreach (var personaQuest in personaQuests.Quests)
            {
                foreach (var item in personaQuest.Items)
                {
                    AllQuestsMap[item.SlugName] = item;
                }
            }
            PersonaQuests = personaQuests.Quests.ToDictionary(x => x.SlugName, x => x);
            OnSearchPersonaQuests?.Invoke();
            return personaQuests;
        }

        public static async Task<UpdatePersonaQuestItemResponseSDK> UpdatePersonaQuestItem(string slugName,QuestItemUpdateAction updateAction, uint value)
        {
            if (AllQuestsMap.TryGetValue(slugName, out var questItem))
            {
                if (questItem.Completed)
                {
                    Logger.Log($"{questItem.DisplayName}已经完成，不再更新");
                    return null;
                }
            }
            MetricsHelper.TrackEvent(MetricsKeys.EVENT_DAILY_QUEST_DONE,new Dictionary<string, object>()
            {
                {MetricsKeys.PARAM_QUEST_SLUG,slugName}
            });
            var updatedPersonaQuestItem = await PassportFeatureSDK.Quest.UpdatePersonaQuestItem(slugName, updateAction, value);
            return updatedPersonaQuestItem;
        }

        public static async Task<RedeemQuestItemRewardsResponse> RedeemQuestItemRewards(string questItemSlug,Action<Exception> failedCallback)
        {
            try
            {
                var redeem = await PassportFeatureSDK.Quest.RedeemQuestItemRewards(slugName: questItemSlug);
                await SearchPersonaQuests();
                return redeem;  
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                failedCallback?.Invoke(e);
                throw;
            }

        }
    }
}