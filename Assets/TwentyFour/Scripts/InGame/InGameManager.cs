using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Economy;
using TMPro;
using TwentyFour.Scripts.Metrics;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.UI;
using Unity.UOS.TwentyFour.Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Unity.UOS.TwentyFour.UOSGateway;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#endif
using Logger = Unity.UOS.TwentyFour.Common.Logger;
using Unity.UOS.TwentyFour.Scripts.Battle.Model;
using UnityEngine.UI;

namespace Unity.UOS.TwentyFour
{
    public enum GameMode
    {
        Stage,
        Battle
    }
    public class InGameManager : MonoBehaviour
    {
        [SerializeField] public GameObject BattlePage;
        [SerializeField] public GameObject ResultPopup;

        public GameObject ResultExitButton;
        public GameObject NextStageButton;
        public GameObject ResultPlaceholder;
        public GameObject RewardDetail;
        
        [SerializeField] public TextMeshProUGUI StageTMP;

        public Image RewardLightImage;
        public ParticleSystem RewardParticle;
        //[SerializeField] public TextMeshProUGUI NextStageButtonTMP;
        [SerializeField] public Text RewardQuantityTMP;
        [SerializeField] private GameMode gameMode = GameMode.Stage;
        
        public static Stage currentStage;
        public static InGameManager instance;
        public bool JustTestInGame=false;
        
        public UnityEvent SuccessEvent=new UnityEvent(){};
        public GameMode GameMode => gameMode;

        private Text _resultPlaceholderText;
        private Text resultPlaceholderText
        {
            get
            {
                if (_resultPlaceholderText == null)
                {
                    _resultPlaceholderText = ResultPlaceholder.GetComponent<Text>();
                }

                return _resultPlaceholderText;
            }
        }

        [SerializeField] private GameObject quitPopup;
        
        private void Awake()
        {
            instance = this;
            if (JustTestInGame == true)
            {
                currentStage = StageManager.ReturnAllStages()[0];
                SetCurrentStage(currentStage);
            }

            if (gameMode == GameMode.Battle)
            {
                // 联机模式
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
                WX.OnShow(WXOnShowCheckDisconnection);
#endif
                CheckDisconnection();
                MuninnMessage.OnJudgeResult.AddListener(ShowResult);
                if (MuninnManager.Singleton != null)
                {
                    GameInitManagerLocal.PreviousRoomId = MuninnManager.Singleton.GetMuninnRoomView().Room.Id;
                    GameInitManagerLocal.PreviousBattleMode = MuninnManager.Singleton.GetBattleMode();
                    MetricsHelper.TrackEvent(MetricsKeys.EVENT_JOIN_BATTLE, new Dictionary<string, object>()
                    {
                        { MetricsKeys.PARAM_BATTLE_MODE, GameInitManagerLocal.PreviousBattleMode.ToString() },
                        { MetricsKeys.PARAM_IS_ROBOT_ROOM, MuninnManager.IsRobotRoom() },
                        { MetricsKeys.PARAM_ROOM_ID, MuninnManager.Singleton.GetMuninnRoomView().Room.Id },
                    });
                }

            }
        }
        
        
        // Start is called before the first frame update
        void Start()
        {
            if(JustTestInGame==true)return;
            if (gameMode == GameMode.Stage)
            {
                SetCurrentStage(StageManager.NextStage());
                if(PersonaPropertiesHelper.ShowStageTutorial)
                    TutorialManager.Instance.StartTutorial();
            }
        }
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        void WXOnShowCheckDisconnection(WeChatWASM.OnShowListenerResult result)
        {
            CheckDisconnection();
        }
#endif
        void CheckDisconnection()
        {
            if (MuninnManager.Singleton != null)
            {
                MuninnManager.Singleton.OnDisconnectAction -= OnDisconnectAction;
                MuninnManager.Singleton.OnDisconnectAction += OnDisconnectAction;
                Logger.LogInfo("MuninnManager.Singleton.InRoom: " + MuninnManager.Singleton.InRoom);
                if (!MuninnManager.Singleton.InRoom)
                {
                    // quitPopup.SetActive(true);
                }
            }
        }
        
        private void OnDisconnectAction()
        {
            // 处理断连事件
            // 游戏已经结束
            if (MuninnMessage.GameOver())
            {
                // quitPopup.SetActive(true);
            }
            // 游戏尚未结束
            else
            {
                UIMessage.Show("正在重连...");
                RoomManager.JoinRoom(BattleMode.OneOnOneCustom, MuninnManager.Singleton.RoomId);
            }
        }

        private void OnDestroy()
        {
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
            WX.OffShow(WXOnShowCheckDisconnection);
#endif
            if (MuninnManager.Singleton == null) return;
            MuninnManager.Singleton.OnDisconnectAction -= OnDisconnectAction;
        }

        public void ExitBattle()
        {
            RoomManager.LeaveRoom();
            ExitGame();
        }

        void SetCurrentStage(Stage s)
        {
            currentStage = s;
            if (gameMode == GameMode.Stage)
            {
                StageTMP.text = (s.index + 1).ToString();
            }
        }

        async Task<uint> GetRewardRemote()
        {
            // if (StageManager.playerStageScores[currentStage.index] > 0)
            // {
            //     Debug.Log("Already got the reward");
            //     RewardQuantityTMP.text = "0";
            //     return;
            // }

            try
            {
                Transaction transaction =
                    await PassportFeatureSDK.Economy.VirtualPurchase("STAGE_REWARD_GOLD_COIN", 1);
                return transaction.Product.Rewards[0].Quantity;
            }
            catch (PassportException e)
            {
                Logger.LogError($"GetRewardRemote Failed: code:{e.Code} message:{e.Message}");
            }

            return 0;

            // Task<Transaction> task = PassportFeatureSDK.Economy.VirtualPurchase("STAGE_REWARD_GOLD_COIN", null);
            // yield return new WaitUntil(() => task.IsCompleted);
        }

