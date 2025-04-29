using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ReadyBgEffect : MonoBehaviour
{
    private void OnEnable()
    {
        
    }

    public void PlayEffect()
    {
        gameObject.SetActive(true);
        var image = GetComponent<Image>();
        var color = image.color;
        color.a = 0.2f;
        var tweener = DOTween.ToAlpha(() => color, (x) => color = x, 1f, 0.38f);
        tweener.onUpdate += () => { image.color = color; };
        tweener.onComplete += () =>
        {
            var tweener2 = DOTween.ToAlpha(() => color, (x) => color = x, 0.2f, 0.62f);
            tweener2.onUpdate += () => { image.color = color; };
        };
    }
}
