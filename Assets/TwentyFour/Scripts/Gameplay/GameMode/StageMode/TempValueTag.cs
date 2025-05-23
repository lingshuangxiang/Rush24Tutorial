using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TwentyFour.Scripts.Gameplay.GameMode.StageMode
{
    public class TempValueTag : MonoBehaviour
    {
        public TextMeshProUGUI TextTMP;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetText(string text)
        {
            TextTMP.text = text;
        }
    }
}
