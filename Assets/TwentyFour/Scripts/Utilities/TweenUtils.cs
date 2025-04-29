using System;
using DG.Tweening;
using UnityEngine;

public class TweenUtils
{
    public const float AnimationDurationFadeOut = .2f;
    public const float AnimationDurationShort = .35f;
    public static Sequence DOBounceInSequence(Transform transform, Sequence sequence = null, float softness = .2f)
    {
        if (sequence == null)
        {
            sequence = DOTween.Sequence();
        }
        
        sequence.Append(transform.DOScale(0, 0));
        sequence.Append(transform.DOScale(1 + softness, 3f / 7f * AnimationDurationShort));
        sequence.Append(transform.DOScale(1 - softness / 2, 11f / 35f * AnimationDurationShort));
        sequence.Append(transform.DOScale(1, 9f / 35f * AnimationDurationShort));

        return sequence;
    }
    
    public static Sequence DOZoomOutSequence(Transform transform, Sequence sequence = null, Action callback = null )
    {
        if (sequence == null)
        {
            sequence = DOTween.Sequence();
        }
        
        sequence.Append(transform.DOScale(0, AnimationDurationFadeOut));

        if (callback != null)
        {
            sequence.AppendCallback(callback.Invoke);
        }

        return sequence;
    }
    
    public static Sequence DOPressSequence(Transform transform, float duration = AnimationDurationShort, Sequence sequence = null, float softness = .2f)
    {
        float scaleX = transform.localScale.x, scaleY = transform.localScale.y;
        if (sequence == null)
        {
            sequence = DOTween.Sequence();
        }
        
        sequence.Append(transform.DOScale(new Vector3(scaleX, scaleY, transform.localScale.z), 0));
        sequence.Append(transform.DOScale(new Vector3((1f + 0.2f * softness) * scaleX, (1 - 0.2f * softness) * scaleY, transform.localScale.z), 1f / 4f * AnimationDurationShort));
        sequence.Append(transform.DOScale(new Vector3((1 - 0.2f * softness) * scaleX, (1f + 0.2f * softness) * scaleY, transform.localScale.z), 1f / 4f * AnimationDurationShort));
        sequence.Append(transform.DOScale(new Vector3(scaleX, scaleY, transform.localScale.z), 1f / 2f * AnimationDurationShort));

        return sequence;
    }
}
