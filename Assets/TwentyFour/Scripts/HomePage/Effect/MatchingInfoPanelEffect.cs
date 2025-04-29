using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MatchingInfoPanelEffect : MonoBehaviour
{

    public GameObject Text_timer, Text_Hint;
    [FormerlySerializedAs("HintText")] public Text SmallHintText;
    public Text JoinText;
    public Color DefaultColor;
    public Color GreenColor;
    public Image Mainpanel;
    SimpleMatchTimer matchTimer;

    private bool isTweening;
    Tweener tweener;
    public float BreathingLightValue = 0.5f;
    public float BreathingLightDuration = 0.5f;
    private void Start()
    {
        matchTimer = Text_timer?.GetComponent<SimpleMatchTimer>();
    }

    public void Reset()
    {
        tweener?.Kill();
        isTweening = false;
        StopAllCoroutines();
        //StopCoroutine(nameof(greyToGreen));
        
        Mainpanel.color = DefaultColor;
        Text_timer.gameObject.SetActive(true);
        matchTimer?.ResetTimer();
        Text_Hint.gameObject.SetActive(false);
        
        JoinText.gameObject.SetActive(false);
        SmallHintText.gameObject.SetActive(true);
    }
    
    public void OnMatchSuccessEffect()
    {
        StopAllCoroutines();
        //StartCoroutine(greyToGreen());
        if (!isTweening)
        {
            isTweening = true;
            VibrateHelper.VibrateLong();
            BGMManager.Instance.PlayAFX(AFXMusic.BattleMatchMakingSuccess);
            tweener =  Mainpanel.DOColor(GreenColor,0.5f).From(DefaultColor).OnComplete((() =>
            {
                tweener = Mainpanel.DOFade(BreathingLightValue, BreathingLightDuration).SetLoops(-1, LoopType.Yoyo);
            }));
        }
        Text_timer.gameObject.SetActive(false);
        matchTimer?.ResetTimer();
        Text_Hint.gameObject.SetActive(true);
    }

    public void OnJoinRoomSuccessEffect()
    {
        JoinText.gameObject.SetActive(true);
        SmallHintText.gameObject.SetActive(false);
    }

    IEnumerator greyToGreen()
    {
        float a = 0;
        while (a<1)
        {
            a += Time.deltaTime;
            Mainpanel.color = Color.Lerp(Mainpanel.color, GreenColor, a);
            yield return null;
        }  
    }
}
