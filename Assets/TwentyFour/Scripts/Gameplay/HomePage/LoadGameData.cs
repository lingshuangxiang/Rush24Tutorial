using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TwentyFour.Scripts.Features.Save;
using TwentyFour.Scripts.Gameplay.GameMode.StageMode;

namespace TwentyFour.Scripts.Gameplay.HomePage
{
    public class LoadGameData : MonoBehaviour
    {
        public GameObject createPersonaDialog;
        public Text progressTextTmp;
        
        void Start()
        {
            StartCoroutine(Init());
        }
        
        // Start is called before the first frame update
        IEnumerator Init()
        {
            yield return StartCoroutine(InitStage());
            yield return StartCoroutine(InitSave());
            GameRouter.LoadHomeSceneFirst();
        }

        IEnumerator InitStage()
        {
            progressTextTmp.text = "正在...构建世界...";
            StageManager.LoadAllStagesFromRemoteConfig();
            yield return null;
        }
        
        IEnumerator InitSave()
        {
            progressTextTmp.text = "正在...了解过去...";
            UOSSave.Init();
            yield break;
        }
    }
}