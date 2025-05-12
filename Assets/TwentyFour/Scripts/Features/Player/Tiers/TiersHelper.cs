using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leaderboard;
using Unity.Passport.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Logger = Unity.UOS.TwentyFour.Common.Logger;


public static class TiersHelper
{
     public const int TopTierScore = 48;
     public const int StarCountPerTier = 4;
     public const string TiersLeaderboardSlugName = "SEASON202504Leaderboard";
     public const string TiersLeaderboardRegion = "shanghai"; 

     public static UnityEvent UpdatePassSuccess = new UnityEvent();
     public static UnityEvent UpdatePassFail = new UnityEvent();
     public static UnityEvent<Leaderboard.ListLeaderboardScoresResponse> GetResponse = new UnityEvent<Leaderboard.ListLeaderboardScoresResponse>();
     public static UnityEvent<Leaderboard.GetMemberScoreResponse> GetMineReponse = new UnityEvent<Leaderboard.GetMemberScoreResponse>();
          
     public static ListLeaderboardScoresResponse LocalLeaderboardScoresResponse;
     
     public static Dictionary<string,ListLeaderboardScoresResponse> AllLeaderboardScoresResponse = new Dictionary<string, ListLeaderboardScoresResponse>();
     public static Dictionary<string,GetLeaderboardResponse> LeaderboardInfos = new Dictionary<string, GetLeaderboardResponse>();
     public static Dictionary<string,GetMemberScoreResponse> SelfLeaderboardScoresResponse = new Dictionary<string, GetMemberScoreResponse>();
     
     public static async Task UpdatePlayerScore()
     {
          UpdatePassSuccess?.Invoke();
     }

     /// <summary>
     /// 获取排行榜最前面的名次
     /// </summary>
     /// <param name="count"></param>
     public static async Task<Leaderboard.ListLeaderboardScoresResponse> ListTierLeaderBoard(int count = 20)
     {
          var resp = await ListLeaderBoard(TiersLeaderboardSlugName, count);
          LocalLeaderboardScoresResponse = resp;
          
          return resp;
     }

     /// <summary>
     ///  获取排行榜最前面的名次
     /// </summary>
     /// <param name="leaderboardSlugName"></param>
     /// <param name="count"></param>
     public static async Task<Leaderboard.ListLeaderboardScoresResponse> ListLeaderBoard(string leaderboardSlugName,
          int count)
     {
          Leaderboard.ListLeaderboardScoresResponse resp =
               await PassportFeatureSDK.Leaderboard.ListLeaderboardScores(leaderboardSlugName, null, null, 0,
                    (uint)count);
          var leaderboardInfo = await PassportFeatureSDK.Leaderboard.GetLeaderboard(leaderboardSlugName);
          var selfLeaderboardScoresResponse = await GetMyLeaderboardScore(leaderboardSlugName);
          LeaderboardInfos[leaderboardSlugName] = leaderboardInfo;
          var sb = new StringBuilder();
          sb.AppendLine($"Leaderboard Scores: {leaderboardSlugName}");
          foreach (Leaderboard.LeaderboardMemberScore score in resp.Scores)
          { 
               sb.AppendLine($"{score.DisplayName}: {score.Score},bucketId: {score.BucketId}");
          }
          Logger.Log(sb);
          SelfLeaderboardScoresResponse[leaderboardSlugName] = selfLeaderboardScoresResponse;
          AllLeaderboardScoresResponse[leaderboardSlugName] = resp;
          GetResponse?.Invoke(resp);
          return resp;
     }

     /// <summary>
     /// 获取自己的排名
     /// </summary>
     /// <param name="leaderboardSlugName"></param>
     /// <param name="range"></param>
     public static async Task<Leaderboard.GetMemberScoreResponse> GetMyLeaderboardScore(string leaderboardSlugName,
          int range = 0)
     {
          // var range = 0;
          Leaderboard.GetMemberScoreResponse scoreList =
               await PassportFeatureSDK.Leaderboard.GetScore(leaderboardSlugName, (uint)range);
          // 若是需要打印出自己的成绩
          if (scoreList.Scores.Any())
          {
               var score = scoreList.Scores[range];
               Logger.Log(
                    $"[TiersHelper] DisplayName: {score.DisplayName},PersonaProperties: {score.PersonaProperties},Rank: {score.Rank},Score: {score.Score},Tier: {score.Tier}");
          }
          if(leaderboardSlugName == TiersLeaderboardSlugName)
               GetMineReponse?.Invoke(scoreList);
          return scoreList;
     }

     public static TierUserScoreData LocalTierUserScoreData;
     public static IEnumerator GetTierUserScoreData(string personaId,Action failedCallback = null)
     {
          var url =
               $"{UosAppConfigs.GetBaseStatelessUrl()}player_info?uniqueId={personaId}";
          //Debug.LogError(url);
          using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
          {
               yield return webRequest.SendWebRequest();

               if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
               {
                    Logger.LogError("Error: " + webRequest.error);
                    failedCallback?.Invoke();
               }
               else
               {
                    string jsonString = webRequest.downloadHandler.text;
                    var data = JsonUtility.FromJson<TierUserScoreData>(jsonString);
                    LocalTierUserScoreData = data;
          
               }
          }
     }
     public static void Dispose()
     {
          LocalLeaderboardScoresResponse = null;
          AllLeaderboardScoresResponse?.Clear();
          SelfLeaderboardScoresResponse?.Clear();
          LocalTierUserScoreData = null;
          LeaderboardInfos?.Clear();
     }
     public static int GetTier(string tierName)
     {
          var number = 0;
          if (string.IsNullOrEmpty(tierName) || tierName?.Length < 2)
          {
               return number;
          }
          var level = tierName.Substring(0, 2);
          var subLevel = tierName.Substring(2);

          var levelDiff = TiersBadge.SubLevelCount * TiersBadge.ScoreDiffBetweenSubLevel;
          
          switch (level)
          {
               case "石头":
                    number = 0;
                    break;
               case "青铜":
                    number = levelDiff * 1;
                    break;
               case "白银":
                    number = levelDiff * 2;
                    break;
               case "黄金":
                    number = levelDiff * 3;
                    break;
               case "钻石":
                    number = levelDiff * 4;
                    break;
          }

          switch (subLevel)
          {
               case "I":
                    number += 0;
                    break;
               case "II":
                    number += TiersBadge.ScoreDiffBetweenSubLevel * 1;
                    break;
               case "III":
                    number += TiersBadge.ScoreDiffBetweenSubLevel * 2;
                    break;
          }

          return number;
     }
}
[Serializable]
public class TierUserScoreData {
     public int highestScore;
     public string highestTier;
     public float totalTime;
     public int totalResolved;
     
}
