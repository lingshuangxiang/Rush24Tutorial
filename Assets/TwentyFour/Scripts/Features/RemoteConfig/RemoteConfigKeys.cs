namespace TwentyFour.Scripts.RemoteConfig
{
    public static class RemoteConfigKeys
    {
        public const string DefaultRankCount = "RankCount";
        
        // 机器人参数配置
        public const string RobotClientConfig = "RobotClientConfig";
        public const string RobotServerConfig = "RobotServerConfig";
        
        public const string UseAdvanceQuestionsRate = "UseAdvanceQuestionsRate";
        
        public const string WXSubscribe_TournamentMatch = "WXSubscribe_TournamentMatch";
        
        public const string CurrentSeasonSlug = nameof(CurrentSeasonSlug);
        
        // 机器人降级配置
        public const string DowngradeConfig = "DowngradeConfig";
    }
}