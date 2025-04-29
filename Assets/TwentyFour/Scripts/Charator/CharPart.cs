using System;
using System.Collections;
using System.Collections.Generic;
using TwentyFour.Scripts.PersonaProperty;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UOS.TwentyFour.Charator
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

        public static bool IsOneSet(Dictionary<string,string> properties,string setName)
        {
            if (properties.TryGetValue(PersonaPropertyKeys.ActiveAvatarEyeKey, out string eye) &&
                properties.TryGetValue(PersonaPropertyKeys.ActiveAvatarMouthKey, out string mouth) &&
                properties.TryGetValue(PersonaPropertyKeys.ActiveAvatarHeadKey, out string head) &&
                eye == setName && head == setName && mouth == setName)
            {
               return true; 
            }
            return false;
        }
        public static string GetSetSlug(Dictionary<string,string> properties)
        {
            if (properties.TryGetValue(PersonaPropertyKeys.ActiveAvatarEyeKey, out string eye) &&
                properties.TryGetValue(PersonaPropertyKeys.ActiveAvatarMouthKey, out string mouth) &&
                properties.TryGetValue(PersonaPropertyKeys.ActiveAvatarHeadKey, out string head) &&
                eye == mouth && mouth == head && head == eye)
            {
                return eye; 
            }
            return string.Empty;
        }

    }
}

