using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Muninn;
using Unity.Muninn.Model;
using Unity.UOS.TwentyFour.Model;
using Unity.UOS.TwentyFour.Model.Sync;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.Advertisements;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

namespace Unity.UOS.TwentyFour
{
    [Serializable]
    public struct Expression
    {
        public int Left, Right; //card group index
        
        public Operator Operator;
        public float Result;
        public string fractionResult;
    }

    public class AnswerManager : MonoBehaviour
    {
        [SerializeField] public List<CardGroup> CardGroups;
        [SerializeField] public InGameManager InGameManager;

        private List<Expression> expressions;
        private Expression currentExpression;
        private List<TextMeshProUGUI> expressionTMPs;
        public TextMeshProUGUI myCardsResultHintText;
        

        // Start is called before the first frame update
        void Start()
        {
            expressionTMPs = new List<TextMeshProUGUI>();
            foreach (Transform child in transform)
            {
                expressionTMPs.Add(child.gameObject.GetComponent<TextMeshProUGUI>());
            }

            foreach (var cardGroup in CardGroups)
            {
                cardGroup.onCardGroupClick += AddCard;
            }
            ResetAnswer();
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void AddCard(int groupIndex)
        {
             Logger.Log("AddCard"+groupIndex);

             BGMManager.Instance.PlayAFX(AFXMusic.CardClick);
            //add to answer line
            if (currentExpression.Operator is null)//代表了进入公式的第一个卡牌
            {
                currentExpression.Left = groupIndex;
                HighlightCardGroup(groupIndex);
                UpdateExpressionText();
            }
            else  //代表选择了运算公式后选择的卡牌
            {
                
                currentExpression.Right = groupIndex; 
                HighlightCardGroup(-1);
                UpdateExpressionText();
                NextExpression();

                if (!CheckAnswer())
                {
                    //set previous target group as default selection of next expression
                    AddCard(expressions[^1].Right);
                }
            }
            
            PlayAnswerAFX();
            //TODO: send sync msg
        }

        void PlayAnswerAFX()
        {
            if (expressions.Count == 3)
            {
                var resolved = Math.Abs(expressions[2].Result - 24) < 0.01;
                AFXMusic afxMusic = resolved ? AFXMusic.BattleCorrect : AFXMusic.WrongAnswer;
                BGMManager.Instance.PlayAFX(afxMusic);
            }
        }
        public void AddOperator(Operator op)
        {
            if (currentExpression.Left < 0)
            {
                return;
            }
            CardGroups[currentExpression.Left].Lock();
            currentExpression.Operator = op;
            
            UpdateExpressionText();
            
            //TODO: send sync msg
        }

        public void RemoveOperator(Operator op)
        {
            if (currentExpression.Left < 0)
            {
                return;
            }
            CardGroups[currentExpression.Left].UnLock();
            currentExpression.Operator = null;
            UpdateExpressionText();
            
        }
        
        
        public void ResetAnswer()
        {
            //reset calculate steps
            currentExpression = new Expression
            {
                Left = -1,
                Right = -1
            };
            expressions = new List<Expression>(3);
            
            //clean answer paper
            foreach (var eTMP in expressionTMPs)
            {
                eTMP.text = "";
            }

            foreach (var cardGroup in CardGroups)
            {
                cardGroup.SetHighlighted(false);
            }

            //reset cards
            foreach (var cardGroup in CardGroups)
            {
                cardGroup.Init();
            }
        }

        private void HighlightCardGroup(int index)
        {
            for (int i=0; i< CardGroups.Count; i++)
            {
                CardGroups[i].SetHighlighted(i == index);
            }
        }

        private void UpdateExpressionText()
        {
            SetExpressionText(expressions.Count, currentExpression);
        }

        private void SetExpressionText(int tmpIndex, Expression exp)
        {
            if (tmpIndex < 0 || tmpIndex >= expressionTMPs.Count)
            {
                return;
            }

            string expressionText = "";

            if (exp.Left >= 0)
            {
                expressionText += $"{CardGroups[exp.Left].ShowValue()}";
            }

            if (!(exp.Operator is null))
            {
                expressionText += $" {exp.Operator.Symble()}";
            }

            if (exp.Right >= 0)
            {
                float testCalculate = exp.Operator
                    .Calculate(CardGroups[exp.Left].GetValue(), CardGroups[exp.Right].GetValue());
                if (Common.Utils.IsInteger(testCalculate)) //是整数
                {
                    expressionText +=
                        $" {CardGroups[exp.Right].ShowValue()} = {(int)(exp.Operator.Calculate(CardGroups[exp.Left].GetValue(), CardGroups[exp.Right].GetValue()))}";
                }
                else //得用分数形式进行计算
                {
                    Operator.Fraction fraction = exp.Operator.CalculateFraction(CardGroups[exp.Left].fractionValue,
                        CardGroups[exp.Right].fractionValue, exp.Operator.name); //计算分数
                    expressionText +=
                        $" {CardGroups[exp.Right].ShowValue()} = {fraction.Numerator.ToString()}/{fraction.Denominator.ToString()}";
                }
            }

            expressionTMPs[tmpIndex].text = expressionText;
        }
        
        private void NextExpression()
        {
            CardGroup left = CardGroups[currentExpression.Left], right = CardGroups[currentExpression.Right];
            //do calculation
            currentExpression.Result = currentExpression.Operator.Calculate(left.GetValue(), right.GetValue());
            
            //move cards
            right.Append(left, currentExpression.Result);
            right.fractionValue = currentExpression.Operator.CalculateFraction(left.fractionValue,right.fractionValue,currentExpression.Operator.name);//计算分数
            right.ShowValueTag();
            //de-highlight left
            CardGroups[currentExpression.Left].SetHighlighted(false);

            expressions.Add(currentExpression);
            currentExpression = new Expression
            {
                Left = -1,
                Right = -1
            };
        }


        private bool CheckAnswer()
        {

            if (InGameManager.GameMode == GameMode.Stage)
            {
                if (expressions.Count == 3 && Math.Abs(expressions[2].Result - 24) < 0.01)
                {
                    InGameManager.ShowResult(true);
                    return true;
                }       

                return false;
            }
            // 联机模式
            else
            {
                SubmitAnswer(expressions, InGameManager.currentStage.index);
            }
   
            return false;

        }
        
        /// <summary>
        /// 消息：提交答案给服务器
        /// </summary>
        /// <param name="expressions"></param>
        /// <param name="index"></param>
        public void SubmitAnswer(List<Expression> expressions, int index)
        {
            var serverMessage = new MuninnMessageData()
            {
                type = MuninnMessageData.Type.SubmitAnswer.ToString(),
                answerExpressions = expressions,
                answerIndex = index,
                personaID = Identity.persona.PersonaID
            };
            Logger.Log($"提交的答案 : {JsonUtility.ToJson(serverMessage)}");
            MuninnNetwork.RaiseEvent(
                Encoding.UTF8.GetBytes(JsonUtility.ToJson(serverMessage)),
                new RaiseEventOptions() {Target = RaiseEventTarget.TO_PLUGIN}
            );
        }

    }
}