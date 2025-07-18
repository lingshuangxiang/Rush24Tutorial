using System.Collections;
using DG.Tweening;                                      // DOTween动画库
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Logger = TwentyFour.Scripts.Utilities.Logger;
using UnityEngine.UI;
using TwentyFour.Scripts.Gameplay.HomePage;             // 主页路由系统
using TwentyFour.Scripts.Utilities;                     // 工具类（音效管理等）
using TwentyFour.Scripts.Art.UIEffect;                  // UI特效系统

namespace TwentyFour.Scripts.Gameplay.GameMode.StageMode
{
    /// <summary>
    /// 闯关模式游戏内管理器
    /// 
    /// 核心职责：
    /// 1. 管理闯关游戏的整体流程（开始 → 游戏中 → 结束）
    /// 2. 处理游戏成功后的奖励展示和数据保存
    /// 3. 控制关卡切换和游戏退出逻辑
    /// 4. 管理结算界面的UI动画和交互
    /// 
    /// 工作流程：
    /// 进入关卡 → 显示关卡信息 → 游戏进行 → 成功后展示结果 → 保存进度 → 下一关或退出
    /// 
    /// 设计模式：单例模式，全局唯一访问点
    /// </summary>
    public class InGameManager : MonoBehaviour
    {
        [Header("游戏界面组件")]
        [SerializeField] public GameObject BattlePage;         // 游戏对战页面
        [SerializeField] public GameObject ResultPopup;        // 结果弹窗

        [Header("结果界面UI组件")]
        public GameObject ResultExitButton;                    // 结果页退出按钮
        public GameObject NextStageButton;                     // 下一关按钮
        public GameObject ResultPlaceholder;                   // 结果占位符（显示加载文本）
        public GameObject RewardDetail;                        // 奖励详情面板
        
        [Header("关卡信息显示")]
        [SerializeField] public Text StageTMP;                 // 当前关卡数显示

        [Header("奖励动画组件")]
        public Image RewardLightImage;                         // 奖励光效图片
        public ParticleSystem RewardParticle;                 // 奖励粒子特效
        [SerializeField] public Text RewardQuantityTMP;       // 奖励数量文本

        [Header("静态数据管理")]
        public static Stage currentStage;                      // 当前游戏关卡（静态，跨脚本访问）
        public static InGameManager instance;                  // 单例实例（全局访问点）
        
        [Header("事件系统")]
        public UnityEvent SuccessEvent = new UnityEvent(){};   // 成功事件（其他系统可监听）
        /// <summary>
        /// 结果占位符文本的延迟获取属性
        /// 
        /// 使用属性模式实现延迟初始化，避免在Awake/Start中获取组件可能出现的空引用问题
        /// 只有在第一次访问时才会获取Text组件，并缓存结果
        /// </summary>
        private Text _resultPlaceholderText;
        private Text resultPlaceholderText
        {
            get
            {
                if (_resultPlaceholderText == null)
                {
                    _resultPlaceholderText = ResultPlaceholder.GetComponent<Text>();
                }
                return _resultPlaceholderText;
            }
        }

        [Header("弹窗管理")]
        [SerializeField] private GameObject quitPopup;          // 退出确认弹窗
        
        /// <summary>
        /// Unity生命周期 - 对象实例化时调用
        /// 
        /// 设置单例实例，确保全局只有一个InGameManager
        /// 其他脚本可通过InGameManager.instance访问此管理器
        /// </summary>
        private void Awake()
        {
            instance = this;
        }
        
        /// <summary>
        /// Unity生命周期 - 脚本启动时调用
        /// 
        /// 初始化当前关卡：
        /// 1. 从StageManager获取下一个要玩的关卡
        /// 2. 设置并显示关卡信息
        /// </summary>
        void Start()
        {
            SetCurrentStage(StageManager.NextStage());
        }
        
        /// <summary>
        /// 退出对战的公共接口
        /// 
        /// 供UI按钮调用，直接调用ExitGame方法返回主界面
        /// </summary>
        public void ExitBattle()
        {
            ExitGame();
        }

        /// <summary>
        /// 设置当前关卡并更新UI显示
        /// 
        /// 参数：
        /// s - 要设置的关卡对象
        /// 
        /// 功能：
        /// 1. 更新静态变量currentStage（供其他脚本访问）
        /// 2. 更新界面上的关卡数显示（从0开始的索引转换为从1开始的关卡号）
        /// </summary>
        void SetCurrentStage(Stage s)
        {
            currentStage = s;
            StageTMP.text = (s.index + 1).ToString();   // 显示给玩家的关卡号从1开始
        }

