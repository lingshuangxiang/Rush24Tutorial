using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

public class ButtonClickEffect : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public Image image;
    
    Sequence btnPressSequence;

    private void OnSelectEffect()
    {
        //play animation
        if (btnPressSequence ==  null)
        {
            btnPressSequence = TweenUtils.DOPressSequence(image.transform, .1f);
            btnPressSequence.SetAutoKill(false);
        }
        else
        {
            btnPressSequence.Restart();
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        BGMManager.Instance.PlayAFX(AFXMusic.ButtonEffectClick);
        OnSelectEffect();
    }
}
