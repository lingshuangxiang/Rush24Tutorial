using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.UOS.TwentyFour.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Unity.UOS.TwentyFour
{
    public class CardGroup : MonoBehaviour, IPointerClickHandler
    {
        public delegate void CardGroupClick(int index);
        
        [SerializeField] public int CardIndex;
        [SerializeField] public GivenCard Original;
        [SerializeField] public GameObject AppendingParent;
        [SerializeField] public TempValueTag ValueTag;
        
        public List<GivenCard> appendings;
        public CardGroupClick onCardGroupClick;
        
        private float currentValue;
        public Operator.Fraction fractionValue;
        private float originValue;
        private string markValue;//可能除下来是分数
        private bool isLocked;
        private Sequence aniSequence;
        
        private Image _highlightBorder;
        private Image highlightBorder
        {
            get
            {
                if (_highlightBorder == null)
                {
                    _highlightBorder = GetComponent<Image>();
                }

                return _highlightBorder;
            }
        }

        private void Update()
        {
            //TODO: highlightborder breath effect
        }

        public void ShowValueTag()
        {
            if (Common.Utils.IsInteger(currentValue)) //是整数
            {
                ValueTag.SetText($"{currentValue:N0}");
            }
            else
            {
                ValueTag.SetText($"{fractionValue.Numerator.ToString()}/{fractionValue.Denominator.ToString()}");
            }

            ValueTag.gameObject.SetActive(true);
        }

        public void HideValueTag()
        {
            ValueTag.SetText("");
            ValueTag.gameObject.SetActive(false);
        }

        /// <summary>
        /// 合并数据并且更新位置
        /// </summary>
        /// <param name="group2"></param>
        /// <param name="newValue"></param>
        public void Append(CardGroup group2, float newValue)
        {
            
            currentValue = newValue;
            ShowValueTag();
            ///////  设置卡牌位置     ////////
            List<GivenCard> cards = new List<GivenCard> { group2.Original };
            cards.AddRange(group2.appendings);
            int i = appendings.Count;
            foreach (var card in cards)
            {
                card.gameObject.transform.SetParent(AppendingParent.transform);
                Vector3 pos = card.gameObject.transform.localPosition;
                card.gameObject.transform.localPosition = new Vector3(pos.x, pos.y, -0.01f * ++i);
            }
            //////////////////
            appendings.AddRange(cards);
            group2.SetEmpty();
        }

        public void SetEmpty()
        {
            HideValueTag();
            appendings = new List<GivenCard>();
        }

        /// <summary>
        /// 用于显示自己的Value，如果不是整数就需要显示分数
        /// </summary>
        /// <returns></returns>
        public string ShowValue()
        {
            
            if (Common.Utils.IsInteger(GetValue()))//是整数就显示整数
            {
                return GetValue().ToString();
            }
            else// 不是整数就显示分数
            {
                return fractionValue.Numerator.ToString()+"/"+fractionValue.Denominator.ToString();
            }
        }

        public float GetValue()
        {
            return currentValue;
        }


        public void ResetCard(Stage stage)
        {
            Original.Init(stage.question.cards[CardIndex]);
            HideValueTag();
            SetHighlighted(false);
            isLocked = false;
            gameObject.SetActive(true);
            var oTrans = Original.gameObject.transform;
            Vector3 pos = oTrans.localPosition;
            oTrans.localPosition = new Vector3(pos.x, pos.y, 0);
            
            oTrans.SetParent(transform);
            oTrans.SetSiblingIndex(0);
            
            currentValue = Original.cardModel.number;
            originValue = currentValue;//进行备份
            fractionValue = new Operator.Fraction((int)originValue, 1);//存储为分数数值
            appendings = new List<GivenCard>();
        }

        public void Init()
        {
            if (InGameManager.currentStage == null)
            {
                //all stages cleared, 
                return;
            }

            ResetCard(InGameManager.currentStage);
        }

        public void Init(Stage stage)
        {
            // currentStage = stage;
            ResetCard(stage);
        }

        public void Lock()
        {
            isLocked = true;
        }

        public void UnLock()
        {
            isLocked = false;
        }

        public void SetHighlighted(bool v = true)
        {
            if (highlightBorder)
            {
                highlightBorder.enabled = v;
            }

            if (v)
            {
                //perform bounce animation
                aniSequence = DOTween.Sequence();
                aniSequence.Append(transform.DOScale(0.95f, 0.05f))
                    .Append(transform.DOScale(1.25f, 0.1f))
                    .Append(transform.DOScale(1.1f, 0.05f));
                aniSequence.Play();
            }
            else
            {
                //scale back immediately
                if (!(aniSequence is null) && aniSequence.IsPlaying())
                {
                    aniSequence.Kill();
                }
                transform.localScale = Vector3.one;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isLocked && onCardGroupClick != null)
            {
                onCardGroupClick(CardIndex);
            }
        }
    }
}