using System;

namespace TwentyFour.Scripts.Tournament
{
   
    [Serializable]
    public class TournamentReferenceData
    {
        public TournamentDataReferenceSlugType SlugType;
        public SerializableDictionary<string, string> Datas;
    }

    [Serializable]
    public class TournamentTimeData
    {
        public string DailyActiveStartTime;
        public string DurationTime;
        public bool IsSpanDay;
    }
    public enum TournamentDataReferenceSlugType
    {
        Leaderboards,
        Categories,
        Quests,
        Currency
    }
}