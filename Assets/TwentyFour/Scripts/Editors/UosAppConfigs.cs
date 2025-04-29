using System.Collections;
using System.Collections.Generic;
using Guild;
using UnityEngine;

// public class ReadUosAppData : MonoBehaviour
// {
//     // Start is called before the first frame update
//     public string ReadLineFromUosAppConfig(int index)
//     {
//         TextAsset textAsset = Resources.Load<TextAsset>("UosAppConfig");
//         if (textAsset == null)
//         {
//             Debug.LogError("UosAppConfig.txt not found in Resources folder");
//             return string.Empty;
//         }
//
//         string[] lines = textAsset.text.Split('\n');
//         if (index < 0 || index >= lines.Length)
//         {
//             Debug.LogWarning($"Index {index} is out of range. File has {lines.Length} lines.");
//             return string.Empty;
//         }
//
//         return lines[index].Trim();
//     }
//     
//     
// }

//using UnityEngine;

[CreateAssetMenu(fileName = "UosAppConfig", menuName = "ScriptableObjects/UosAppConfig", order = 1)]
public class UosAppConfigs : ScriptableObject
{
    public string MatchConfigId;
    public string RoomProfileUUID;
    public string AccoplishStatelessUrl;
    public string CurrentEnv;
    public static UosAppConfigs GetUosAppConfigs()
    {
        return Resources.Load<UosAppConfigs>("UosConfigs");
    }

    public static string GetBaseStatelessUrl()
    {
        return $"https://stateless.unity.cn/release/{GetUosAppConfigs().AccoplishStatelessUrl}/";
    }


}
