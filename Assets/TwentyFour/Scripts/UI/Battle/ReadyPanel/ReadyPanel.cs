using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadyPanel : MonoBehaviour
{
    public List<GameObject> CountDownItems = new List<GameObject>();

    private void OnEnable()
    {
        //StartCoroutine(TestCountDown());
    }

    public void Play(int countDown)
    {
        if (countDown < 0 || countDown >= CountDownItems.Count)
        {
            return;
        }
        CountDownItems[countDown].SetActive(true);
    }

    IEnumerator TestCountDown()
    {
        for (int i = 3; i >= 0; i--)
        {
            Play(i);
            yield return new WaitForSeconds(1f);
        }
    }
}
