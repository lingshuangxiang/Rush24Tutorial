using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class ScreenOptionHelper : MonoBehaviour
{
    public bool KeepScreenOn;

    private void Start()
    {
        if(ScreenOption.IsKeepScreenOn != KeepScreenOn)
            SetKeepScreenOn(KeepScreenOn);
    }

    private void OnDestroy()
    {
        
    }

    public void SetKeepScreenOn(bool isOn)
    {
        ScreenOption.IsKeepScreenOn = isOn;
        Logger.LogInfo("SetKeepScreenOn:"+isOn);
    }
}
