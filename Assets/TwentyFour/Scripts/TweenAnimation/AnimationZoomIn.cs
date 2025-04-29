using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DG.Tweening;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class AnimationZoomIn : MonoBehaviour
{
    [SerializeField] public float duration = 5f;
    [SerializeField] public float scale = 1.5f;

    private Tweener tween;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    void OnEnable()
    {
        tween = transform.DOScale(scale, duration);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnDestroy()
    {
        tween?.Kill();
    }
}
