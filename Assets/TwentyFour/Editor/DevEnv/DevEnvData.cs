using System;
using System.Collections.Generic;
using System.Linq;
using Cloud;
using UnityEngine;

namespace Unity.UOS.TwentyFour.Editor.DevEnv
{
    [CreateAssetMenu(fileName = "DevEnvData", menuName = "ScriptableObjects/DevEnvData", order = 2)]
    public class DevEnvData : ScriptableObject
    {
        public List<DevEnvInfo> DevEnvInfos = new List<DevEnvInfo>();
        
        public static DevEnvData GetDevEnvData()
        {
            return  Resources.Load<DevEnvData>("DevEnv/DevEnvData");
        }
        
    }

    [Serializable]
    public struct DevEnvInfo
    {
        public string name;
        public string app_id;
        public string app_secret;
        public string app_service_secret;
        public string match_config_id;
        public string room_profile_UUID;
        public string accoplish_stateless_url;
        public string cdn_bucket;
    }
}