using UnityEngine;
using UnityEngine.UI;

namespace TwentyFour.Scripts.Gameplay.GameMode.StageMode
{
    public class TempValueTag : MonoBehaviour
    {
        public Text TextTMP;
        public void SetText(string text)
        {
            TextTMP.text = text;
        }
    }
}
