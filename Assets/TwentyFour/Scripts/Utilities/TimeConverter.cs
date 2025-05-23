using System;
using System.Globalization;
using UnityEngine;

namespace TwentyFour.Scripts.Utilities
{
    public static class TimeConverter
    {
        public static double ToTotalSeconds(string time)
        {
            string format = "yyyy-MM-dd'T'HH:mm:ss'Z'";
            if (DateTime.TryParseExact(
                    time,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dateTime))
            {
                return DateTimeToTotalSeconds(dateTime);
            }
            else
            {
                Debug.LogError("格式解析失败");
            }

            return double.NaN;

        }
        public static double ToTotalSecondsSS(string time)
        {
            string format = "yyyy-MM-dd'T'HH:mm:ss.fffK";
            if (DateTime.TryParseExact(
                    time,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dateTime))
            {
                return DateTimeToTotalSeconds(dateTime);
            }
            else
            {
                Debug.LogError("格式解析失败");
            }

            return double.NaN;

        }
        public static DateTime ToUTCDate(string time)
        {
            string format = "yyyy-MM-dd'T'HH:mm:ss'Z'";
            if (DateTime.TryParseExact(
                    time,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dateTime))
            {
                return dateTime.ToUniversalTime();
            }
            else
            {
                Debug.LogError("格式解析失败");
            }

            return DateTime.MinValue;

        }
        public static double DateTimeToTotalSeconds(DateTime dateTime)
        {
            //Debug.LogError($"kind ; {dateTime.Kind}");
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            if(dateTime.Kind == DateTimeKind.Local)
                dateTime = dateTime.ToUniversalTime();
            TimeSpan timeSpan = dateTime - epoch;
            //Debug.Log("UTC Time: " + dateTime.ToString("o")); // 输出ISO 8601格式
            //Debug.Log("Local Time: " + dateTime.ToLocalTime());
            //Debug.Log("Seconds: " + timeSpan.TotalSeconds);
            return timeSpan.TotalSeconds;
        }
        public static DateTime ConvertSecondsToDateTime(double totalSeconds)
        {
            // 1. 定义基准时间（1970-01-01 UTC）
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
            // 2. 将总秒数转换为TimeSpan
            TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
        
            // 3. 基准时间 + 时间间隔 = 目标时间（UTC）
            DateTime targetTime = epoch.Add(timeSpan);
        
            // 4. 转换为本地时间（可选）
            return targetTime.ToLocalTime();
        }

        /// <summary>
        /// 将HH:mm:ss字符串转换为DateTime类型
        /// </summary>
        /// <param name="timeString"></param>
        /// <returns></returns>
        public static DateTime ConvertStringToDateTime_Hms(string timeString)
        {
            DateTime dateTime = DateTime.ParseExact(timeString, "HH:mm:ss'Z'", null);
        
            var utcDate = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Utc);
            // // 获取当前日期
            // DateTime currentDate = DateTime.Today.ToUniversalTime();
            //
            // // 将时间与当前日期合并
            // DateTime result = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 
            //     dateTime.Hour, dateTime.Minute, dateTime.Second,currentDate.Kind);

            //Debug.Log(result);
            return utcDate;
        }

        public static TimeSpan ConvertStringToTimeSpan_Hms(string timeString)
        {
            // 方法2：精确格式解析（推荐）
            TimeSpan time = TimeSpan.ParseExact(
                timeString,
                "hh\\:mm\\:ss",
                CultureInfo.InvariantCulture
            );
            return time;
        }
        public static string GetTimeAgoOrAfter(long timestamp ,bool showEndWord = true)
        {
            // 将Unix时间戳转换为UTC时间，再转为本地时间
            DateTimeOffset utcTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            DateTimeOffset localTime = utcTime.ToLocalTime();
    
            // 获取当前本地时间
            DateTimeOffset now = DateTimeOffset.Now;
    
            // 计算时间差
            TimeSpan difference = now - localTime;
            bool isFuture = difference < TimeSpan.Zero;
            TimeSpan absDifference = difference.Duration();
    
            // 分解时间差
            int days = absDifference.Days;
            int hours = absDifference.Hours;
            int minutes = absDifference.Minutes;
            int seconds = absDifference.Seconds;
    
            string endWord = showEndWord ? (isFuture ? "后" : "前") : "";
            // 格式化输出
            if (days > 0)
            {
                return $"{days}天{endWord}";
            }
            else if(hours > 0)
            {
                return $"{hours}小时{minutes}分钟{endWord}";
            }
            else if (minutes > 0)
            {
                return $"{minutes}分钟{seconds}秒{endWord}";
            }
            else if (seconds >= 0)
            {
                return $"{seconds}秒{endWord}";
            }
            return $"{days}天{hours}小时{minutes}分钟{seconds}秒{endWord}";
        }
        
        public static string GetTimeAgoOrAfterFormat(long timestamp ,bool showEndWord = true)
        {
            // 将Unix时间戳转换为UTC时间，再转为本地时间
            DateTimeOffset utcTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            DateTimeOffset localTime = utcTime.ToLocalTime();
    
            // 获取当前本地时间
            DateTimeOffset now = DateTimeOffset.Now;
    
            // 计算时间差
            TimeSpan difference = now - localTime;
            bool isFuture = difference < TimeSpan.Zero;
            TimeSpan absDifference = difference.Duration();
    
            // 分解时间差
            int days = absDifference.Days;
            int hours = absDifference.Hours;
            int minutes = absDifference.Minutes;
            int seconds = absDifference.Seconds;
    
            string endWord = showEndWord ? (isFuture ? "后" : "前") : "";
            // 格式化输出
            if (days > 0)
            {
                return $"{days}天{endWord}";
            }
            else if(hours > 0)
            {
                return $"{hours}小时{endWord}";
            }
            else if (minutes > 0)
            {
                return $"{minutes}分钟{endWord}";
            }
            else if (seconds >= 0)
            {
                return $"{seconds}秒{endWord}";
            }
            return $"{days}天{hours}小时{minutes}分钟{seconds}秒{endWord}";
        }

        public static DateTime GetDataUpdateTime(DateTime localTime)
        {
            //如果localTime在当天4：00-24：00之间，就返回第二天的4：00
            if (localTime.Hour >= 4 && localTime.Hour < 24)
            {
                var nextDateTime = localTime.AddDays(1);
                return new DateTime(nextDateTime.Year, nextDateTime.Month, nextDateTime.Day, 4, 0, 0);
            }
            else
            {
                return new DateTime(localTime.Year, localTime.Month, localTime.Day, 4, 0, 0);
            }
        }
    }
}