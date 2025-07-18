using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Logger = TwentyFour.Scripts.Utilities.Logger;
using TwentyFour.Scripts.Gameplay.CardSystem;
using TwentyFour.Scripts.Utilities;

namespace TwentyFour.Scripts.Gameplay.GameMode.StageMode
{
    /// <summary>
    /// 运算表达式数据结构
    /// 
    /// 记录24点游戏中的每一步运算：
    /// - Left/Right: 参与运算的两个卡牌组索引
    /// - Operator: 运算符（+、-、×、÷）
    /// - Result: 运算结果（浮点数）
    /// - fractionResult: 分数形式的结果（字符串，当前未使用）
    /// 
    /// 24点游戏需要3次运算：4张→3张→2张→1张
    /// </summary>
    [Serializable]
    public struct Expression
    {
        public int Left, Right;         // 卡牌组索引（0-3）
        public Operator Operator;       // 运算符对象
        public float Result;            // 运算结果
        public string fractionResult;   // 分数结果（预留）
    }

    /// <summary>
    /// 24点游戏答案管理器 - 核心逻辑控制中心
    /// 
    /// 主要职责：
    /// 1. 处理玩家的卡牌选择和运算符选择
    /// 2. 管理24点游戏的运算序列（3步运算）
    /// 3. 验证最终结果是否等于24
    /// 4. 控制卡牌的高亮状态和视觉反馈
    /// 5. 显示运算过程和结果
    /// 
    /// 游戏流程：
    /// 选择卡牌1 → 选择运算符 → 选择卡牌2 → 计算合并 → 重复3次 → 检查结果
    /// 
    /// 设计模式：状态机 + 观察者模式
    /// </summary>
    public class AnswerManager : MonoBehaviour
    {
        // ============== 核心组件引用 ==============
        [SerializeField] public List<CardGroup> CardGroups;    // 4个卡牌组的引用列表
        [SerializeField] public InGameManager InGameManager;   // 游戏管理器引用

        // ============== 游戏状态数据 ==============
        private List<Expression> expressions;          // 已完成的运算列表（最多3个）
        private Expression currentExpression;          // 当前正在构建的运算表达式
        
        // ============== UI显示组件 ==============
        private List<TextMeshProUGUI> expressionTMPs;  // 显示运算过程的文本组件列表
        public TextMeshProUGUI myCardsResultHintText;   // 结果提示文本（未使用）
        
        /// <summary>
        /// 初始化答案管理器
        /// 
        /// 核心初始化流程：
        /// 1. 收集所有显示运算过程的文本组件
        /// 2. 为每个卡牌组绑定点击事件（观察者模式）
        /// 3. 重置游戏状态到初始状态
        /// 
        /// 事件绑定机制：
        /// CardGroup.onCardGroupClick 委托事件 → AnswerManager.AddCard 方法
        /// 实现了UI交互与游戏逻辑的解耦
        /// </summary>
        void Start()
        {
            // 收集子对象中的所有文本组件，用于显示运算步骤
            expressionTMPs = new List<TextMeshProUGUI>();
            foreach (Transform child in transform)
            {
                expressionTMPs.Add(child.gameObject.GetComponent<TextMeshProUGUI>());
            }

            // 为每个卡牌组绑定点击事件 - 关键的事件系统设计
            // 当玩家点击卡牌时，会调用 AddCard(int groupIndex) 方法
            foreach (var cardGroup in CardGroups)
            {
                cardGroup.onCardGroupClick += AddCard;
            }
            
            // 初始化游戏状态
            ResetAnswer();
        }

