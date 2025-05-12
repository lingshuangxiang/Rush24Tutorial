using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class DOTweenHelper
{
    public static void DoCommonShakeRotationZ(this Transform target, float interval = 0.1f, 
        float strength = 10f ,int times = 2)
    {
        var sequence =  DOTween.Sequence();
        for (int i = 0; i < times; i++)
        {
            sequence.Append(target.DORotate(new Vector3(0, 0, strength), interval));
            sequence.Append(target.DORotate(new Vector3(0, 0, -strength), interval));
        }

        sequence.Append(target.DORotate(Vector3.zero, interval));
    }

    public static void DoImagePixelsPerUnitMultiplier(this Image target, float startValue, float endValue,
        float duration = 1f, Ease ease = Ease.Linear ,Action onComplete = null)
    {
        var color  = new Color(1, 1, 1, startValue);
        var tweener = DOTween.ToAlpha(() => color, (x) => color = x, endValue, duration).SetEase(ease);
        tweener.onUpdate += () =>
        {
            target.pixelsPerUnitMultiplier = color.a;
            
        };
        tweener.OnComplete((() =>
        {
            onComplete?.Invoke();
        }));
    }
    
}
