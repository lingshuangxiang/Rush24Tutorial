using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class SettlementAnimBridge : MonoBehaviour
{
    public SettlementAnimType AnimType;

    public AnimEventTest AnimEvent;

    public void SetOnDestroyAction(Action action)
    {
        Logger.Log($"SetOnDestroyAction type:{AnimType}");
        if (AnimEvent != null)
        {
            AnimEvent.OnDestroyAction = null;
            AnimEvent.OnDestroyAction += action;
        }
    }
    
}

public enum SettlementAnimType
{
    Lose,
    Win,
    Draw
}
