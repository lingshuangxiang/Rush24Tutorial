using System;
using UnityEngine;

namespace TwentyFour.Scripts.Features.Player
{
    [Serializable]
    public class AvatarPart
    {
        public GameObject prefab;
        public string name;
        public string type;
        public string slug;
        public Sprite robbedSprite;
        public Sprite LoadingMaskSprite;
    }
}