using System.Collections.Generic;
using UnityEngine;

namespace TwentyFour.Scripts.Features.Player
{
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
}