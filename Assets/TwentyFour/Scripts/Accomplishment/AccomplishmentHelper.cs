using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

namespace TwentyFour.Scripts.Accomplishment
{
    public static class AccomplishmentHelper
    {
        public static AccomplishmentData LocalAccomplishmentData;
        public static IEnumerator GetData(string personaId)
        {
            var url =
                $"{UosAppConfigs.GetBaseStatelessUrl()}crud?method=get_matches&uniqueId={personaId}&page=1&pageSize=10";
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
                    var data = JsonUtility.FromJson<AccomplishmentData>(jsonString);
                    LocalAccomplishmentData = data;
          
                }
            }
        }
    }
}