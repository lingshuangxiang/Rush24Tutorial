using UnityEngine;
using Unity.Muninn.Model;
using UnityEngine.UI;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

namespace Unity.UOS.TwentyFour.Scripts.Component
{
    public class InGameAvatarUI : MonoBehaviour
    {
        [SerializeField] private Text playerNameText;

        public void Init(MuninnPlayer player)
        {
            var name = player?.Name;
            if (player == null)
            {
                Logger.LogError("Failed to match player name");
                name = "神秘人";
            }
            playerNameText.text = name;
        }
    }
}