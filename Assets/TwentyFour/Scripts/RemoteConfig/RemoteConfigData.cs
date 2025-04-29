using System.Collections;
using System.Collections.Generic;
using Cloud;
using Google.Protobuf.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RemoteConfigData", menuName = "ScriptableObjects/RemoteConfigData", order = 1)]
public class RemoteConfigData : ScriptableObject
{
    public List<string> Keys;
    public List<ConfigType> Types;
}
