using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFour.Scripts.Art.Effects
{
    public class FianlPanelEffect : MonoBehaviour
    {
        public GameObject Text_timer, Text_Hint;

        public Color DefaultColor;
        public Color GreenColor;
        public Image Mainpanel;

        public void OnMatchSuccessEffect()
        {
            StopAllCoroutines();
            StartCoroutine("greyToGreen");
            Text_timer.gameObject.SetActive(false);
            Text_Hint.gameObject.SetActive(true);
        }

        IEnumerator greyToGreen()
        {
            float a = 0;
            while (a < 1)
            {
                a += Time.deltaTime;
                Mainpanel.color = Color.Lerp(Mainpanel.color, GreenColor, a);
                yield return null;
            }
        }
    }
}