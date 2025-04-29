using System;
using System.Collections;
using System.Collections.Generic;

using Unity.UOS.TwentyFour;
using UnityEditor;
using UnityEngine;

public class CollectCardsEffect : MonoBehaviour
{
    public AnswerManager findAnswerManager;

    public List<Transform> cards = new List<Transform>();

    public List<Transform> animPoint = new List<Transform>();

    public float lerpTime=.5f;

    public float ShiftValue = 0.3f;
    
    public void Start()
    {
        findAnswerManager= FindObjectOfType<AnswerManager>();   
    }

    /// <summary>
    /// 延迟一秒钟播放showandPlay
    /// </summary>
    public void WaitToShow()
    {
        StopCoroutine(waitShow());
        StartCoroutine(waitShow());
    }

    IEnumerator waitShow()
    {
        yield return new WaitForSeconds(1f);
        ShowAndPlay();
    }

    private Coroutine moveCardsCoroutine;
    private Animator spreadCardsAnimator;
    
    public void ShowAndPlay()
    {
        //hide highlight border
        if (findAnswerManager != null)
        {
            for (int i = 0; i < findAnswerManager.CardGroups.Count; i++)
            {
                var cardGroup = findAnswerManager.CardGroups[i];
                cardGroup.SetHighlighted(false);
            }
        }
        
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.parent = animPoint[i];
        }
        spreadCardsAnimator = animPoint[0].transform.parent.GetComponent<Animator>();
        
        if (moveCardsCoroutine != null)
        {
            StopCoroutine(moveCardsCoroutine);
        }
        moveCardsCoroutine = StartCoroutine(LerpToOrigin());
    }

    IEnumerator LerpToOrigin()
    {
        float screeenheight = Screen.height;
        float lerpAlpha = 0;
        
        spreadCardsAnimator.Play("Idle");
        while (lerpAlpha <= 1)
        {
            lerpAlpha += Time.deltaTime * 1 / lerpTime;
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].transform.localPosition =
                    Vector3.Lerp(cards[i].transform.localPosition, Vector3.up * ShiftValue, lerpAlpha);
                cards[i].transform.localRotation = Quaternion.Euler(Vector3.zero);
            }

            yield return null;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.localPosition =
                Vector3.Lerp(cards[i].transform.localPosition, Vector3.up * ShiftValue, 1);
        }

        animPoint[0].transform.parent.GetComponent<Animator>().Play("Start");
    }
 
    public void ResetToOrigin()
    {
        if (spreadCardsAnimator != null)
        {
            spreadCardsAnimator.Play("Idle");
        }

        if (moveCardsCoroutine != null)
        {
            StopCoroutine(moveCardsCoroutine);
        }
        
        if (findAnswerManager != null)
        {
            for (int i = 0; i < findAnswerManager.CardGroups.Count; i++)
            {
                Transform waitToFind;
                cards[i].SetParent( findAnswerManager.CardGroups[i].transform);
                cards[i].transform.localPosition=Vector3.zero;
                cards[i].transform.localRotation = Quaternion.Euler(Vector3.zero);
                cards[i].transform.localScale =Vector3.one;;
            }
        }
    }

    // [Button]
    // public void TestFind()
    // {
    //     cards.Clear();
    //     if (findAnswerManager != null)
    //     {
    //         for (int i = 0; i < findAnswerManager.CardGroups.Count; i++)
    //         {
    //             Transform waitToFind;
    //             waitToFind = findAnswerManager.CardGroups[i].transform.Find("Card");
    //             if (waitToFind != null)
    //             {
    //                 cards.Add(waitToFind);
    //                 continue;
    //             }
    //             if()
    //             
    //         }
    //     }
    // }


}
// [CustomEditor(typeof(TaEffectCollectCards))]
// public class TaEffectCollectCardsEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         //继承基类方法
//         base.OnInspectorGUI();
//        
//         //绘制Button
//         if (GUILayout.Button("尝试寻找"))
//         {
//             TaEffectCollectCards myScript = (TaEffectCollectCards)target;   
//           //  myScript.TestFind();    
//             //执行方法
//           
//         }
//      
//     }
//     
// }
