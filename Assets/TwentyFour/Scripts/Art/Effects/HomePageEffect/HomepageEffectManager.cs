using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TwentyFour.Scripts.Utilities;

namespace TwentyFour.Scripts.Art.Effects
{
    public class HomepageEffectManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            //Switch BGM to homepage music
            BGMManager.Instance.Play(BGMManager.BackgroundMusic.HomepageBGM);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnAddVitClick()
        {
            UIMessage.Show($"看广告去！");
        }

        public void OnAddGemClick()
        {
            UIMessage.Show($"看广告去！");
        }


        public void OnStageClick()
        {
        }
    }
}