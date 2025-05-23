using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TwentyFour.Scripts.Utilities;

public class ButtonDownEffect : MonoBehaviour,IPointerDownHandler
{
    [SerializeField] public Image image;
    
    Sequence btnPressSequence;
    

    public void OnPointerDown(PointerEventData eventData)
    {
        BGMManager.Instance.PlayAFX(AFXMusic.ButtonEffectClick);
        OnSelectEffect();
    }
    private void OnSelectEffect()
    {
        if(!GetComponent<Button>().interactable) return;
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
}

