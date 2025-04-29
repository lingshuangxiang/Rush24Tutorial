using System;
using System.Collections;
using TwentyFour.Scripts.Common;
using UnityEngine;
using Logger = Unity.UOS.TwentyFour.Common.Logger;


namespace TwentyFour.Scripts.Vitality
{
    public class CoroutineBridge : MonoBehaviour
    {
        public bool Checking;
        WaitForSecondsRealtime OneSecondsRealtime => VitalityHelper.Instance.OneSeconds;
        public IEnumerator CheckUIVitality()
        {
            while (Checking)
            {
                yield return OneSecondsRealtime;
                var now = DateTime.UtcNow;
                var currentTime = TimeConverter.DateTimeToTotalSeconds(now);
                var deltaTime = (int)Math.Ceiling(currentTime - VitalityHelper.Instance.lastUpdated);
                Logger.Log(
                    $"【CoroutineBridge{this.GetHashCode()}】:currentCount{VitalityHelper.Instance.CurrentVitality}  currentTime :{currentTime} delta:{deltaTime}  下一次回复还需{VitalityHelper.Instance.intervalSeconds - deltaTime % VitalityHelper.Instance.intervalSeconds}");

                VitalityHelper.Instance.NextTimeRemain = VitalityHelper.Instance.intervalSeconds -
                                                        deltaTime % VitalityHelper.Instance.intervalSeconds;
                if (deltaTime >= VitalityHelper.Instance.intervalSeconds)
                {
                    VitalityHelper.Instance.RefreshVITCount(deltaTime);
                    
                }
                VitalityHelper.Instance.OnVitalityCheck?.Invoke();
            }
            
        }

        public void StartCheck()
        {
            Checking = true;
            StartCoroutine(CheckUIVitality());
        }
    }
}