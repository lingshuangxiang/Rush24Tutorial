using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TwentyFour.Scripts.Category;
using TwentyFour.Scripts.Quest;
using TwentyFour.Scripts.RemoteConfig;
using Unity.Collections.LowLevel.Unsafe;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.Networking;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

namespace TwentyFour.Scripts.Tournament
{
    public static class TournamentDataHelper
    {
        public const string LeaderboardSlug_Main = "TournamentLeaderboardSlug_Main";
        public const string LeaderboardSlug_PerfectWin = "TournamentLeaderboardSlug_PerfectWin";
        public const string LeaderboardSlug_FastSolveSingle = "TournamentLeaderboardSlug_FastSolveSingle";
        public const string LeaderboardSlug_FastestAverageTime = "TournamentLeaderboardSlug_FastestAverageTime";
        private const string URL = "https://stateless.unity.cn/release/CONFIG/tournament";

        public static List<TournamentData> TournamentDatas = new List<TournamentData>();
        public static SelfTournamentScoreData TournamentScoreData;
        public static TournamentData Add(string json)
        {
            TournamentData data = ScriptableObject.CreateInstance<TournamentData>(); // 先创建实例
            JsonUtility.FromJsonOverwrite(json, data); // 覆盖字段
            TournamentDatas?.Add(data);
            return data;
        }
        public static void Dispose()
        {
            TournamentData.Current = null;
            TournamentScoreData = null;
            TournamentDatas?.Clear();
        }

        public static IEnumerator FetchTournamentData()
        {
            if (TournamentData.Current)
            {
                var leaderboardCount = RemoteConfigHelper.GetInt(RemoteConfigKeys.DefaultRankCount);
                var count = leaderboardCount == 0 ? 20 : leaderboardCount;
                var tournamentLeaderBoard = TournamentData.Current.GetLeaderboardData();
                if (tournamentLeaderBoard != null)
                {
                    foreach (var leaderboard in tournamentLeaderBoard)
                    {
                        var awaiter = TiersHelper.ListLeaderBoard(leaderboard.Value, count).GetAwaiter();
                        yield return new WaitUntil(() => awaiter.IsCompleted);
                    }
                }
                CategoryHelper.Init();
                var categories = CategoryHelper.ListCategories();
                yield return new WaitUntil(()=>categories.IsCompleted);
                var defalut = CategoryHelper.ListProducts(CategoryHelper.DefaultGOLDCategory);
                yield return new WaitUntil(()=>defalut.IsCompleted);
                foreach (var data in TournamentDataHelper.TournamentDatas)
                {
                    var category = CategoryHelper.ListProducts(data.GetStoreData());
                    yield return new WaitUntil(()=>category.IsCompleted);
                }
                yield return TournamentDataHelper.GetSelfTournamentScoreData(TournamentData.Current.SlugName, Identity.persona.PersonaID);
                var questAwaiter = QuestHelper.SearchPersonaQuests().GetAwaiter();
                yield return new WaitUntil(() => questAwaiter.IsCompleted);
                yield return WXSubscribe.SendGETRequest(TournamentData.Current.SlugName);

            }
        }
        
        public static IEnumerator GetSelfTournamentScoreData(string tournamentSlug, string personaId)
        {
            var url =
                $"{UosAppConfigs.GetBaseStatelessUrl()}tournament?tournamentSlugName={tournamentSlug}&uniqueId={personaId}";
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
                    var data = JsonConvert.DeserializeObject<List<SelfTournamentScoreData>>(jsonString);
                    TournamentScoreData = data[0];
                }
            }
        }
    }

    [Serializable]
    public class SelfTournamentScoreData
    {
        public string winRate;
        public string winCount;
        public string completeWinCount;
        public string avgTime;
        public string totalCount;
    }
}