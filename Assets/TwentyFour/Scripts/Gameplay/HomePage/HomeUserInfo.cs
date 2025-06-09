using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TwentyFour.Scripts.Features.Player;

namespace TwentyFour.Scripts.Gameplay.HomePage
{
    public class HomeUserInfo : MonoBehaviour
    {
        [SerializeField] public Text UserNameText;
        [SerializeField] public TextMeshProUGUI CoinText;

        public GameObject TournamentButton;
        public GameObject StageButton;
        public GameObject StageButtonTournament;
        public GameObject NewMessageHint;
        public GameObject TournamentActiveHint;
        
        public TextMeshProUGUI VITText;
        public Text VITCostText;
        public GameObject RightButtonSizeFitter;

        public GameObject DefaultCategoryUpdatedHint;

        
        // Start is called before the first frame update
        void Start()
        {
            GetUserBagInfo();
        }

        public void OnClickDefaultStore()
        {
            DefaultCategoryUpdatedHint.SetActive(false);
        }
        private void OnViewInbox(bool newMessage)
        {
            NewMessageHint.SetActive(newMessage);
        }
        
        
        public GameObject RedeemQuestHint;
        public GameObject RedeemDailyQuestHint;
        
        

        void GetUserBagInfo()
        {
            //GetPersonaInventoryResponse personaInventories = await PassportFeatureSDK.Economy.SearchPersonaInventory();

        }
    }
}