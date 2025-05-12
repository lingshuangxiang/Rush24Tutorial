using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextEllipsisEffect : MonoBehaviour
{
    Text text;
    private string content;
    private int countIndex = 0;
    YieldInstruction waitSecond;
    public float IntervalSecond = 0.5f;

    private List<string> ends = new List<string>()
    {
        string.Empty,
        ".",
        "..",
        "...",
    };
    private void Awake()
    {
        text = GetComponent<Text>();
        content = text.text;
        waitSecond = new WaitForSeconds(IntervalSecond);

    }

    public void SetContent(string content)
    {
        this.content = content;
        StopAllCoroutines();
        if (string.IsNullOrEmpty(content))
        {
            text.text = string.Empty;
            
        }
        else
        {
            text.text = content + ends[countIndex];
            StartCoroutine(DoEffect());
        }
    }
    private void OnEnable()
    {
        StartCoroutine(DoEffect());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
    IEnumerator DoEffect()
    {
        while (true)
        {
            if (string.IsNullOrEmpty(content))
            {
                text.text = string.Empty;
            }
            else
            {
                text.text = content + ends[countIndex];
                countIndex++;
                if (countIndex >= ends.Count)
                {
                    countIndex = 0;
                }
            }
            yield return waitSecond;

        }
    }
}

    

