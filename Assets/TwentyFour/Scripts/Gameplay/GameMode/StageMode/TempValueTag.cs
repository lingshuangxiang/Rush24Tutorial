using TMPro;
using UnityEngine;

namespace TwentyFour.Scripts.Gameplay.GameMode.StageMode
{
    public class TempValueTag : MonoBehaviour
    {
        public TextMeshProUGUI TextTMP;

        public void SetText(string text)
        {
            TextTMP.text = text;
        }
    }
}
