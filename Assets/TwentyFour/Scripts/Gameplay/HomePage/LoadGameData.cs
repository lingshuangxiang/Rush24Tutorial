using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TwentyFour.Scripts.Features.Save;                    // 存档系统
using TwentyFour.Scripts.Gameplay.GameMode.StageMode;      // 关卡管理系统

namespace TwentyFour.Scripts.Gameplay.HomePage
{
    /// <summary>
    /// 游戏数据加载管理器
    /// 
    /// 功能职责：
    /// 1. 在用户登录成功后初始化游戏核心数据
    /// 2. 按顺序加载关卡数据和用户存档
    /// 3. 完成数据加载后跳转到主界面
    /// 
    /// 执行顺序：
    /// Start() → Init() → InitStage() → InitSave() → GameRouter.LoadHomeSceneFirst()
    /// 
    /// 这是连接登录和游戏主体的桥梁，确保进入游戏前所有必要数据都已准备就绪
    /// </summary>
    public class LoadGameData : MonoBehaviour
    {
        [Header("UI组件引用")]
        public GameObject createPersonaDialog;      // 创建角色对话框（暂未使用，可能用于首次登录）
        public Text coverPageHintText;              // 加载页面的状态提示文本
        
        /// <summary>
        /// Unity生命周期 - 脚本启动时调用
        /// 
        /// 当LoginController激活loadGameData对象后，此方法会被调用
        /// 立即启动数据初始化协程
        /// </summary>
        void Start()
        {
            // 启动数据加载主流程
            StartCoroutine(Init());
        }
        
        /// <summary>
        /// 主要的数据初始化协程
        /// 
        /// 按顺序执行两个关键的初始化步骤：
        /// 1. InitStage() - 从远程CDN加载关卡数据
        /// 2. InitSave() - 加载用户存档数据  
        /// 3. 完成后跳转到主界面
        /// 
        /// 使用yield return确保每个步骤完成后再执行下一步
        /// </summary>
        IEnumerator Init()
        {
            // 第一步：初始化关卡数据（从CDN下载题库）
            yield return StartCoroutine(InitStage());
            
            // 第二步：初始化用户存档数据（加载游戏进度）
            yield return StartCoroutine(InitSave());
            
            // 第三步：所有数据加载完成，跳转到游戏主界面
            GameRouter.LoadHomeSceneFirst();
        }

        /// <summary>
        /// 初始化关卡数据协程
        /// 
        /// 功能：
        /// 1. 更新UI提示为"正在...构建世界..."
        /// 2. 从远程CDN服务器下载24点题库数据
        /// 3. 解析并缓存所有关卡信息到StageManager
        /// 
        /// 这是游戏内容的基础，没有关卡数据就无法开始游戏
        /// </summary>
        IEnumerator InitStage()
        {
            // 更新加载状态提示
            coverPageHintText.text = "正在...构建世界...";
            
            // 从远程配置加载所有关卡数据
            // 这会从CDN下载questions_1.csv文件，包含所有24点题目
            StageManager.LoadAllStagesFromRemoteConfig();
            
            // 等待一帧，确保UI更新和数据处理完成
            yield return null;
        }
        
        /// <summary>
        /// 初始化用户存档数据协程
        /// 
        /// 功能：
        /// 1. 更新UI提示为"正在...了解过去..."
        /// 2. 从本地/云端加载用户的游戏进度
        /// 3. 恢复用户的关卡完成状态、分数等数据
        /// 
        /// 这确保玩家能够从上次游戏的地方继续进行
        /// </summary>
        IEnumerator InitSave()
        {
            // 更新加载状态提示（体现了游戏的文学性表达）
            coverPageHintText.text = "正在...了解过去...";
            
            // 初始化存档系统，加载用户数据
            // 这会从PlayerPrefs或云端加载用户的游戏进度
            UOSSave.Init();
            
            // yield break 表示协程正常结束（等同于return）
            yield break;
        }
    }
}