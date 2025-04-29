using System;
using System.Collections.Generic;
using Unity.UOS.TwentyFour.Model;
using UnityEngine;

namespace Unity.UOS.TwentyFour
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