using System.Collections;
using System.Collections.Generic;
using Guild;
using UnityEngine;
using Unity.UOS.TwentyFour;

[CreateAssetMenu(fileName = "AllAvatarParts", menuName = "ScriptableObjects/AllAvatarParts", order = 2)]
public class AllAvatarParts : ScriptableObject
{
    public List<AvatarPart> avatarParts;

    static AllAvatarParts AllAvatarPartsInstance;
    public static AllAvatarParts GetAllAvatarParts()
    {
        AllAvatarPartsInstance ??= Resources.Load<AllAvatarParts>("AllAvatarParts");
        return AllAvatarPartsInstance;
    }
}
