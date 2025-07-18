using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TwentyFour.Scripts.Utilities;

namespace TwentyFour.Scripts.Gameplay.HomePage
{
    /// <summary>
    /// 游戏初始化状态枚举
    /// 定义主界面可能存在的各种游戏状态，主要用于对战模式的状态管理
    /// </summary>
    public enum InitState
    {
        None,                   // 无特殊状态，正常的主界面状态
        OpponentQuits,          // 对手退出游戏（对战过程中或匹配过程中）
        MatchAgain,             // 请求再次匹配（上一局游戏结束后）
        ShowTournamentPanel     // 显示锦标赛面板（比赛模式相关）
    }

    /// <summary>
    /// 主界面游戏初始化管理器
    /// 
    /// 功能职责：
    /// 1. 管理主界面的初始化状态和流程
    /// 2. 处理对战模式的各种状态变化
    /// 3. 管理匹配系统的状态和UI响应
    /// 4. 处理玩家重新匹配和对手退出等情况
    /// 
    /// 生命周期：
    /// Awake() → Start() → OnEnable() → 根据InitState执行对应逻辑
    /// 
    /// 注意：这是一个状态管理器，主要服务于多人对战功能
    /// </summary>
    public class GameInitManagerLocal : MonoBehaviour
    {
        [Header("静态状态管理")]
        // 全局游戏初始化状态，跨场景保持
        public static InitState MyInitState = InitState.None;
        
        // 是否是自定义再次游戏请求的发送者
        public static bool IsCustomOnceMoreSender;
        
        // 是否收到自定义再次游戏的响应
        public static bool ReceiveCustomOnceMoreResponse;
        
        // 上一次游戏房间的ID（用于重连或再次匹配）
        public static string PreviousRoomId;

        [Header("UI组件引用")]
        public GameObject matchCanvas;      // 匹配界面画布················
        public Button matchButton;         // 匹配按钮
        public Button BattleButton;        // 对战按钮

        [Header("事件系统")]
        public UnityEvent OnStartEvent;    // 启动时触发的事件

        /// <summary>
        /// Unity生命周期 - 对象实例化时调用
        /// 
        /// 目前为空实现，预留用于早期初始化逻辑
        /// 可以在这里进行不依赖其他组件的初始化工作
        /// </summary>
        void Awake()
        {
            // TODO: 可以在这里添加早期初始化逻辑
        }

        /// <summary>
        /// Unity生命周期 - 脚本启动时调用
        /// 
        /// 执行主界面的基础初始化：
        /// 1. 触发启动事件，通知其他系统主界面已准备就绪
        /// 2. 重置自定义匹配发送者状态
        /// </summary>
        private void Start()
        {
            // 触发启动事件，其他系统可以监听此事件来执行相应的初始化
            OnStartEvent?.Invoke();
            
            // 重置自定义再次游戏的发送者状态
            // 确保每次进入主界面时状态是干净的
            IsCustomOnceMoreSender = false;
        }

        /// <summary>
        /// Unity生命周期 - 对象每次被激活时调用
        /// 
        /// 这是状态处理的核心方法，根据全局InitState执行相应的逻辑：
        /// - MatchAgain: 处理再次匹配请求
        /// - OpponentQuits: 处理对手退出情况
        /// - ShowTournamentPanel: 显示锦标赛界面
        /// 
        /// 每次处理完状态后都会重置为None，避免重复处理
        /// </summary>
        public void OnEnable()
        {
            // 检查当前的初始化状态并执行相应逻辑
            if (MyInitState == InitState.MatchAgain)
            {
                // 处理再次匹配的情况
                // TODO: 这里可以添加重新开始匹配的逻辑
                // 比如自动重新匹配或显示匹配选项
                
                // 处理完成后重置状态
                MyInitState = InitState.None;
            }
            else if (MyInitState == InitState.OpponentQuits)
            {
                // 对手退出时显示提示信息
                UIMessage.Show("对手退出！");
                
                // 注释的代码是另一种显示提示的方式
                //StartCoroutine(myMatchMakingManager.ShowHint("对手退出！"));
                
                // 处理完成后重置状态
                MyInitState = InitState.None;
            }
            else if (MyInitState == InitState.ShowTournamentPanel)
            {
                // 显示锦标赛相关界面
                StartCoroutine(Tournament());
                
                // 处理完成后重置状态
                MyInitState = InitState.None;
            }
        }

        /// <summary>
        /// 锦标赛模式协程
        /// 
        /// 处理锦标赛相关的UI显示和逻辑
        /// 
        /// 参数：
        /// matchAgain - 是否是再次匹配的情况
        /// 
        /// 目前为空实现，可能在未来版本中添加锦标赛功能
        /// </summary>
        IEnumerator Tournament(bool matchAgain = false)
        {
            // 等待一帧，确保UI更新完成
            yield return null;
            
            // TODO: 在这里添加锦标赛相关逻辑
            // 比如：
            // - 显示锦标赛排行榜
            // - 处理锦标赛匹配
            // - 显示锦标赛奖励
        }
    }
}