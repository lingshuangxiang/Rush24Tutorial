using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class VITConsumeToast : MonoBehaviour
{
    public Text VITCostText;
    public RectTransform StartPos;
    public RectTransform EndPos;

    public RectTransform Target;
    public CanvasGroup TargetGroup;

    private void OnEnable()
    {
        //Show(5);
    }

    public void Show(int vitCost)
    {
        gameObject.SetActive(true);
        VITCostText.text = $"-{vitCost}";
        Target.DOMove(EndPos.position, 0.5f).From(StartPos.position).SetEase(Ease.Linear).OnComplete(() =>
        {
            TargetGroup.DOFade(0, 0.5f).From(1).OnComplete(() =>
            {
                gameObject.SetActive(false);

            });
        });
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
