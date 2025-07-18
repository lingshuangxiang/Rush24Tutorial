using UnityEngine;
using UnityEngine.UI;
using TwentyFour.Scripts.Utilities;                    // 自定义颜色和工具类

namespace TwentyFour.Scripts.Gameplay.CardSystem
{
    /// <summary>
    /// 单张卡牌的显示和管理组件
    /// 
    /// 核心功能：
    /// 1. 管理扑克牌的视觉显示（花色、数字、颜色）
    /// 2. 根据卡牌数据模型动态加载对应的图片资源
    /// 3. 处理卡牌的位置和动画变换
    /// 4. 提供标准扑克牌的显示规则（红桃♥️方块♦️为红色，黑桃♠️梅花♣️为黑色）
    /// 
    /// 设计思路：
    /// 每张卡牌包含数据模型(Card)和视觉表现(UI组件)
    /// 数据驱动UI，支持动态换肤和主题切换
    /// 
    /// 注释：代码中预留了拖拽接口，但当前版本未启用
    /// </summary>
    public class GivenCard : MonoBehaviour
    //,IInitializePotentialDragHandler,IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("卡牌数据模型")]
        [SerializeField] public Card cardModel;            // 卡牌的数据模型（花色、数字等）
        
        [Header("UI显示组件")]
        [SerializeField] public GameObject cardNum;        // 卡牌数字显示对象
        public GameObject cardFaceGo;                      // 卡牌背景/花色显示对象
        private Image cardFaceRenderer;                    // 卡牌背景图片渲染器
        private Text cardNumTextMeshPro;                   // 卡牌数字文本组件

        [Header("拖拽相关（预留）")]
        private Vector3 offset;                            // 拖拽偏移量（当前未使用）
        private Vector3 originalPos;                       // 原始位置（当前未使用）

        [Header("位置和动画控制")]
        public RectTransform CardRect;                     // 卡牌的RectTransform（用于位置和动画控制）
        public RectTransform CardStart;                    // 卡牌起始位置参考点
        public RectTransform CardEnd;                      // 卡牌结束位置参考点


        /// <summary>
        /// Unity生命周期 - 脚本启动时调用
        /// 
        /// 当前为空实现，预留用于初始化逻辑
        /// 实际的卡牌初始化通过Init()方法进行
        /// </summary>
        void Start()
        {
            // 预留初始化逻辑
        }

        /// <summary>
        /// 初始化卡牌显示
        /// 
        /// 根据传入的卡牌数据模型设置卡牌的视觉表现
        /// 这是卡牌显示的核心入口方法
        /// 
        /// 参数：
        /// card - 卡牌数据模型，包含花色和数字信息
        /// 
        /// 执行流程：
        /// 1. 保存卡牌数据模型
        /// 2. 调用LoadCardFace()加载视觉资源
        /// </summary>
        public void Init(Card card)
        {
            cardModel = card;
            LoadCardFace();
        }

        /// <summary>
        /// Unity生命周期 - 每帧更新
        /// 
        /// 当前为空实现，预留用于动画更新或状态检查
        /// </summary>
        void Update()
        {
            // 预留更新逻辑
        }

        /// <summary>
        /// 加载卡牌的视觉外观
        /// 
        /// 这是卡牌显示的核心方法，负责：
        /// 1. 构建资源路径并加载对应的花色图片
        /// 2. 设置卡牌数字的显示文本
        /// 3. 根据花色设置数字的颜色（红色花色用红色，黑色花色用黑色）
        /// 
        /// 资源命名规则：
        /// - 路径格式："Easter/card_" + 花色名称
        /// - 例如：Heart → "Easter/card_Heart"
        /// 
        /// 颜色规则（标准扑克牌）：
        /// - 红桃(Heart)和方块(Diamond)：红色文字
        /// - 黑桃(Spade)和梅花(Club)：黑色文字
        /// </summary>
        public void LoadCardFace()
        {
            // 构建卡牌背景图片的资源路径
            string path = "Easter/card_";
            path += cardModel.suit.ToString();  // 添加花色名称（Heart、Diamond、Spade、Club）
            
            // 获取并设置卡牌数字显示组件
            cardNumTextMeshPro = cardNum.GetComponent<Text>(); 
            cardNumTextMeshPro.text = cardModel.text;  // 设置数字文本（A、2-10、J、Q、K）

            // 根据花色设置数字颜色（遵循标准扑克牌颜色规则）
            if(cardModel.suit == Suit.Heart || cardModel.suit == Suit.Diamond)
            {
                // 红桃♥️和方块♦️使用红色
                cardNumTextMeshPro.color = CustomColors.CardRed;
            }
            else
            {
                // 黑桃♠️和梅花♣️使用黑色
                cardNumTextMeshPro.color = CustomColors.CardDark;
            }
            
            // 加载并设置卡牌背景图片
            cardFaceRenderer = cardFaceGo.GetComponent<Image>();
            cardFaceRenderer.sprite = Resources.Load(path, typeof(Sprite)) as Sprite;
        }
        
    }
}