        /// <summary>
        /// 处理卡牌点击事件 - 24点游戏的核心交互逻辑
        /// 
        /// 状态机设计：
        /// 状态1: 等待第一张卡牌 (currentExpression.Operator == null)
        ///   - 设置 Left 索引，高亮选中的卡牌
        /// 状态2: 已选择运算符，等待第二张卡牌
        ///   - 设置 Right 索引，执行运算，进入下一轮
        /// 
        /// 智能选择机制：
        /// - 完成一次运算后，自动选择合并后的卡牌作为下次运算的起点
        /// - 这样可以连续进行多次运算，直到得到最终结果
        /// 
        /// 音效反馈：
        /// - 每次点击都有音效
        /// - 最终结果有特殊音效（正确/错误）
        /// </summary>
        /// <param name="groupIndex">被点击的卡牌组索引（0-3）</param>
        public void AddCard(int groupIndex)
        {
             Logger.Log("AddCard"+groupIndex);

             // 播放卡牌点击音效
             BGMManager.Instance.PlayAFX(AFXMusic.CardClick);
             
            // 状态机：根据当前表达式状态决定行为
            if (currentExpression.Operator is null) // 状态1：等待第一张卡牌
            {
                currentExpression.Left = groupIndex;
                HighlightCardGroup(groupIndex);     // 高亮选中的卡牌
                UpdateExpressionText();             // 更新显示
            }
            else  // 状态2：已选择运算符，现在选择第二张卡牌
            {
                currentExpression.Right = groupIndex; 
                HighlightCardGroup(-1);             // 取消所有高亮
                UpdateExpressionText();             // 更新显示
                NextExpression();                   // 执行运算并进入下一轮

                // 智能选择：如果游戏未结束，自动选择合并后的卡牌
                if (!CheckAnswer())
                {
                    // 使用语法糖 expressions[^1] 获取最后一个表达式的右侧索引
                    // 这是合并后的卡牌，作为下次运算的起点
                    AddCard(expressions[^1].Right);
                }
            }
            
            // 播放结果相关的音效
            PlayAnswerAFX();
        }

        /// <summary>
        /// 播放答案相关的音效
        /// 
        /// 音效逻辑：
        /// - 只在完成所有3次运算后播放特殊音效
        /// - 根据最终结果是否为24播放不同音效
        /// - 使用容错比较（0.01误差）处理浮点数精度问题
        /// </summary>
        void PlayAnswerAFX()
        {
            if (expressions.Count == 3)  // 完成了所有3次运算
            {
                // 浮点数比较：使用小误差范围而不是直接相等比较
                var resolved = Math.Abs(expressions[2].Result - 24) < 0.01;
                AFXMusic afxMusic = resolved ? AFXMusic.BattleCorrect : AFXMusic.WrongAnswer;
                BGMManager.Instance.PlayAFX(afxMusic);
            }
        }
        
        /// <summary>
        /// 添加运算符到当前表达式
        /// 
        /// 运算符添加逻辑：
        /// 1. 必须先选择左侧卡牌才能添加运算符
        /// 2. 锁定左侧卡牌防止重复选择
        /// 3. 更新显示以显示部分表达式
        /// 
        /// 状态转换：等待第一张卡牌 → 等待第二张卡牌
        /// </summary>
        /// <param name="op">选择的运算符对象</param>
        public void AddOperator(Operator op)
        {
            if (currentExpression.Left < 0)  // 必须先选择左侧卡牌
            {
                return;
            }
            
            // 锁定已选择的卡牌，防止状态混乱
            CardGroups[currentExpression.Left].Lock();
            currentExpression.Operator = op;
            
            UpdateExpressionText();
        }

        /// <summary>
        /// 移除当前表达式的运算符
        /// 
        /// 撤销操作：
        /// 1. 解锁左侧卡牌
        /// 2. 清除运算符
        /// 3. 更新显示
        /// 
        /// 状态转换：等待第二张卡牌 → 等待第一张卡牌
        /// </summary>
        /// <param name="op">要移除的运算符（用于验证，当前未使用）</param>
        public void RemoveOperator(Operator op)
        {
            if (currentExpression.Left < 0)  // 没有选择左侧卡牌时无需操作
            {
                return;
            }
            
            // 解锁卡牌并清除运算符
            CardGroups[currentExpression.Left].UnLock();
            currentExpression.Operator = null;
            UpdateExpressionText();
        }
        
