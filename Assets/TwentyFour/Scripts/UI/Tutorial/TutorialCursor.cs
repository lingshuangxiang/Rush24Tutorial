using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TutorialCursor : MonoBehaviour
{
    public GameObject Pointer;

    private Tweener pointerTweener;

    // Start is called before the first frame update
    void Start()
    {
        pointerTweener = Pointer.transform.DOScale(1.5f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetAutoKill(false);
    }

    public void Display(bool value)
    {
        gameObject.SetActive(value);

        if (value)
        {
            pointerTweener.Play();
        }
        else
        {
            pointerTweener.Pause();
        }
    }

    public void DoMove(Vector3 position, float duration)
    {
        transform.DOMove(position, duration);
    }

 
}