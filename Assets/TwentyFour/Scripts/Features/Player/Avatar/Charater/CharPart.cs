using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TwentyFour.Scripts.Features.Player
{
    public enum CharPartEnum
    {
        Head,
        Eye,
        Mouth,
        Headwear
    }
    
    public class CharPart : MonoBehaviour
    {
        public GameObject Head, Eye, Mouth, Headwear;

        public void SetActivePart(CharPartEnum part)
        {
            if (Head)
            {
                Head.SetActive(part.Equals(CharPartEnum.Head));
            }
            if (Eye)
            {
                Eye.SetActive(part.Equals(CharPartEnum.Eye));
            }
            if (Mouth)
            {
                Mouth.SetActive(part.Equals(CharPartEnum.Mouth));
            }
            if (Headwear)
            {
                Headwear.SetActive(part.Equals(CharPartEnum.Headwear));
            }
        }
    }
}

