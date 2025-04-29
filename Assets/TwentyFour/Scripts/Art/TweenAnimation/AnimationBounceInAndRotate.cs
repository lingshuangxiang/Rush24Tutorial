using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;

public class AnimationBounceInAndRotate : MonoBehaviour
{
    const float RotateDuration = 2f;

    private Sequence _mySequence;
    
    // Start is called before the first frame update
    void Start()
    {

    }
    
    void OnEnable()
    {
        if (_mySequence == null)
        {
            _mySequence = TweenUtils.DOBounceInSequence(transform);
            _mySequence.SetAutoKill(false);
            _mySequence.Join(transform.DORotate(new Vector3(0, 0, 180), RotateDuration).SetEase(Ease.Linear));
            TweenUtils.DOZoomOutSequence(transform, _mySequence, () => { gameObject.SetActive(false); });
        }
        else
        {
            _mySequence.Restart();
        }
    }

    private void OnDestroy()
    {
        _mySequence?.Kill();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
