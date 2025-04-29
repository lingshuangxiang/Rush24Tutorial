using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TwentyFour.Scripts.Common;
using UnityEditor;
using UnityEngine;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

namespace TwentyFour.Scripts.Tournament
{
    [Serializable]
    [CreateAssetMenu(fileName = "TournamentData", menuName = "ScriptableObjects/TournamentData", order = 3)]
    public class TournamentData : ScriptableObject
    {
        public static TournamentData Current;
        public string Name;
        public string Description;
        public string DisplayName;
        public string SlugName;
        public string StartDate;
        public string EndDate;
        public string ExpireTime;
        public List<TournamentTimeData> TimeData;
        //public string DailyActiveEndTime;
        public List<TournamentReferenceData> ReferenceSlugs = new List<TournamentReferenceData>();
        public string MatchMakingConfig;

        public bool InActiveDate()
        {
            bool enable = false;
            var StartDateTime = TimeConverter.ToTotalSeconds(StartDate);
            var EndDateTime = TimeConverter.ToTotalSeconds(EndDate);
            var now = DateTime.UtcNow;
            var currentTime = TimeConverter.DateTimeToTotalSeconds(now);
            if (currentTime >= StartDateTime && currentTime < EndDateTime)
            {
                enable = true;
            }

            return enable;
        }

        public Dictionary<string, string> GetLeaderboardData()
        {
            foreach (var reference in ReferenceSlugs)
            {
                if (reference.SlugType == TournamentDataReferenceSlugType.Leaderboards)
                {
                    return reference.Datas.ToDictionary();
                }
            }
            return null;
        }
        
        public string GetStoreData()
        {
            foreach (var reference in ReferenceSlugs)
            {
                if (reference.SlugType == TournamentDataReferenceSlugType.Categories)
                {
                    return reference.Datas.ToDictionary()["main"];
                }
            }
            return null;
        }

        public bool IsExpired()
        {
            if(string.IsNullOrEmpty(ExpireTime))
                return false;
            
            var ExpiredDateTime = TimeConverter.ToTotalSeconds(ExpireTime);
            var now = DateTime.UtcNow;
            var currentTime = TimeConverter.DateTimeToTotalSeconds(now);
            if (currentTime > ExpiredDateTime)
            {
                return true;
            }
            return false;
        }
        public bool IsActive()
        {
            bool enable = false;
            foreach (var timeData in TimeData)
            {
                
                var StartDateTime = TimeConverter.ToTotalSeconds(StartDate);
                var EndDateTime = TimeConverter.ToTotalSeconds(EndDate);
                var now = DateTime.UtcNow;
                var currentTime = TimeConverter.DateTimeToTotalSeconds(now);
                if (currentTime >= StartDateTime && currentTime < EndDateTime)
                {
                    //在活动日期中
                    if (timeData.DurationTime == "24:00:00")
                    {
                        return true;
                    }
                    var todayStartTime = TimeConverter.ConvertStringToDateTime_Hms(timeData.DailyActiveStartTime);
                    if (timeData.IsSpanDay)
                    {
                        todayStartTime = todayStartTime.AddDays(-1);
                    }
                    var todayStartSeconds = TimeConverter.DateTimeToTotalSeconds(todayStartTime);
                    var todayEndTime =
                        todayStartTime.Add(TimeConverter.ConvertStringToTimeSpan_Hms(timeData.DurationTime));
                    var todayEndSeconds = TimeConverter.DateTimeToTotalSeconds(todayEndTime);
                    //Debug.LogError($"ISACTIVE Start:{todayStartTime.ToString("s")}    End:{todayEndTime.ToString("s")}");

                    if ( currentTime >= todayStartSeconds && currentTime < todayEndSeconds)
                    {
                        enable = true;
                    }
                }
                
            }
            return enable;
        }

        public string GetActiveTimeString()
        {
            var sb = new StringBuilder();
            foreach (var timeData in TimeData)
            {
                var (todayStartTime, todayEndTime) = GetStartAndEndTime(timeData);
                sb.AppendLine($"{todayStartTime.ToLocalTime().ToString("HH:mm",new CultureInfo("zh-CN"))} ~ {todayEndTime.ToLocalTime().ToString("HH:mm",new CultureInfo("zh-CN"))}");
            }
            return sb.ToString();
        }

        public (DateTime, DateTime) GetStartAndEndTime(TournamentTimeData timeData)
        {
            var todayStartTime = TimeConverter.ConvertStringToDateTime_Hms(timeData.DailyActiveStartTime);
            if (timeData.IsSpanDay)
            {
                todayStartTime = todayStartTime.AddDays(-1);
            }

            DateTime todayEndTime;
            if (timeData.DurationTime == "24:00:00")
            {
                todayEndTime = todayStartTime.AddDays(1);
            }
            else
            {
                todayEndTime =
                    todayStartTime.Add(TimeConverter.ConvertStringToTimeSpan_Hms(timeData.DurationTime));
            }
            
            return (todayStartTime, todayEndTime);
        }