        /// <summary>
        /// 重置结果弹窗到初始状态
        /// 
        /// 准备显示游戏结果时调用，确保UI状态正确：
        /// 1. 显示等待奖励的提示文本
        /// 2. 隐藏下一关按钮（等待奖励处理完成后显示）
        /// 3. 显示占位符，隐藏奖励详情
        /// 4. 隐藏奖励光效
        /// </summary>
        void ResetResultPopup()
        {
            resultPlaceholderText.text = "等待奖励进口袋...";
            NextStageButton.SetActive(false);
            ResultPlaceholder.SetActive(true);
            RewardDetail.SetActive(false);
            RewardLightImage.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 显示奖励详情和动画效果
        /// 
        /// 参数：
        /// newReward - 是否是新获得的奖励（首次通关为true，重复通关为false）
        /// 
        /// 功能：
        /// 1. 激活并播放奖励光效和粒子特效
        /// 2. 根据是否首次通关显示不同的UI和文案
        /// 3. 显示下一关按钮，允许玩家继续游戏
        /// 
        /// 视觉效果：
        /// - 首次通关：完整的奖励动画 + 奖励详情
        /// - 重复通关：简化显示 + "领过奖励啦"提示
        /// </summary>
        void DisplayRewardDetail(bool newReward = true)
        {
            // 激活奖励光效并播放粒子特效
            RewardLightImage.gameObject.SetActive(true);
            RewardParticle.Play();
            
            if (newReward)
            {
                // 首次通关：显示完整奖励详情
                RewardDetail.SetActive(true);
                ResultPlaceholder.SetActive(false);
                
                // 奖励光效淡入动画（从透明到不透明，0.5秒，线性缓动）
                RewardLightImage.DOFade(1, 0.5f).From(0).SetEase(Ease.Linear);
            }
            else
            {
                // 重复通关：显示已领取提示
                resultPlaceholderText.text = "领过奖励啦";
            }
            
            // 显示下一关按钮，允许玩家继续
            NextStageButton.SetActive(true);
        } 

        /// <summary>
        /// 显示游戏结果的核心方法
        /// 
        /// 参数：
        /// result - 游戏是否成功（true为成功，false为失败）
        /// 
        /// 执行流程：
        /// 1. 触发成功事件（通知其他系统）
        /// 2. 重置并显示结果弹窗
        /// 3. 播放胜利音效
        /// 4. 根据是否首次通关决定奖励处理流程
        /// 
        /// 首次通关：UI动画结束后保存数据并给奖励
        /// 重复通关：直接显示已领取状态
        /// </summary>
        public void ShowResult(bool result)
        {
            // 只处理成功的情况，失败直接返回
            if (!result) return;
            
            // 触发成功事件，其他系统可以监听此事件做相应处理
            SuccessEvent?.Invoke();
            
            // 重置结果弹窗到初始状态
            ResetResultPopup();
            
            // 播放关卡胜利音效
            BGMManager.Instance.PlayAFX(AFXMusic.StageWin);
            
            // 显示结果弹窗
            ResultPopup.SetActive(true);
            
            // 检查是否首次通关（分数为0表示未通关过）
            if (StageManager.playerStageScores[currentStage.index] == 0)
            {
                // 首次通关：等待UI动画结束后处理数据保存和奖励
                var uiEffect = ResultPopup.GetComponent<UIEffect>();
                if (uiEffect != null)
                {
                    // 清除之前的监听器，避免重复绑定
                    uiEffect.OnEnd.RemoveAllListeners();
                    
                    // 绑定UI动画结束事件，动画完成后执行数据上传
                    uiEffect.OnEnd.AddListener(UploadUserData);
                }
            }
            else
            {
                // 重复通关：直接显示已领取奖励状态
                DisplayRewardDetail(false);
            }
        }

        /// <summary>
        /// 上传用户数据的统一入口
        /// 
        /// 当首次通关时调用，并行执行两个操作：
        /// 1. 保存游戏进度到云端
        /// 2. 处理奖励发放流程
        /// 
        /// 使用协程确保数据处理的异步性，不阻塞主线程
        /// </summary>
        void UploadUserData()
        {
            StartCoroutine(SaveProgressToCloud());
            StartCoroutine(GetReward());
        }

        /// <summary>
        /// 保存游戏进度到云端的协程
        /// 
        /// 功能：
        /// 1. 将当前关卡标记为已完成（分数设为1）
        /// 2. 触发StageManager的数据保存机制
        /// 
        /// 注意：yield break表示协程立即结束，实际保存逻辑在StageManager中处理
        /// </summary>
        IEnumerator SaveProgressToCloud()
        {
            // 设置当前关卡的完成分数为1（表示已通关）
            StageManager.SetStageScore(1);
            yield break;    // 立即结束协程
        }

        /// <summary>
        /// 获取奖励的协程
        /// 
        /// 功能：
        /// 1. 等待1秒（给玩家一个心理期待时间）
        /// 2. 显示奖励详情和动画效果
        /// 
        /// 这个延时增强了获得奖励的仪式感和满足感
        /// </summary>
        IEnumerator GetReward()
        {
            // 等待1秒，营造奖励发放的期待感
            yield return new WaitForSeconds(1);
            
            // 显示奖励详情（首次获得）
            DisplayRewardDetail();
        }
        
        /// <summary>
        /// 进入下一关的方法
        /// 
        /// 供"下一关"按钮调用，处理关卡切换逻辑：
        /// 1. 获取下一个关卡数据
        /// 2. 隐藏结果弹窗
        /// 3. 如果没有更多关卡则退出游戏，否则开始新关卡
        /// 
        /// 关卡切换是无缝的，玩家可以连续游戏
        /// </summary>
        public void NextRound()
        {
            // 获取下一个关卡（偏移量为1，表示下一关）
            var nextStage = StageManager.NextStage(1);
            
            // 隐藏结果弹窗，准备开始新关卡
            ResultPopup.SetActive(false);

            // 检查是否还有更多关卡
            if (nextStage is null)
            {
                // 没有更多关卡，退出到主界面
                ExitGame();
                return;
            }
            
            // 设置新的关卡并开始游戏
            SetCurrentStage(nextStage);
        }

        /// <summary>
        /// 退出游戏，返回主界面
        /// 
        /// 供多个地方调用：
        /// 1. 手动退出按钮
        /// 2. 所有关卡完成后自动退出
        /// 3. 其他需要返回主界面的场景
        /// 
        /// 使用GameRouter确保场景切换的一致性
        /// </summary>
        public void ExitGame()
        {
            GameRouter.LoadHomeScene();
        }
    }
}