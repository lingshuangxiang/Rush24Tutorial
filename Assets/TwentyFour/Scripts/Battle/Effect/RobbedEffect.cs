using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TwentyFour.Scripts.PersonaProperty;
using Unity.Muninn.Model;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine.UI;
using UOS.TwentyFour.Charator;

public class RobbedEffect : MonoBehaviour
{
    private Sequence _mySequence;
   public GameObject robbedLabel;


   public Image BG;
   public Image Hint;
   public Image Icon;

   public GameObject Spark;
   
   public GameObject StartPos;
   public GameObject MidPos;
   public GameObject EndPos;

   public float MidWaitTime = 1f;
   
   
   public float BGFadeInTime = 1;
   public float HintZoomInTime = 1;
   public float HintZoomInDelayTime = 1;

   public Ease HintZoomInEase;

   public float BGFadeOutTime = 1;
   public float HintZoomOutTime = 1;
   public Ease HintZoomOutEase;

   public float IconMoveInTime = 1;
   public Ease IconMoveInEase;
   public float IconMoveOutTime = 1;
   public Ease IconMoveOutEase;

   public string CurrentResolvedPersonaID;

   public void OnEnable()
   {
       Play();
   }
   
   public void Play()
   {
       AllAvatarParts parts = AllAvatarParts.GetAllAvatarParts();
       MuninnPlayer p = MuninnManager.GetRoom().Players.
           Find(player => player.Id == CurrentResolvedPersonaID);
       if (p != null)
       {
           Sprite robbed = parts.avatarParts.Find(avatar => avatar.slug == CharSetSlugKeys.Default).robbedSprite;
           var slug = CharPart.GetSetSlug(p.Properties);
           if (!string.IsNullOrEmpty(slug))
           {
               robbed = parts.avatarParts.Find(avatar => avatar.slug == slug).robbedSprite;
           }
           Icon.sprite = robbed;
       }
       if(_mySequence!=null)
           _mySequence.Kill();
       Hint.gameObject.SetActive(false);
       Spark.SetActive(false);
       BG.DOFade(1, BGFadeInTime).From(0);
       Hint.transform.DOScale(1, HintZoomInDelayTime).From(1).OnComplete((() =>
       {
           Hint.gameObject.SetActive(true);

           Hint.transform.DOScale(1, HintZoomInTime).From(6).SetEase(HintZoomInEase).OnComplete((() =>
           {
               Spark.SetActive(true);
               //Spark.transform.DOScale(1, 0.5f).From(2);
               Spark.transform.DOShakeScale(1,2).OnComplete((() =>
               {
                   Spark.transform.localScale = Vector3.one;
               }));
           }));
           Hint.DOFade(1, HintZoomInTime).From(0).SetEase(Ease.Linear);
       }));
       robbedLabel?.SetActive(true);
       //robbedLabel?.transform.DOScale(1, 0.5f).From(5);

       _mySequence= DOTween.Sequence();
       _mySequence.Append(Icon.transform.DOMove(MidPos.transform.position, IconMoveInTime).From(StartPos.transform.position).SetEase(IconMoveInEase).OnComplete((
           () =>
           {
               
           })));
       _mySequence.Append(this.GetComponent<RectTransform>().DOScale(Vector2.one, MidWaitTime).OnComplete((() =>
       {
           Hint.transform.DOScale(0, HintZoomOutTime).From(1).SetEase(HintZoomOutEase);
           BG.DOFade(0, BGFadeOutTime).From(1);
       })));//相当于等待2s
       _mySequence.Append(Icon.transform.DOMove(EndPos.transform.position, IconMoveOutTime).From(MidPos.transform.position).SetEase(IconMoveOutEase).OnComplete((
           () =>
           {
           })));
       
       _mySequence.onComplete += Disappear;
       

   }
   void Disappear()
   {
      this.gameObject.SetActive(false);
      QuestionList getquestionlist = FindAnyObjectByType(typeof(QuestionList)) as QuestionList;
      getquestionlist.SetIndex(getquestionlist.myIndex); //TODO: next index?
   }
   

}
