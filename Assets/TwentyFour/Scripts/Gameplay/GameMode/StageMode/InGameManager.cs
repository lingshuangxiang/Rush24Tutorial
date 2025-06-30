using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Logger = TwentyFour.Scripts.Utilities.Logger;
using UnityEngine.UI;
using TwentyFour.Scripts.Gameplay.HomePage;
using TwentyFour.Scripts.Utilities;
using TwentyFour.Scripts.Art.UIEffect;

namespace TwentyFour.Scripts.Gameplay.GameMode.StageMode
{
    public class InGameManager : MonoBehaviour
    {
        [SerializeField] public GameObject BattlePage;
        [SerializeField] public GameObject ResultPopup;

        public GameObject ResultExitButton;
        public GameObject NextStageButton;
        public GameObject ResultPlaceholder;
        public GameObject RewardDetail;
        
        [SerializeField] public Text StageTMP;

        public Image RewardLightImage;
        public ParticleSystem RewardParticle;
        //[SerializeField] public TextMeshProUGUI NextStageButtonTMP;
        [SerializeField] public Text RewardQuantityTMP;
        
        public static Stage currentStage;
        public static InGameManager instance;
        
        public UnityEvent SuccessEvent=new UnityEvent(){};
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
        }
        
        
        // Start is called before the first frame update
        void Start()
        {
            SetCurrentStage(StageManager.NextStage());
        }
        
        public void ExitBattle()
        {
            ExitGame();
        }

        void SetCurrentStage(Stage s)
        {
            currentStage = s;
            StageTMP.text = (s.index + 1).ToString();
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
            if (!result) return;
            
            SuccessEvent?.Invoke();
            
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
            var nextStage = StageManager.NextStage(1);
            ResultPopup.SetActive(false);

            if (nextStage is null)
            {
                ExitGame();
                return;
            }
            SetCurrentStage(nextStage);
        }

        public void ExitGame()
        {
            GameRouter.LoadHomeScene();
        }
    }
}