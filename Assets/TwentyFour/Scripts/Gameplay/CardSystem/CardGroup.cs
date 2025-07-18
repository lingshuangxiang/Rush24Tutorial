using System.Collections.Generic;
using DG.Tweening;                                      // DOTween动画库
using UnityEngine;
using UnityEngine.EventSystems;                         // Unity事件系统
using UnityEngine.UI;
using TwentyFour.Scripts.Utilities;                     // 工具类
using TwentyFour.Scripts.Gameplay.GameMode.StageMode;   // 游戏管理器

namespace TwentyFour.Scripts.Gameplay.CardSystem
{
    /// <summary>
    /// 卡牌组控制器 - 24点游戏的核心交互组件
    /// 
    /// 核心功能：
    /// 1. 管理单张或多张卡牌的组合（运算结果会合并多张卡牌）
    /// 2. 处理卡牌的点击交互和视觉反馈
    /// 3. 显示卡牌的数值（整数或分数形式）
    /// 4. 管理卡牌的动画效果（高亮、缩放、移动等）
    /// 5. 支持精确的分数计算，避免浮点数误差
    /// 
    /// 24点游戏流程：
    /// 4张卡牌 → 选择2张 + 运算符 → 3张卡牌 → 选择2张 + 运算符 → 2张卡牌 → 最终1张 = 24
    /// 
    /// 设计模式：观察者模式（委托事件）+ 组合模式（卡牌合并）
    /// </summary>
    public class CardGroup : MonoBehaviour, IPointerDownHandler
    {
        /// <summary>
        /// 卡牌组点击事件委托
        /// 当玩家点击卡牌组时，会传递卡牌组的索引给AnswerManager
        /// </summary>
        public delegate void CardGroupClick(int index);
        
        [Header("卡牌组基础数据")]
        [SerializeField] public int CardIndex;              // 卡牌组的索引（0-3，对应4个卡牌位置）
        [SerializeField] public GivenCard Original;         // 原始卡牌（初始显示的单张卡牌）
        [SerializeField] public GameObject AppendingParent; // 合并卡牌的父容器
        [SerializeField] public TempValueTag ValueTag;      // 数值标签（显示计算结果）
        
        [Header("卡牌组合系统")]
        public List<GivenCard> appendings;                  // 附加的卡牌列表（运算后合并的卡牌）
        public CardGroupClick onCardGroupClick;             // 点击事件回调
        
        [Header("数值计算系统")]
        private float currentValue;                          // 当前数值（浮点数表示）
        public Operator.Fraction fractionValue;             // 分数表示（精确计算，避免浮点误差）
        private float originValue;                           // 原始数值（备份用）
        private string markValue;                            // 标记值（可能是分数形式的字符串）
        
        [Header("交互状态管理")]
        private bool isLocked;                               // 是否锁定（锁定时不能点击）
        private Sequence aniSequence;                       // 主动画序列
        public Image highlightBorder;                       // 高亮边框

        [Header("视觉效果")]
        int degree = 0;                                      // 卡牌的随机旋转角度
        private Vector2 oriPos;                              // 原始位置
        /// <summary>
        /// Unity生命周期 - 脚本启动时调用
        /// 
        /// 初始化卡牌的视觉效果：
        /// 1. 生成随机旋转角度（-5到5度），让每张卡牌看起来更自然
        /// 2. 记录原始位置，用于动画恢复
        /// </summary>
        private void Start()
        {
            // 生成随机旋转角度，让卡牌显示更有趣味性
            degree = Random.Range(-5, 5);
            
            // 记录原始位置，用于动画恢复
            oriPos = transform.localPosition;
        }

        /// <summary>
        /// Unity生命周期 - 每帧更新
        /// 
        /// 预留用于高亮边框的呼吸效果
        /// 目前为空实现，可以添加边框闪烁、脉冲等效果
        /// </summary>
        private void Update()
        {
            //TODO: highlightborder breath effect
            // 可以在这里添加高亮边框的呼吸动画效果
        }

        /// <summary>
        /// 显示数值标签
        /// 
        /// 根据当前数值的类型（整数或分数）显示不同的格式：
        /// - 整数：直接显示数字（如 "6"）
        /// - 分数：显示分数形式（如 "2/3"）
        /// 
        /// 这是24点游戏精确计算的关键，避免了浮点数运算的精度问题
        /// </summary>
        public void ShowValueTag()
        {
            if (Utils.IsInteger(currentValue))
            {
                // 如果是整数，格式化为无小数点的数字显示
                ValueTag.SetText($"{currentValue:N0}");
            }
            else
            {
                // 如果是小数，显示对应的分数形式
                ValueTag.SetText($"{fractionValue.Numerator.ToString()}/{fractionValue.Denominator.ToString()}");
            }

            // 激活数值标签显示
            ValueTag.gameObject.SetActive(true);
        }

        /// <summary>
        /// 隐藏数值标签
        /// 
        /// 清空文本内容并隐藏标签GameObject
        /// 通常在卡牌重置或游戏重新开始时调用
        /// </summary>
        public void HideValueTag()
        {
            ValueTag.SetText("");
            ValueTag.gameObject.SetActive(false);
        }

