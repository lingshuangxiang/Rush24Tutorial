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
using TwentyFour.Scripts.Tournament;
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
            InBoxHelper.OnViewInbox += OnViewInbox;
            QuestHelper.OnSearchPersonaQuests += RefreshQuestInfo;
            VitalityHelper.Instance.OnVitalityUpdated += RefreshVIT;
            OnViewInbox(InBoxHelper.NewMessageFound);
            GetUserBagInfo();
            RefreshQuestInfo();
            RefreshVIT();
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
            
            if(VitalityHelper.Inited && VitalityHelper.Instance != null)
                VitalityHelper.Instance.OnVitalityUpdated -= RefreshVIT;
            PersonaPropertiesHelper.OnPersonaUpdatedAction -= OnPersonaUpdatedAction;
            InventoryHelper.OnInventoryUpdated -= GetUserBagInfo;
            InBoxHelper.OnViewInbox -= OnViewInbox;
            QuestHelper.OnSearchPersonaQuests -= RefreshQuestInfo;



        }   
        public GameObject RedeemQuestHint;
        public GameObject RedeemDailyQuestHint;
        public void ConsumeVIT()
        {
            VitalityHelper.Instance.ConsumeVitality(Identity.persona.PersonaID,20);
        }
        void RefreshVIT()
        {
            VITCostText.text = $"{VitalityHelper.MatchCost}";
            VITText.text = $"{VitalityHelper.Instance.CurrentVitality}/{VitalityHelper.Instance.MaxVitality}";
        }
        void RefreshQuestInfo()
        {
            if (TournamentData.Current)
            {
                var questRefData = TournamentData.Current.ReferenceSlugs.Find(item => item.SlugType == TournamentDataReferenceSlugType.Quests);
                var questSlug = questRefData.Datas.ToDictionary()["main"];
                var questData = QuestHelper.PersonaQuests[questSlug];
                var canRedeem = false;
                foreach (var quest in questData.Items)
                {
                    if (quest.Completed && !quest.Redeemed)
                    {
                        canRedeem = true;
                        break;
                    }
                }
                RedeemQuestHint?.SetActive(canRedeem);
            }
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

                if (TournamentData.Current != null)
                {
                    TournamentButton.SetActive(true);
                    StageButton.SetActive(false);
                    StageButtonTournament.SetActive(true);
                    TournamentActiveHint.SetActive(TournamentData.Current.IsActive());
                   
                    
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

    }
}