        public (DateTime,DateTime) GetNearByStartDateTime()
        {
            DateTime nearStart = DateTime.MinValue;
            DateTime nearEnd = DateTime.MinValue;
            double temp = Double.MaxValue;
            var StartDateTime = TimeConverter.ToTotalSeconds(StartDate);
            var now = DateTime.UtcNow;
            var currentTime = TimeConverter.DateTimeToTotalSeconds(now);
            if (currentTime > StartDateTime) //活动已开始
            {
                foreach (var timeData in TimeData)
                {
                    var (todayStartTime, todayEndTime) = GetStartAndEndTime(timeData);
                    var startSeconds = TimeConverter.DateTimeToTotalSeconds(todayStartTime);
                    var endSeconds = TimeConverter.DateTimeToTotalSeconds(todayEndTime);

                    if (currentTime < startSeconds)
                    {
                        if (startSeconds - currentTime < temp)
                        {
                            temp = startSeconds - currentTime;
                            nearStart = todayStartTime;
                            nearEnd = todayEndTime;
                        }
                    }
                }
            }

            //判断是否超过当天最后一场
            if (nearStart == DateTime.MinValue || nearEnd == DateTime.MinValue)
            {
                //Debug.LogError("活动已开始");
                var timeData = TimeData[0];
                var (todayFirstStartTime, todayFirstEndTime) = GetStartAndEndTime(timeData);

                //活动开始时间大于当前时间,即活动未开始
                if (TimeConverter.DateTimeToTotalSeconds(todayFirstStartTime) <
                    TimeConverter.ToTotalSeconds(StartDate))
                {
                    string format = "yyyy-MM-dd'T'HH:mm:ss'Z'";
                    if (DateTime.TryParseExact(
                            StartDate,
                            format,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                            out var dateTime))
                    {
                        
                        if(dateTime.Kind == DateTimeKind.Local)
                            dateTime = dateTime.ToUniversalTime();
                        var start = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, todayFirstStartTime.Hour,
                            todayFirstStartTime.Minute, todayFirstStartTime.Second, DateTimeKind.Utc);
                        DateTime end;

                        if (timeData.DurationTime == "24:00:00")
                        {
                            end = start.AddDays(1);
                        }
                        else
                        {
                            end = start.Add(TimeConverter.ConvertStringToTimeSpan_Hms(timeData.DurationTime));

                        }
                        nearStart = start;
                        nearEnd = end;
                    }
                    
                }
                else
                {
                    //是否可以跨天下一场
                    var tempStart = todayFirstStartTime.AddDays(1);
                    var EndDateTime = TimeConverter.ToTotalSeconds(EndDate);
                    if (TimeConverter.DateTimeToTotalSeconds(tempStart) < EndDateTime)
                    {
                        nearEnd = todayFirstEndTime.AddDays(1);
                        nearStart = todayFirstStartTime.AddDays(1);
                    }
                }
                
            }
            Logger.LogInfo( $"Start:{nearStart.ToString("s")}    End:{nearEnd.ToString("s")}");
            return (nearStart,nearEnd);
        }
        public string GetNearbyTimeString()
        {
            foreach (var timeData in TimeData)
            {
                
                var StartDateTime = TimeConverter.ToTotalSeconds(StartDate);
                var EndDateTime = TimeConverter.ToTotalSeconds(EndDate);
                var now = DateTime.UtcNow;
                var currentTime = TimeConverter.DateTimeToTotalSeconds(now);
                if (currentTime >= StartDateTime && currentTime < EndDateTime)
                {
                    //在活动日期中
                    if (timeData.DurationTime == "24:00:00")
                    {
                        return "全天";
                    }
                    var todayStartTime = TimeConverter.ConvertStringToDateTime_Hms(timeData.DailyActiveStartTime);
                    if (timeData.IsSpanDay)
                    {
                        todayStartTime = todayStartTime.AddDays(-1);
                    }
                    var todayStartSeconds = TimeConverter.DateTimeToTotalSeconds(todayStartTime);
                    var todayEndTime =
                        todayStartTime.Add(TimeConverter.ConvertStringToTimeSpan_Hms(timeData.DurationTime));
                    var todayEndSeconds = TimeConverter.DateTimeToTotalSeconds(todayEndTime);
                    if ( currentTime >= todayStartSeconds && currentTime < todayEndSeconds)
                    {
                        return $"{todayStartTime.ToLocalTime().ToString("HH:mm",new CultureInfo("zh-CN"))} ~ {todayEndTime.ToLocalTime().ToString("HH:mm",new CultureInfo("zh-CN"))}";
                    }
                }
                
            }
            return string.Empty;
        }
        public static void GenJson()
        {
            var data = Resources.Load<TournamentData>("TournamentData");
            string json = JsonUtility.ToJson(data);
            GUIUtility.systemCopyBuffer = json;
            Debug.Log($"已复制至剪切板：{json}");
            // var seconds = TimeConverter.ToTotalSeconds(data.StartDate);
            // Debug.Log(seconds);
            // var date = TimeConverter.ConvertSecondsToDateTime(seconds);
            // Debug.Log(date);
            //
            // var todatStart = TimeConverter.ConvertStringToDateTime_Hms(data.DailyActiveStartTime);
            // Debug.Log(TimeConverter.DateTimeToTotalSeconds(todatStart));
        }
    }


}