using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;
using TwentyFour.Scripts.Utilities;

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
