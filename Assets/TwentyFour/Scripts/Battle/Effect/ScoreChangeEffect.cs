using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.UI;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class ScoreChangeEffect : MonoBehaviour
{
    public TextMeshProUGUI MyText;

    int currentScore;


    public ParticleSystem StarParticle;
    public Image BaseLight;
    public Image LightImage;
    public Image LightImage2;
    public RectTransform WinHint;

    public Text WinText;

    [Header("动画参数")] public float SmallSize;
    public float duration1;
    public float duration2;
    public Ease ease1;
    public Ease ease2;

    public Ease LightImageEase;
    public float LightImageScale = 1.5f;
    public float LightImageDuration = 1f;
    public Ease LightImageEase2;
    public float LightImageScale2 = 1.5f;
    public float LightImageDuration2 = 1f;

    private void OnEnable()
    {
        //测试
        // ShowScoreEffect(true);
    }

    public void ShowScore(int myScore, bool isMyScore)
    {
        MyText.text = myScore.ToString();
        bool win = false;
        if (currentScore < myScore)
        {
            if (myScore == 3)
            {
                win = true;
            }

            Logger.LogInfo("ShowScore" + myScore);
            ShowScoreEffect(win, isMyScore);
        }

        currentScore = myScore;
    }

    public void ShowScoreEffect(bool win, bool isMyScore)
    {
        MyText.transform.DOScale(SmallSize, duration1).From(4).SetEase(ease1).OnComplete((() =>
        {
            MyText.transform.DOScale(6, duration2).From(SmallSize).SetEase(ease2).OnComplete((() => { MyText.transform.DOScale(4, duration2).From(6).SetEase(ease1); }));
        }));
        if (win)
        {
            BaseLight.gameObject.SetActive(true);
            BaseLight.DOFade(0.7f, 1f).From(0);
            BaseLight.transform.DOScale(Vector3.one * 0.5f, 1f).From(Vector3.zero);
            BaseLight.transform.DOLocalRotate(new Vector3(0, 0, 360), 10, RotateMode.FastBeyond360).From(Vector3.zero).SetEase(Ease.Linear).SetLoops(-1);
            LightImage.transform.DOScale(LightImageScale, LightImageDuration).From(1).SetEase(LightImageEase).SetLoops(-1, LoopType.Yoyo);
            LightImage2.transform.DOScale(LightImageScale2, LightImageDuration2).From(1).SetEase(LightImageEase2).SetLoops(-1, LoopType.Yoyo);
            WinHint.gameObject.SetActive(true);
            WinHint.transform.DoCommonShakeRotationZ();

            if (WinText != null)
            {
                if (isMyScore)
                {
                    //current player win
                    WinText.text = "完胜(5:0)可以加更多星，继续加油！";
                }
                else
                {
                    WinText.text = "完败(0:5)会失去更多星，别放弃！";
                }
                WinText.gameObject.SetActive(true);
                WinText.DOFade(1f, 1f).From(0);
                WinText.transform.DOScale(1, 1f).From(0.2f);
            }
            
            StarParticle.Play();
        }
    }
}