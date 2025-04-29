using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
[System.Serializable]
public class UIEffectData
{
    public RectTransform Target;
    public float Duration = 0.2f;
    public float Delay = 0f;
    [NonSerialized]
    public Vector3 OriginScale;
    public float StartScale;
    public Ease Ease = Ease.Linear;
    public bool ShakeRotation = false;
    public float ShakeIntensity = 0f;

    private Tween tween;
    public void Init()
    {
        OriginScale = Target.localScale; 
        Reset();
    }

    public void Reset()
    {
        Target.DOKill();
        Target.localScale = OriginScale * StartScale;
    }
    
    public void Play()
    {
        tween = Target.DOScale(OriginScale, Duration).SetEase(Ease);
        if (ShakeRotation)
            tween = Target.DOShakeRotation(Duration, ShakeIntensity).SetEase(Ease);
    }
}
