using UnityEngine;
using TwentyFour.Scripts.Utilities;

namespace TwentyFour.Scripts.Art.Audio
{
    public class AudioPlayHelper : MonoBehaviour
    {
        public void PlayAFX(int type)
        {
            BGMManager.Instance.PlayAFX((AFXMusic)type);
        }

        public void EnableBGM(bool isOn)
        {
            BGMManager.Instance.EnableBGM(isOn);
        }

        public void EnableAFX(bool isOn)
        {
            BGMManager.Instance.EnableAFX(isOn);
        }
    }
}