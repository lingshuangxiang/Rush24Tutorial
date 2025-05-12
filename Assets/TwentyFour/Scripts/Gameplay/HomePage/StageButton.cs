using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Unity.UOS.TwentyFour
{
    public class StageButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        public TextMeshProUGUI textTMP;

        public int stageIndex;
        
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetStageIndex(int index)
        {
            textTMP.text = $"{index + 1}";
            stageIndex = index;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            StageManager.selectedStage = stageIndex;
            GameRouter.LoadStageGameScene();
        }
    }
}