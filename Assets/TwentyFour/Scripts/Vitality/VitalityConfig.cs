using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TwentyFour.Scripts.Vitality
{
    public class VitalityConfig
    {
        public int interval;
        public int points;
        public int matchCost;
    }
   [Serializable]
    public class VitalityResource 
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("maxValue")]
        public int MaxValue { get; set; }

        [JsonProperty("customData")]
        public Dictionary<string, object> CustomData { get; set; } = new();

        [JsonProperty("resourceSlug")]
        public string ResourceSlug { get; set; }

        [JsonProperty("resourceType")]
        public string ResourceType { get; set; }

        [JsonProperty("namespace")]
        public string Namespace { get; set; }
    }

    [Serializable]
    public class VitalityResourceQuery 
    {
        [JsonProperty("resource")]
        public VitalityResource Resource { get; set; }

        [JsonProperty("lastUpdateTime")]
        public string LastUpdateTime { get; set; }

        [JsonProperty("quantity")]
        public int CurrentQuantity { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
        // 实用方法：计算剩余百分比
        public float GetRemainingPercentage() => 
            (float)CurrentQuantity / Resource.MaxValue * 100;
    }
}