using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibrateMono : MonoBehaviour
{
    public void Vibrate(int level)
    {
        VibrateHelper.Vibrate(level);
    }

    public void VibrateLong()
    {
        VibrateHelper.VibrateLong();
    }
}
