using System.Collections.Generic;
using UnityEngine;

namespace Unity.UOS.TwentyFour
{
    public class StageList : MonoBehaviour
    {
        [SerializeField]public GameObject PageIndicatorGo;
        [SerializeField]public GameObject StagePagePrefab;
        [SerializeField]public GameObject StageCompletePrefab;
        [SerializeField]public GameObject StageLockPrefab;
        [SerializeField]public GameObject StageReadyPrefab;

        private const int PAGE_SIZE = 18;
        List<GameObject> stagePages;
        int activePage = 0;
        
        public GameObject NextButton;
        public GameObject PreviousButton;
        
        // Start is called before the first frame update
        void Start()
        {
            StageManager.selectedStage = null;
            bool setReadyStage = false;
            stagePages = new List<GameObject>();
            activePage = 0;
            GameObject page = null;
            
            //init stage items according to user score data
            var allStages = StageManager.GetAllStages(GameMode.Stage);
            for (var i=0; i<allStages.Count; i++)
            {
                //instantiate new page if previous is full
                if (i % PAGE_SIZE == 0)
                {
                    if (page != null)
                    {
                        stagePages.Add(page);
                    }
                    page = Instantiate(StagePagePrefab, transform);
                }
                
                var score = StageManager.playerStageScores[i];
                if (score > 0)
                {
                    var go = Instantiate(StageCompletePrefab, page.transform);
                    StageButton sb = go.GetComponent<StageButton>();
                    sb.SetStageIndex(i);
                } else if (!setReadyStage)
                {
                    var go = Instantiate(StageReadyPrefab, page.transform);
                    StageButton sb = go.GetComponent<StageButton>();
                    sb.SetStageIndex(i);
                    activePage = stagePages.Count;
                    setReadyStage = true;
                }
                else
                {
                    var go = Instantiate(StageLockPrefab, page.transform);
                }
            }
            if (page != null)
            {
                stagePages.Add(page);
            }

            //init page indicator
            PageIndicatorGo.GetComponent<PageIndicator>().Init(stagePages.Count, activePage);
            GotoPage(activePage);
            RefreshButtons();
        }

        private void OnEnable()
        {
            if (stagePages != null)
                RefreshButtons();
        }

        public void GotoPage(int i)
        {
            foreach (var page in stagePages)
            {
                page.SetActive(false);
            }
            stagePages[i].SetActive(true);
        }

        public void GotoPreviousPage()
        {
            activePage = activePage > 1 ? activePage - 1 : 0;
            RefreshButtons();
            GotoPage(activePage);
        }
        
        public void GotoNextPage()
        {
            activePage = activePage < stagePages.Count - 1 ? activePage + 1 : stagePages.Count - 1;
            RefreshButtons();
            GotoPage(activePage);
        }

        void RefreshButtons()
        {
            PreviousButton.SetActive(activePage > 0);
            NextButton.SetActive(activePage < stagePages.Count-1);
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}