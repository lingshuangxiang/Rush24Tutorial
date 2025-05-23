using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TwentyFour.Scripts.Gameplay.GameMode.StageMode;

namespace TwentyFour.Scripts.Gameplay.HomePage
{
    public class StageButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        public TextMeshProUGUI textTMP;

        public int stageIndex;

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