using System.Collections;
using System.Collections.Generic;
using TwentyFour.Scripts.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public Toggle toggleBGM;
    public Toggle toggleAFX;

    private void OnEnable()
    {
        InitBgMAndAFX();
    }

    private void InitBgMAndAFX()
    {
        toggleAFX.isOn = !BGMManager.Instance.AFXSource.mute;
        toggleBGM.isOn = !BGMManager.Instance.audioSource.mute;
    }
}