        /// <summary>
        /// 合并两个卡牌组的核心方法
        /// 
        /// 当两张卡牌进行运算后，需要将结果合并到一个卡牌组中：
        /// 1. 更新当前卡牌组的数值为运算结果
        /// 2. 将另一个卡牌组的所有卡牌移动到当前组
        /// 3. 调整卡牌的层级和位置，实现视觉上的合并效果
        /// 4. 清空被合并的卡牌组
        /// 
        /// 参数：
        /// group2 - 要被合并的卡牌组
        /// newValue - 运算后的新数值
        /// 
        /// 这是24点游戏的关键机制：4张→3张→2张→1张
        /// </summary>
        public void Append(CardGroup group2, float newValue)
        {
            // 更新当前数值并显示
            currentValue = newValue;
            ShowValueTag();
            
            /////// 设置卡牌位置和层级 ////////
            // 收集要合并的所有卡牌（原始卡牌 + 附加卡牌）
            List<GivenCard> cards = new List<GivenCard> { group2.Original };
            cards.AddRange(group2.appendings);
            
            // 设置卡牌的父级和层级
            int i = appendings.Count;
            foreach (var card in cards)
            {
                // 将卡牌移动到当前组的容器中
                card.gameObject.transform.SetParent(AppendingParent.transform);
                
                // 保持原有的X、Y位置，但调整Z位置实现层级效果
                Vector3 pos = card.gameObject.transform.localPosition;
                card.gameObject.transform.localPosition = new Vector3(pos.x, pos.y, -0.01f * ++i);
            }
            //////////////////
            
            // 将合并的卡牌添加到当前组的附加列表中
            appendings.AddRange(cards);
            
            // 清空被合并的卡牌组，避免重复引用
            group2.SetEmpty();
        }

        /// <summary>
        /// 清空卡牌组，重置为空状态
        /// 
        /// 当卡牌组被合并到其他组后调用：
        /// 1. 隐藏数值标签
        /// 2. 清空附加卡牌列表
        /// 
        /// 这样可以避免同一张卡牌被多个组引用的问题
        /// </summary>
        public void SetEmpty()
        {
            HideValueTag();
            appendings = new List<GivenCard>();
        }

        /// <summary>
        /// 获取卡牌组的显示值
        /// 
        /// 根据数值类型返回合适的字符串表示：
        /// - 整数：直接返回数字字符串
        /// - 分数：返回"分子/分母"格式
        /// 
        /// 返回值：格式化后的数值字符串
        /// </summary>
        public string ShowValue()
        {
            if (Utils.IsInteger(GetValue()))
            {
                // 整数直接转换为字符串
                return GetValue().ToString();
            }
            else
            {
                // 分数格式："分子/分母"
                return fractionValue.Numerator.ToString() + "/" + fractionValue.Denominator.ToString();
            }
        }

        /// <summary>
        /// 获取当前卡牌组的数值
        /// 
        /// 返回值：当前的浮点数值
        /// </summary>
        public float GetValue()
        {
            return currentValue;
        }


        /// <summary>
        /// 重置卡牌到指定关卡的初始状态
        /// 
        /// 每次开始新关卡时调用，恢复卡牌组到原始状态：
        /// 1. 初始化原始卡牌的数据和显示
        /// 2. 隐藏数值标签和高亮效果
        /// 3. 重置位置、旋转、缩放等视觉属性
        /// 4. 设置初始数值（整数和分数形式）
        /// 5. 清空附加卡牌列表
        /// 
        /// 参数：
        /// stage - 当前关卡数据，包含卡牌配置
        /// </summary>
        public void ResetCard(Stage stage)
        {
            // 初始化原始卡牌的数据
            Original.Init(stage.question.cards[CardIndex]);
            
            // 重置UI状态
            HideValueTag();
            SetHighlighted(false);
            isLocked = false;
            gameObject.SetActive(true);
            
            // 重置卡牌的Transform层级关系
            var oTrans = Original.gameObject.transform;
            oTrans.SetParent(transform);
            oTrans.SetSiblingIndex(0);  // 设为第一个子对象
            oTrans.localPosition = Vector3.zero;

            // 重置卡牌的视觉属性（位置、旋转、缩放）
            Original.CardRect.DOMove(Original.CardStart.position, 0);      // 移动到起始位置
            Original.CardRect.DOLocalRotate(new Vector3(0, 0, degree), 0f); // 应用随机旋转
            Original.CardRect.DOScale(1.2f, 0f);                           // 设置默认缩放
            
            // 设置数值（同时设置浮点数和分数形式）
            currentValue = Original.cardModel.number;
            originValue = currentValue;  // 备份原始值
            fractionValue = new Operator.Fraction((int)originValue, 1);  // 转换为分数（如 5 = 5/1）
            
            // 清空附加卡牌列表
            appendings = new List<GivenCard>();
        }