        /// <summary>
        /// 重置游戏到初始状态
        /// 
        /// 完整重置流程：
        /// 1. 清空当前表达式和历史表达式
        /// 2. 清空所有显示文本
        /// 3. 取消所有卡牌高亮
        /// 4. 重置所有卡牌组到初始状态
        /// 
        /// 调用时机：
        /// - 游戏开始时
        /// - 重新开始游戏时
        /// - 切换关卡时
        /// </summary>
        public void ResetAnswer()
        {
            // 重置表达式状态
            currentExpression = new Expression
            {
                Left = -1,      // -1 表示未选择
                Right = -1
            };
            expressions = new List<Expression>(3);  // 预分配容量为3
            
            // 清空UI显示
            foreach (var eTMP in expressionTMPs)
            {
                eTMP.text = "";
            }

            // 重置卡牌视觉状态
            foreach (var cardGroup in CardGroups)
            {
                cardGroup.SetHighlighted(false);
            }

            // 重置卡牌数据状态
            foreach (var cardGroup in CardGroups)
            {
                cardGroup.Init();  // 恢复到初始的单张卡牌状态
            }
        }

        /// <summary>
        /// 控制卡牌组的高亮状态
        /// 
        /// 高亮逻辑：
        /// - index >= 0: 高亮指定索引的卡牌组，其他取消高亮
        /// - index < 0: 取消所有卡牌组的高亮
        /// 
        /// 视觉反馈设计：
        /// - 帮助玩家明确当前选择状态
        /// - 在选择第一张卡牌时提供视觉指示
        /// - 在选择运算符后清除高亮，等待第二张卡牌
        /// </summary>
        /// <param name="index">要高亮的卡牌组索引，-1表示清除所有高亮</param>
        private void HighlightCardGroup(int index)
        {
            for (int i=0; i< CardGroups.Count; i++)
            {
                CardGroups[i].SetHighlighted(i == index);
            }
        }

        /// <summary>
        /// 更新当前表达式的显示文本
        /// 
        /// 简单的封装方法，调用具体的文本设置逻辑
        /// 用于在表达式状态改变时刷新UI显示
        /// </summary>
        private void UpdateExpressionText()
        {
            SetExpressionText(expressions.Count, currentExpression);
        }

        /// <summary>
        /// 设置指定位置的表达式显示文本
        /// 
        /// 显示格式：
        /// - 部分表达式："5"（只有左侧）
        /// - 带运算符："5 +"（左侧+运算符）
        /// - 完整表达式："5 + 3 = 8"（完整运算结果）
        /// 
        /// 数值显示策略：
        /// - 整数：直接显示整数形式
        /// - 分数：显示为分数形式（如 "2/3"）
        /// 
        /// 分数计算：
        /// - 避免浮点精度问题
        /// - 提供更直观的数学表示
        /// </summary>
        /// <param name="tmpIndex">文本组件的索引位置</param>
        /// <param name="exp">要显示的表达式数据</param>
        private void SetExpressionText(int tmpIndex, Expression exp)
        {
            // 边界检查：确保索引有效
            if (tmpIndex < 0 || tmpIndex >= expressionTMPs.Count)
            {
                return;
            }

            string expressionText = "";

            // 第一步：显示左侧操作数
            if (exp.Left >= 0)
            {
                expressionText += $"{CardGroups[exp.Left].ShowValue()}";
            }

            // 第二步：显示运算符
            if (!(exp.Operator is null))
            {
                expressionText += $" {exp.Operator.GetSymbol()}";
            }

            // 第三步：显示右侧操作数和计算结果
            if (exp.Right >= 0)
            {
                // 预计算结果以决定显示格式
                float testCalculate = exp.Operator
                    .Calculate(CardGroups[exp.Left].GetValue(), CardGroups[exp.Right].GetValue());
                    
                if (Utils.IsInteger(testCalculate)) // 结果是整数
                {
                    expressionText +=
                        $" {CardGroups[exp.Right].ShowValue()} = {(int)(exp.Operator.Calculate(CardGroups[exp.Left].GetValue(), CardGroups[exp.Right].GetValue()))}";
                }
                else // 结果需要用分数形式显示
                {
                    // 使用分数计算避免浮点精度问题
                    Operator.Fraction fraction = exp.Operator.CalculateFraction(CardGroups[exp.Left].fractionValue,
                        CardGroups[exp.Right].fractionValue, exp.Operator.name);
                    expressionText +=
                        $" {CardGroups[exp.Right].ShowValue()} = {fraction.Numerator.ToString()}/{fraction.Denominator.ToString()}";
                }
            }

            // 更新UI文本显示
            expressionTMPs[tmpIndex].text = expressionText;
        }
        