        void ResetResultPopup()
        {
            resultPlaceholderText.text = "等待奖励进口袋...";
            NextStageButton.SetActive(false);
            ResultPlaceholder.SetActive(true);
            RewardDetail.SetActive(false);
            RewardLightImage.gameObject.SetActive(false);
        }
        
        void DisplayRewardDetail(bool newReward = true)
        {
            RewardLightImage.gameObject.SetActive(true);
            RewardParticle.Play();
            if (newReward)
            {
                RewardDetail.SetActive(true);
                ResultPlaceholder.SetActive(false);
                RewardLightImage.DOFade(1, 0.5f).From(0).SetEase(Ease.Linear);
            }
            else
            {
                resultPlaceholderText.text = "领过奖励啦";
            }
            
            NextStageButton.SetActive(true);
        } 

        public void ShowResult(bool result)
        {
            Logger.Log("调用 Show Result");
            if (!result) return;
            
            SuccessEvent?.Invoke();
            if(JustTestInGame)
                return;

            if (gameMode == GameMode.Stage)
            {
                MetricsHelper.TrackEvent(MetricsKeys.EVENT_CLEAR_STAGE, new Dictionary<string, object>()
                {
                    {MetricsKeys.PARAM_STAGE_INDEX, currentStage.index},
                });
                ResetResultPopup();
                BGMManager.Instance.PlayAFX(AFXMusic.StageWin);
                //save progress and get reward at first pass
                ResultPopup.SetActive(true);
                if (StageManager.playerStageScores[currentStage.index] == 0)
                {
                    var uiEffect = ResultPopup.GetComponent<UIEffect>();
                    if (uiEffect != null)
                    {
                        uiEffect.OnEnd.RemoveAllListeners();
                        uiEffect.OnEnd.AddListener(UploadUserData);
                    }
                }
                else
                {
                    DisplayRewardDetail(false);
                }
                

                //todo new account
                if (PersonaPropertiesHelper.ShowStageTutorial)
                {
                    ResultExitButton.SetActive(true);
                    TutorialManager.Instance.ShowTutorial("show_stage_tutorial",true);
                
                    PersonaPropertiesHelper.ShowStageTutorial = false;
                }
                // SetCurrentStage(StageManager.NextStage(1));
            }
        }

        void UploadUserData()
        {
            StartCoroutine(SaveProgressToCloud());
            StartCoroutine(GetReward());
        }
        IEnumerator SaveProgressToCloud()
        {
            Task saveTask = StageManager.SetStageScore(1);
            yield return new WaitUntil(()=>saveTask.IsCompleted);
            
        }

        IEnumerator GetReward()
        {
            Task<uint> rewardTask = GetRewardRemote();
            yield return new WaitUntil(()=>rewardTask.IsCompleted);
            yield return new WaitForSeconds(.7f);
            if (rewardTask.IsCompleted && !rewardTask.IsFaulted && rewardTask.Result>0)
            {
                RewardQuantityTMP.text = "+ " + rewardTask.Result.ToString();
                DisplayRewardDetail();
            }
        }
            
        public void SkipTutorial()
        {
            TutorialManager.Instance.SetProperties("show_stage_tutorial");
            PersonaPropertiesHelper.ShowStageTutorial = false;
            ExitGame();
        }
        
        public void NextRound()
        {
            SetCurrentStage(StageManager.NextStage(1));
            
            if (currentStage is null)
            {
                ExitGame();
            }
            
            ResultPopup.SetActive(false);
        }

        public void ExitGame()
        {
            PersonaPropertiesHelper.GetPersonaProperties();
            GameRouter.LoadHomeScene();
        }
        
        //TODO: init with different game mode: stage/battle
        
        #region BATTLE
        /// <summary>
        /// 接收到牌局信息
        /// </summary>
        public static void OnReceiveBattleStages(List<Stage> stages)
        {
            // //匹配超时或者房主已经取消匹配
            // if (MatchMakingManager.IsMatchedAndInRoom == false)
            // {
            //     RoomManager.LeaveRoom();
            //     GameInitManagerLocal.MyInitState = InitState.OpponentQuits;
            //     InGameManager.instance.ExitGame();
            //     return;
            // }
            Logger.LogInfo("接收到牌组数据+OnReceiveBattleStages");
            var roomView = MuninnManager.Singleton.GetMuninnRoomView();
            // if (roomView.Players.Count != 2)
            // {
            //     Logger.LogError("房间人数不为2");
            //     UIMessage.Show("对手退出");
            //     RoomManager.LeaveRoom();
            //     MuninnMessage.Clear();
            //     return;
            // }
            // 收到服务器下发的题目的时候为每张牌设置一个随机花色
            foreach (Stage s in stages)
                s.question?.ShuffleCardsSuit();
            
            StageManager.SetAllStages(stages, GameMode.Battle);
            StageManager.selectedStage = 0;
            currentStage = stages[0];
            MatchMakingManager.IsMatchedAndInRoom = true;
            GameRouter.LoadBattleGameScene(); 
            MuninnMessage.OnStages.RemoveListener(InGameManager.OnReceiveBattleStages);//移出上一个addScene避免重复加载
            //VIT System
            if (MuninnManager.Singleton.GetBattleMode() == BattleMode.OneOnOne)
            {
                VitalityHelper.Instance.Reset();
                VitalityHelper.Instance.ConsumeVitality(Identity.persona.PersonaID,(uint)VitalityHelper.MatchCost);
                UIManager.Instance.ShowVITConsumeToast(VitalityHelper.MatchCost);
            }
            
        }
        #endregion
    }
}