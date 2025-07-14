using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = TwentyFour.Scripts.Utilities.Logger;
using TwentyFour.Scripts.Art.Effects;

namespace TwentyFour.Scripts.Gameplay.HomePage
{
    /// <summary>
    /// 游戏场景路由管理器
    /// 负责管理整个游戏的场景切换逻辑，是项目的导航中心
    /// 采用静态类设计，全局唯一，方便在任何地方调用场景切换
    /// </summary>
    public static class GameRouter
    {
        // 场景文件夹路径常量 - 所有游戏场景都存放在这个路径下
        public const string FolderPath = "TwentyFour/Scenes/";

        // 各个场景的名称常量定义
        public const string LoadingScene = "LoadingScene";      // 加载过渡场景
        public const string StartScene = "FirstInitScene";     // 游戏启动/登录场景  
        public const string MainScene = "MainScene";           // 主界面场景
        public const string BattleScene = "BattleScene";       // 对战模式场景
        public const string StageScene = "StageScene";         // 闯关模式场景

        // 全局状态标志
        public static bool isLoggingOut = false;               // 标记用户是否正在登出

        /// <summary>
        /// Unity运行时初始化方法
        /// 在游戏启动时自动调用，确保总是从正确的起始场景开始
        /// [RuntimeInitializeOnLoadMethod] 特性使此方法在游戏启动时自动执行
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        static void Initialize()
        {
            // 获取当前活动的场景
            Scene scene = SceneManager.GetActiveScene();
            
            // 如果当前场景不是起始场景，强制跳转到起始场景
            // 这确保了游戏总是从登录页面开始，避免直接进入其他场景的问题
            if (!scene.name.Equals(StartScene))
            {
                SceneManager.LoadScene(StartScene);
            }

            // TODO: 在这里初始化所有需要的SDK
            // 例如：登录SDK、分析SDK、广告SDK等
        }

        /// <summary>
        /// 加载过渡场景
        /// 通常用于显示加载动画或进度条
        /// </summary>
        public static void LoadLoadingScene()
        {
            SceneManager.LoadScene(FolderPath + LoadingScene);
        }

        /// <summary>
        /// 加载闯关模式游戏场景
        /// 玩家进入单人闯关模式时调用
        /// </summary>
        public static void LoadStageGameScene()
        {
            Logger.Log("Load Stage Game Scene");
            // 使用异步加载效果，提供平滑的场景切换体验
            // 参数：当前场景名、目标场景路径、是否保留当前场景数据
            AsyncLoadingSceneEffect.LoadScene(SceneManager.GetActiveScene().name, FolderPath + StageScene, false);
        }

        /// <summary>
        /// 加载对战模式游戏场景  
        /// 玩家进入多人对战模式时调用
        /// </summary>
        public static void LoadBattleGameScene()
        {
            Logger.Log("Load Battle Game Scene");
            // 使用异步加载，避免场景切换时的卡顿
            AsyncLoadingSceneEffect.LoadScene(SceneManager.GetActiveScene().name, FolderPath + BattleScene, false);
        }

        /// <summary>
        /// 加载主界面场景
        /// 从游戏场景返回主菜单时调用
        /// </summary>
        public static void LoadHomeScene()
        {
            Logger.Log("Load Home Scene");
            // 默认保留场景数据，方便返回时恢复状态
            AsyncLoadingSceneEffect.LoadScene(SceneManager.GetActiveScene().name, FolderPath + MainScene);
        }

        /// <summary>
        /// 首次加载主界面场景
        /// 用户登录成功后首次进入主界面时调用
        /// 与LoadHomeScene的区别是不保留之前的场景数据
        /// </summary>
        public static void LoadHomeSceneFirst()
        {
            Logger.Log("Load Home Scene");
            // false参数表示不保留当前场景数据，全新开始
            AsyncLoadingSceneEffect.LoadScene(SceneManager.GetActiveScene().name, FolderPath + MainScene, false);
        }

        /// <summary>
        /// 用户登出并返回登录界面
        /// 清理用户数据并返回到起始场景
        /// </summary>
        public static void BackAndLogout()
        {
            // 设置登出状态标志，其他系统可以根据此标志进行相应处理
            isLoggingOut = true;
            
            // 直接加载起始场景，不使用异步效果（因为要彻底清理状态）
            SceneManager.LoadScene(FolderPath + StartScene);
        }
        
        /// <summary>
        /// 直接加载起始场景
        /// 用于强制回到登录界面（比如网络错误、认证失败等情况）
        /// </summary>
        public static void LoadStartScene()
        {
            SceneManager.LoadScene(StartScene);
        }
    }
}