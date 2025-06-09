using UnityEngine;
using UnityEngine.UI;
using TwentyFour.Scripts.Features.Player;

namespace TwentyFour.Scripts.Art.Effects
{
    /// <summary>
    /// 本代码会根据提供的信息反馈指定玩家的名称，段位属性等内容,并且同时具备主机对玩家的操作空间
    /// </summary>
    public class PlayerPanelInfoUpdate : MonoBehaviour
    {
        public Text PlayerName = null;
        public TiersBadge TierBadge;

        public Button KickButton;
        public GameObject IsMasterClient;
        public bool IsSelf;
        public PlayerCharatorManager PlayerCharator;

        private void Awake()
        {
            if (IsSelf)
            {
                ShowSelfInfo();
            }
            else
            {
            }
        }

        private void OnDestroy()
        {
            KickButton?.onClick.RemoveAllListeners();

        }

        private void ShowSelfInfo()
        {
        }

        private void UpdateMasterClient()
        {

        }

        public void ShowPlayerInfo()
        {

        }

        private void OnMasterClientChanged(uint obj)
        {
            UpdateMasterClient();
        }
    }
}