using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;                               // Unity事件系统，用于处理点击事件
using UnityEngine.UI;
using TwentyFour.Scripts.Gameplay.GameMode.StageMode;        // 答案管理器
using TwentyFour.Scripts.Utilities;                          // 按钮适配器工具

namespace TwentyFour.Scripts.Gameplay.CardSystem
{
    /// <summary>
    /// 运算符按钮交互控制器
    /// 
    /// 核心功能：
    /// 1. 显示运算符符号和图标（+、-、×、÷）
    /// 2. 处理玩家点击运算符的交互逻辑
    /// 3. 管理运算符按钮的选中/取消选中状态
    /// 4. 与AnswerManager通信，传递玩家的运算符选择
    /// 
    /// 交互流程：
    /// 玩家点击运算符 → 按钮状态变化 → 通知AnswerManager → 更新游戏逻辑
    /// 
    /// 实现接口：IPointerDownHandler（Unity事件系统）
    /// </summary>
    public class GivenOperator : MonoBehaviour, IPointerDownHandler
    {
        [Header("UI显示组件")]
        public TextMeshProUGUI textObject;                    // 运算符文本显示（+、-、×、÷）

        [Header("核心数据")]
        [SerializeField] public AnswerManager answerManager;  // 答案管理器引用，处理运算逻辑
        [SerializeField] public Operator operatorModel;      // 运算符数据模型（包含计算逻辑）

        [Header("按钮组管理")]
        private ButtonTextAdaptor[] btns;                     // 所有运算符按钮的适配器数组

        [Header("图标系统")]
        public Image Icon;                                    // 运算符图标显示
        public List<Sprite> IconSprites;                     // 运算符图标精灵列表（按枚举顺序排列）
        /// <summary>
        /// Unity生命周期 - 脚本启动时调用
        /// 
        /// 初始化运算符按钮：
        /// 1. 获取父级容器下所有的按钮适配器（用于统一状态管理）
        /// 2. 根据运算符类型设置对应的图标
        /// 
        /// 设计思路：所有运算符按钮作为一个按钮组，同时只能选中一个
        /// </summary>
        void Start()
        {
            // 获取父级容器下所有的按钮适配器组件
            // 这样可以统一管理所有运算符按钮的选中状态
            btns = transform.parent.GetComponentsInChildren<ButtonTextAdaptor>();
            
            // 根据运算符类型设置对应的图标
            // operatorModel.name是枚举类型，可以直接转换为数组索引
            Icon.sprite = IconSprites[(int)operatorModel.name];
        }

        /// <summary>
        /// Unity生命周期 - 每帧更新
        /// 
        /// 实时更新运算符文本显示
        /// 虽然运算符符号通常不会改变，但这确保了UI与数据模型的同步
        /// 
        /// 注意：在实际项目中，可以优化为只在需要时更新，而不是每帧更新
        /// </summary>
        void Update()
        {
            // 从运算符数据模型获取符号并显示
            textObject.text = operatorModel.GetSymbol();
        }

        /// <summary>
        /// 实现IPointerDownHandler接口 - 处理鼠标/触摸按下事件
        /// 
        /// 这是运算符选择的核心逻辑：
        /// 1. 向答案管理器添加当前运算符
        /// 2. 更新所有运算符按钮的选中状态（单选逻辑）
        /// 3. 处理重复点击的取消选择逻辑
        /// 
        /// 交互逻辑：
        /// - 点击未选中的运算符：选中当前，取消其他
        /// - 点击已选中的运算符：取消选择
        /// 
        /// 参数：
        /// eventData - Unity事件系统提供的点击事件数据
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            // 第一步：向答案管理器添加当前运算符
            // AnswerManager会处理这个运算符在24点计算中的逻辑
            answerManager.AddOperator(operatorModel);
            
            // 第二步：遍历所有运算符按钮，实现单选逻辑
            foreach (var btn in btns)
            {
                if (btn.gameObject != gameObject)
                {
                    // 其他按钮：强制取消选中状态
                    btn.OnDeselect();
                }
                else
                {
                    // 当前按钮：处理选中/取消选中的切换逻辑
                    if (btn.isSelected)
                    {
                        // 如果当前按钮已经被选中，再次点击则取消选择
                        // 同时通知答案管理器移除这个运算符
                        answerManager.RemoveOperator(operatorModel);
                    }
                    
                    // 切换当前按钮的选中状态
                    // OnSelect()内部会检查当前状态并进行相应的切换
                    btn.OnSelect();
                }
            }
        }
    }
}