        /// <summary>
        /// 执行当前表达式的运算并进入下一轮
        /// 
        /// 核心运算流程：
        /// 1. 执行数学计算（浮点数和分数两套体系）
        /// 2. 合并卡牌（右侧卡牌吸收左侧卡牌）
        /// 3. 更新视觉显示
        /// 4. 记录运算历史
        /// 5. 准备下一轮运算
        /// 
        /// 卡牌合并机制：
        /// - 左侧卡牌被"吃掉"，数据转移到右侧卡牌
        /// - 右侧卡牌显示新的计算结果
        /// - 保持卡牌总数递减：4→3→2→1
        /// 
        /// 双重计算系统：
        /// - 浮点数：用于快速计算和比较
        /// - 分数：用于精确显示，避免精度问题
        /// </summary>
        private void NextExpression()
        {
            // 获取参与运算的两个卡牌组
            CardGroup left = CardGroups[currentExpression.Left], right = CardGroups[currentExpression.Right];
            
            // 执行浮点数计算
            currentExpression.Result = currentExpression.Operator.Calculate(left.GetValue(), right.GetValue());
            
            // 核心操作：卡牌合并
            right.Append(left, currentExpression.Result);  // 右侧卡牌吸收左侧卡牌
            
            // 执行分数计算（精确计算）
            right.fractionValue = currentExpression.Operator.CalculateFraction(left.fractionValue,right.fractionValue,currentExpression.Operator.name);
            
            // 更新右侧卡牌的显示值
            right.ShowValueTag();
            
            // 取消左侧卡牌的高亮（因为已被合并）
            CardGroups[currentExpression.Left].SetHighlighted(false);

            // 记录这次运算到历史列表
            expressions.Add(currentExpression);
            
            // 重置当前表达式，准备下一轮
            currentExpression = new Expression
            {
                Left = -1,
                Right = -1
            };
        }
        
        /// <summary>
        /// 检查游戏是否完成并验证答案
        /// 
        /// 胜利条件：
        /// 1. 完成所有3次运算（4张卡→1张卡）
        /// 2. 最终结果在24的容错范围内（±0.01）
        /// 
        /// 容错设计：
        /// - 使用0.01的误差范围处理浮点数精度问题
        /// - 避免因计算精度导致的判断错误
        /// 
        /// 游戏结束处理：
        /// - 调用InGameManager.ShowResult()显示结果
        /// - 触发后续的奖励、音效、动画等
        /// 
        /// 返回值：
        /// - true: 游戏胜利完成
        /// - false: 游戏继续进行
        /// </summary>
        /// <returns>是否成功完成24点挑战</returns>
        private bool CheckAnswer()
        {
            // 检查是否完成所有运算且结果正确
            if (expressions.Count == 3 && Math.Abs(expressions[2].Result - 24) < 0.01)
            {
                InGameManager.ShowResult(true);  // 显示胜利结果
                return true;
            }
            return false;  // 游戏继续
        }
    }
}