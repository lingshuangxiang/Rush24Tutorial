using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.UOS.TwentyFour.Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Unity.UOS.TwentyFour.UOSGateway;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#endif
using Logger = Unity.UOS.TwentyFour.Common.Logger;
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
        }
        
        
        // Start is called before the first frame update
        void Start()
        {
            if(JustTestInGame==true)return;
            if (gameMode == GameMode.Stage)
            {
                SetCurrentStage(StageManager.NextStage());
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

        }
        
        private void OnDisconnectAction()
        {

        }

        private void OnDestroy()
        {
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
            WX.OffShow(WXOnShowCheckDisconnection);
#endif
        }

        public void ExitBattle()
        {
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
            }
        }

        void UploadUserData()
        {
            StartCoroutine(SaveProgressToCloud());
            StartCoroutine(GetReward());
        }
        IEnumerator SaveProgressToCloud()
        {
            StageManager.SetStageScore(1);
            yield break;            
        }

        IEnumerator GetReward()
        {
            yield return new WaitForSeconds(1);
            DisplayRewardDetail();
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
            GameRouter.LoadHomeScene();
        }
    }
}