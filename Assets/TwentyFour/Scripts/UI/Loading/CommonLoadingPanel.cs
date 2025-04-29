using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class CommonLoadingPanel : MonoBehaviour
{
    public Text LoadingText;
    public float IntervalSecond = 0.5f;
    public string Hint;
    YieldInstruction waitSecond;
    private int hintIndex = 0;
    private List<string> ends = new List<string>()
    {
        string.Empty,
        ".",
        "..",
        "...",
    };

    public void Show(string text)
    {
        waitSecond = new WaitForSeconds(IntervalSecond);
        gameObject.SetActive(true);
        Hint = text;
        StopAllCoroutines();
        StartCoroutine(ShowLoading());
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        Hint = string.Empty;
        StopAllCoroutines();
    }

    IEnumerator ShowLoading()
    {
        while (true)
        {
            LoadingText.text = Hint + ends[hintIndex];
            hintIndex++;
            if (hintIndex >= ends.Count)
            {
                hintIndex = 0;
            }
            yield return waitSecond;

        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
