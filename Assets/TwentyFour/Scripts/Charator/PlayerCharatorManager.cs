using System.Collections;
using System.Collections.Generic;
using Passport;
using TwentyFour.Scripts.PersonaProperty;
using Unity.UOS.TwentyFour;
using UnityEngine;
using UOS.TwentyFour.Charator;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class PlayerCharatorManager : MonoBehaviour
{
    public const string DefaultAvatarHeadSlug = "CharBlackBall";
    public const string DefaultAvatarEyeSlug = "CharBlackBall";
    public const string DefaultAvatarMouthSlug = "";
    public const string DefaultAvatarHeadwearSlug = "";
    
    public Transform HeadWrapper;
    public Transform EyeWrapper;
    public Transform MouthWrapper;
    public Transform HeadwearWrapper;
    
    public string ActiveAvatarHeadSlug = DefaultAvatarHeadSlug;
    public string ActiveAvatarEyeSlug = DefaultAvatarEyeSlug;
    public string ActiveAvatarMouthSlug = "";
    public string ActiveAvatarHeadwearSlug = "";

    private GameObject headIns, eyeIns, mouthIns, headwearIns;
    private List<CharAnimation> charAnimations = new List<CharAnimation>();

    public void InitPlayerAvatar(Dictionary<string, string> properties)
    {
        properties.TryGetValue(PersonaPropertyKeys.ActiveAvatarHeadKey, out string headSlug);
        ActiveAvatarHeadSlug = headSlug ?? DefaultAvatarHeadSlug;
        
        properties.TryGetValue(PersonaPropertyKeys.ActiveAvatarEyeKey, out string eyeSlug);
        ActiveAvatarEyeSlug = eyeSlug ?? DefaultAvatarEyeSlug;
        
        properties.TryGetValue(PersonaPropertyKeys.ActiveAvatarMouthKey, out string mouthSlug);
        ActiveAvatarMouthSlug = mouthSlug ?? DefaultAvatarMouthSlug;
        
        properties.TryGetValue(PersonaPropertyKeys.ActiveAvatarHeadwearKey, out string headwearSlug);
        ActiveAvatarHeadwearSlug = headwearSlug ?? DefaultAvatarHeadwearSlug;
        
        LoadCharator();
    }

    private void LoadCharator()
    {
        AllAvatarParts parts = AllAvatarParts.GetAllAvatarParts();
        charAnimations = new List<CharAnimation>();
        
        if (headIns)
        {
            Destroy(headIns);
        }
        if (eyeIns)
        {
            Destroy(eyeIns);
        }
        if (mouthIns)
        {
            Destroy(mouthIns);
        }
        
        if (headwearIns)
        {
            Destroy(headwearIns);
        }

        GameObject headPrefab = null, eyePrefab = null, mouthPrefab = null, headwearPrefab = null;
        
        foreach (var part in parts.avatarParts)
        {
            if (part.slug.Equals(ActiveAvatarHeadSlug))
            {
                headPrefab = part.prefab;
                headIns = Instantiate(headPrefab, HeadWrapper);
                charAnimations.Add(headIns.GetComponent<CharAnimation>());
                headIns.GetComponent<CharPart>()?.SetActivePart(CharPartEnum.Head);
            }
            if (part.slug.Equals(ActiveAvatarEyeSlug))
            {
                eyePrefab = part.prefab;
                eyeIns = Instantiate(eyePrefab, EyeWrapper);
                charAnimations.Add(eyeIns.GetComponent<CharAnimation>());
                eyeIns.GetComponent<CharPart>()?.SetActivePart(CharPartEnum.Eye);
            }
            if (part.slug.Equals(ActiveAvatarMouthSlug))
            {
                mouthPrefab = part.prefab;
                mouthIns = Instantiate(mouthPrefab, MouthWrapper);
                charAnimations.Add(mouthIns.GetComponent<CharAnimation>());
                mouthIns.GetComponent<CharPart>()?.SetActivePart(CharPartEnum.Mouth);
            }
            if (part.slug.Equals(ActiveAvatarHeadwearSlug))
            {
                headwearPrefab = part.prefab;
                headwearIns = Instantiate(headwearPrefab, HeadwearWrapper);
                charAnimations.Add(headwearIns.GetComponent<CharAnimation>());
                headwearIns.GetComponent<CharPart>()?.SetActivePart(CharPartEnum.Headwear);
            }
        }
    }

    public void StartLookAroundAnimation()
    {
        foreach (var anim in charAnimations)
        {
            anim?.PlayLookAroundAnimation();
        }
    }
    
    public void StopLookAroundAnimation()
    {
        foreach (var anim in charAnimations)
        {
            anim?.StopLookAroundAnimation();
        }
    }
    
    public void PlayCelebrateAnimation()
    {
        foreach (var anim in charAnimations)
        {
            anim?.PlayCelebrateAnimation();
        }
    }
    
    public void PlayWorkingAnimation()
    {
        foreach (var anim in charAnimations)
        {
            anim?.PlayWorkingAnimation();
        }
    }

    public void PlayIdle()
    {
        foreach (var anim in charAnimations)
        {
            anim?.Idle();
        }
    }

    public static Sprite GetLocalPlayerLoadingMaskSprite()
    {
        var sprite = AllAvatarParts.GetAllAvatarParts().avatarParts.Find(x => x.slug.Equals(DefaultAvatarHeadSlug))
            .LoadingMaskSprite;
        if (PersonaPropertiesHelper.GetLocalProperties()
            .TryGetValue(PersonaPropertyKeys.ActiveAvatarHeadKey, out string headSlug))
        {
            sprite = AllAvatarParts.GetAllAvatarParts().avatarParts
                    .Find(x => x.slug.Equals(headSlug)).LoadingMaskSprite;
        }
        
        return sprite;
    }
}