        /// <summary>
        /// 初始化卡牌组（使用当前关卡）
        /// 
        /// 从InGameManager获取当前关卡数据并初始化
        /// 如果没有当前关卡（所有关卡已完成），则直接返回
        /// </summary>
        public void Init()
        {
            if (InGameManager.currentStage == null)
            {
                // 所有关卡已完成，无需初始化
                return;
            }

            ResetCard(InGameManager.currentStage);
        }

        /// <summary>
        /// 初始化卡牌组（使用指定关卡）
        /// 
        /// 参数：
        /// stage - 指定的关卡数据
        /// </summary>
        public void Init(Stage stage)
        {
            ResetCard(stage);
        }

        /// <summary>
        /// 锁定卡牌组
        /// 
        /// 锁定后玩家无法点击此卡牌组
        /// 通常在选择了第一张卡牌后，锁定该卡牌直到选择运算符
        /// </summary>
        public void Lock()
        {
            isLocked = true;
        }

        /// <summary>
        /// 解锁卡牌组
        /// 
        /// 解锁后玩家可以重新点击此卡牌组
        /// </summary>
        public void UnLock()
        {
            isLocked = false;
        }

        /// <summary>
        /// 附加动画序列列表
        /// 用于管理多个卡牌的同步动画效果
        /// </summary>
        List<Sequence> appendList = new List<Sequence>();

        /// <summary>
        /// 设置卡牌组的高亮状态
        /// 
        /// 这是24点游戏中重要的视觉反馈机制：
        /// 1. 高亮时：播放弹跳动画，移动到指定位置，显示边框
        /// 2. 取消高亮时：恢复原始大小和位置，隐藏边框
        /// 
        /// 动画效果：
        /// - 缩放动画：1.1x → 1.4x → 1.3x（营造弹跳感）
        /// - 位置移动：从起始位置移动到结束位置
        /// - 同步效果：原始卡牌和所有附加卡牌同步动画
        /// 
        /// 参数：
        /// v - true为高亮，false为取消高亮
        /// </summary>
        public void SetHighlighted(bool v = true)
        {
            // 控制高亮边框的显示
            if (highlightBorder)
            {
                highlightBorder.enabled = v;
            }

            var rect = Original.CardRect;
            
            if (v)
            {
                // === 高亮状态：播放弹跳动画 ===
                
                // 创建主卡牌的动画序列
                aniSequence = DOTween.Sequence();
                appendList?.Clear();
                
                // 弹跳动画：小→大→稍小（营造弹性感）
                aniSequence.Append(rect.DOScale(1.1f, 0.05f))
                    .Append(rect.DOScale(1.4f, 0.1f))
                    .Append(rect.DOScale(1.3f, 0.05f));

                // 为所有附加卡牌创建同步动画
                foreach (var card in appendings)
                {
                    var cardRect = card.CardRect;
                    Sequence seq = DOTween.Sequence();
                    seq.Append(cardRect.DOScale(1.1f, 0.05f))
                        .Append(cardRect.DOScale(1.4f, 0.1f))
                        .Append(cardRect.DOScale(1.3f, 0.05f));
                    appendList.Add(seq);
                    seq.Play();
                }
                aniSequence.Play();

                // 数值标签也做同样的弹跳动画
                var tempRect = ValueTag.GetComponent<RectTransform>();
                Sequence tempseq = DOTween.Sequence();
                tempseq.Append(tempRect.DOScale(1.1f, 0.05f))
                    .Append(tempRect.DOScale(1.4f, 0.1f))
                    .Append(tempRect.DOScale(1.3f, 0.05f));
                appendList.Add(tempseq);
                tempseq.Play();

                // 移动到高亮位置
                rect.DOMove(Original.CardEnd.position, 0);
            }
            else
            {
                // === 取消高亮：立即恢复原状 ===
                
                // 停止并清理所有正在进行的动画
                if (!(aniSequence is null) && aniSequence.IsActive())
                {
                    aniSequence.Kill();
                }

                foreach (var seq in appendList)
                {
                    seq.Kill();
                }
                appendList?.Clear();

                // 立即恢复到默认状态
                rect.DOScale(1.2f, 0f);                        // 恢复默认缩放
                rect.DOMove(Original.CardStart.position, 0);   // 恢复起始位置
            }
        }

        /// <summary>
        /// 实现IPointerDownHandler接口 - 处理鼠标/触摸点击事件
        /// 
        /// 24点游戏的核心交互入口：
        /// 1. 检查卡牌组是否被锁定（锁定时不响应点击）
        /// 2. 触发点击事件回调，将卡牌组索引传递给AnswerManager
        /// 
        /// 点击流程：
        /// 玩家点击卡牌 → 检查锁定状态 → 触发回调 → AnswerManager处理逻辑
        /// 
        /// 参数：
        /// eventData - Unity事件系统提供的点击事件数据
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            // 只有在未锁定且有回调监听器的情况下才响应点击
            if (!isLocked && onCardGroupClick != null)
            {
                // 触发点击事件，传递当前卡牌组的索引
                onCardGroupClick(CardIndex);
            }
        }
    }
}