using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TwentyFour.Scripts.Features.Player;

namespace TwentyFour.Scripts.Gameplay.HomePage
{
    /// <summary>
    /// 主界面用户信息展示与交互控制器
    /// 
    /// 主要功能：
    /// 1. 显示玩家昵称、金币、体力等信息
    /// 2. 控制主界面相关按钮和提示的显示/隐藏
    /// 3. 响应用户点击事件（如查看商店、收件箱等）
    /// 4. 预留与背包/经济系统的对接接口
    /// </summary>
    public class HomeUserInfo : MonoBehaviour
    {
        [SerializeField] public Text UserNameText;                // 玩家昵称文本
        [SerializeField] public TextMeshProUGUI CoinText;         // 金币数量文本

        public GameObject TournamentButton;                       // 锦标赛按钮
        public GameObject StageButton;                            // 闯关按钮
        public GameObject StageButtonTournament;                  // 锦标赛专用闯关按钮
        public GameObject NewMessageHint;                         // 新消息提示
        public GameObject TournamentActiveHint;                   // 锦标赛活动提示

        public TextMeshProUGUI VITText;                           // 体力值文本
        public Text VITCostText;                                  // 体力消耗文本
        public GameObject RightButtonSizeFitter;                  // 右侧按钮自适应容器

        public GameObject DefaultCategoryUpdatedHint;             // 默认商店分类更新提示

        // Unity生命周期 - 启动时自动调用
        void Start()
        {
            GetUserBagInfo(); // 获取玩家背包/经济信息（如金币、体力等）
        }

        // 点击默认商店按钮时，隐藏更新提示
        public void OnClickDefaultStore()
        {
            DefaultCategoryUpdatedHint.SetActive(false);
        }

        // 查看收件箱时，控制新消息提示的显示
        private void OnViewInbox(bool newMessage)
        {
            NewMessageHint.SetActive(newMessage);
        }

        public GameObject RedeemQuestHint;                        // 任务兑换提示
        public GameObject RedeemDailyQuestHint;                   // 每日任务兑换提示

        // 获取玩家背包/经济信息（预留，实际实现需对接经济系统SDK）
        void GetUserBagInfo()
        {
            //GetPersonaInventoryResponse personaInventories = await PassportFeatureSDK.Economy.SearchPersonaInventory();
            // TODO: 这里应调用经济系统接口，获取金币、体力等数据并刷新UI
        }
    }
}