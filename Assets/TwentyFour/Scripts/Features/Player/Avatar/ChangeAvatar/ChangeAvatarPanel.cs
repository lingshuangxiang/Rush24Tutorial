using System;
using System.Collections;
using DG.Tweening;                              // DOTween动画库
using UnityEngine;
using UnityEngine.UI;
using TwentyFour.Scripts.Art.UIEffect;          // 自定义UI特效

namespace TwentyFour.Scripts.Features.Player
{
    /// <summary>
    /// 头像更换面板控制器
    /// 
    /// 功能职责：
    /// 1. 管理头像更换界面的显示和隐藏
    /// 2. 播放精美的开场动画效果
    /// 3. 处理用户交互和面板关闭逻辑
    /// 4. 提供回调机制通知外部系统
    /// 
    /// 动画流程：
    /// 面板激活 → 帷幕展开 → 帷幕震动 → 光效出现 → 粒子特效 → 帷幕收起 → 显示退出按钮
    /// 
    /// 这是一个典型的UI动画展示类，使用DOTween实现流畅的视觉效果
    /// </summary>
    public class ChangeAvatarPanel : MonoBehaviour
    {
        [Header("核心组件")]
        public PlayerCharatorManager CharatorManager;   // 玩家角色管理器（处理头像数据）

        [Header("动画UI元素")]
        public RectTransform Light;                     // 光效元素（旋转光圈）
        public RectTransform DrapeL;                    // 左侧帷幕
        public RectTransform DrapeR;                    // 右侧帷幕  
        public RectTransform Drapes;                    // 整体帷幕容器
        public ParticleSystem StartEffect;             // 开场粒子特效

        [Header("交互组件")]
        public Button ExitButton;                       // 退出按钮

        [Header("事件回调")]
        public Action OnExit;                           // 面板关闭时的回调事件

        /// <summary>
        /// 初始化面板
        /// 
        /// 目前为空实现，预留用于面板的初始化设置
        /// 可以在这里进行：
        /// - UI元素的初始状态设置
        /// - 数据绑定
        /// - 事件监听器注册等
        /// </summary>
        public void Init()
        {
            // TODO: 添加面板初始化逻辑
        }

        /// <summary>
        /// 播放面板开场动画效果
        /// 
        /// 停止所有正在进行的协程，然后启动新的动画序列
        /// 确保每次播放都是完整的动画流程
        /// </summary>
        public void PlayEffect()
        {
            // 停止所有正在运行的协程，避免动画冲突
            StopAllCoroutines();
            
            // 启动新的动画播放协程
            StartCoroutine(Play());
        }

        /// <summary>
        /// 主要的动画播放协程
        /// 
        /// 完整的动画序列包括以下步骤：
        /// 1. 隐藏退出按钮，准备开场
        /// 2. 左右帷幕从小到大展开（创造神秘感）
        /// 3. 帷幕震动效果（增加戏剧性）
        /// 4. 光效出现并开始旋转（营造魔法感）
        /// 5. 粒子特效播放（增强视觉冲击）
        /// 6. 帷幕收起，露出内容
        /// 7. 显示退出按钮并设置点击事件
        /// 
        /// 整个动画大约持续4-5秒，营造仪式感和惊喜感
        /// </summary>
        IEnumerator Play()
        {
            // === 第一阶段：准备阶段 ===
            // 隐藏退出按钮，确保用户专注于动画
            ExitButton.gameObject.SetActive(false);
            
            // 等待1秒，给用户一个心理准备时间
            yield return new WaitForSeconds(1);

            // === 第二阶段：帷幕展开 ===
            // 左右帷幕同时从0.2倍缩放展开到1倍（0.5秒内完成）
            // From(0.2f)表示从0.2倍开始，到1倍结束
            DrapeL.DOScaleX(1, 0.5f).From(0.2f);    // 左帷幕展开
            DrapeR.DOScaleX(1, 0.5f).From(0.2f);    // 右帷幕展开
            
            // 等待帷幕展开完成
            yield return new WaitForSeconds(0.5f);

            // === 第三阶段：帷幕震动特效 ===
            // 使用自定义扩展方法让帷幕进行Z轴旋转震动
            // strength: 5 表示震动强度为5度
            Drapes.DoCommonShakeRotationZ(strength: 5);
            
            // 等待震动效果完成
            yield return new WaitForSeconds(1f);

            // === 第四阶段：光效登场 ===
            // 激活光效GameObject
            Light.gameObject.SetActive(true);
            
            // 播放粒子特效，增强视觉冲击
            StartEffect.Play();
            
            // 光效从0缩放到1，持续0.5秒
            Light.DOScale(1, 0.5f).From(0);
            
            // 光效开始无限循环旋转（360度/10秒，线性缓动）
            Light.DOLocalRotate(new Vector3(0, 0, 360), 10, RotateMode.FastBeyond360)
                .From(Vector3.zero)
                .SetEase(Ease.Linear)        // 线性旋转，保持匀速
                .SetLoops(-1);               // 无限循环

            // === 第五阶段：帷幕收起 ===
            // 左右帷幕同时收缩到0.2倍，露出内容
            DrapeL.DOScaleX(0.2f, 0.3f).From(1);    // 左帷幕收起
            DrapeR.DOScaleX(0.2f, 0.3f).From(1);    // 右帷幕收起
            
            // 等待帷幕收起和额外的展示时间
            yield return new WaitForSeconds(1f);

            // === 第六阶段：显示交互元素 ===
            // 显示退出按钮，允许用户交互
            ExitButton.gameObject.SetActive(true);
            
            // 清除之前的点击事件监听器，避免重复绑定
            ExitButton.onClick.RemoveAllListeners();
            
            // 绑定退出按钮点击事件
            ExitButton.onClick.AddListener(() =>
            {
                // 隐藏整个面板
                gameObject.SetActive(false);
                
                // 触发退出回调事件，通知外部系统面板已关闭
                OnExit?.Invoke();
                
                // 清空回调引用，防止内存泄漏
                OnExit = null;
            });

            // 退出按钮文字淡入效果（从透明到不透明，1秒内完成）
            ExitButton.GetComponentInChildren<Text>().DOFade(1, 1f).From(0);
        }

        /// <summary>
        /// Unity生命周期 - 对象每次被激活时调用
        /// 
        /// 每当这个面板被设置为活动状态时，自动播放开场动画
        /// 确保用户每次看到面板都有完整的视觉体验
        /// </summary>
        private void OnEnable()
        {
            // 面板激活时立即播放动画效果
            PlayEffect();
        }
    }
}