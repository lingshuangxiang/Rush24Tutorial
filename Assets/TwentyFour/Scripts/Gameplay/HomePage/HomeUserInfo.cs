using System;
using System.Collections;
using System.Collections.Generic;
using Economy;
using Passport;
using TMPro;
using TwentyFour.Scripts.Category;
using TwentyFour.Scripts.Purchase;
using TwentyFour.Scripts.Quest;
using TwentyFour.Scripts.RemoteConfig;
using Unity.Passport.Runtime;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Unity.UOS.TwentyFour
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
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
            RightButtonSizeFitter.SetActive(true);
#endif
            UserNameText.text = Identity.persona.DisplayName;
            PersonaPropertiesHelper.OnPersonaUpdatedAction += OnPersonaUpdatedAction;
            InventoryHelper.OnInventoryUpdated += GetUserBagInfo;
            QuestHelper.OnSearchPersonaQuests += RefreshQuestInfo;
            GetUserBagInfo();
            RefreshQuestInfo();
            DefaultCategoryUpdatedHint.SetActive(CategoryHelper.DefaultCategoryUpdated);
        }

        public void OnClickDefaultStore()
        {
            DefaultCategoryUpdatedHint.SetActive(false);
            CategoryHelper.DefaultCategoryUpdated = false;
            LocalStorageUtil.SetString(LocalStorageKeys.DEFAULT_CATEGORY_UPDATED,CategoryHelper.DefaultCategoryUpdatedTime);


        }
        private void OnViewInbox(bool newMessage)
        {
            NewMessageHint.SetActive(newMessage);
        }
        

        void OnDestroy()
        {
            PersonaPropertiesHelper.OnPersonaUpdatedAction -= OnPersonaUpdatedAction;
            InventoryHelper.OnInventoryUpdated -= GetUserBagInfo;
            QuestHelper.OnSearchPersonaQuests -= RefreshQuestInfo;



        }   
        public GameObject RedeemQuestHint;
        public GameObject RedeemDailyQuestHint;
        
        void RefreshQuestInfo()
        {
            var DailyData = QuestHelper.PersonaQuests[QuestKeys.DailyMatchQuestsSlug];
            var canRedeemDaily = false;
            foreach (var quest in DailyData.Items)
            {
                if (quest.Completed && !quest.Redeemed)
                {
                    canRedeemDaily = true;
                    break;
                }
            }
            RedeemDailyQuestHint?.SetActive(canRedeemDaily);
        }
        private void OnPersonaUpdatedAction(Persona persona)
        {
            UserNameText.text = Identity.persona.DisplayName;
        }

        async void GetUserBagInfo()
        {
            //GetPersonaInventoryResponse personaInventories = await PassportFeatureSDK.Economy.SearchPersonaInventory();
            foreach (var inventory in InventoryHelper.ExpandedInventoryItems)
            {
                if (inventory.Resource.ResourceSlug == "GOLD_COIN")
                {
                    CoinText.text = inventory.Quantity.ToString();
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

    }
}