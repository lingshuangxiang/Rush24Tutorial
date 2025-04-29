using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimTrigger : MonoBehaviour
{
    public List<UnityEventUnit> allUnityEventUnits = new List<UnityEventUnit>();

    public void PlayEvent(int PlayEvent)
    {
        allUnityEventUnits[PlayEvent].AllGameEvent?.Invoke();
    }
}

[System.Serializable]
public class UnityEventUnit
{
    public UnityEvent AllGameEvent = new UnityEvent();
}
