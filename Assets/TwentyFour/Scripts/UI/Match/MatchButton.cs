using System.Collections;
using Unity.Passport.Runtime.UI;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.Events;

namespace TwentyFour.Scripts.UI.Match
{
    public class MatchButton : MonoBehaviour
    {
        public UnityEvent OnSucceed = new UnityEvent();


        public void FetchVitality()
        {

            if (VitalityHelper.Instance.CurrentVitality >= VitalityHelper.MatchCost)
            {
                OnSucceed?.Invoke();
            }
            else
            {
                UIMessage.Show("体力不足");
            }
            StartCoroutine(GetData());
        }

        IEnumerator GetData()
        {
            yield return VitalityHelper.Instance.GetVitalityData(Identity.persona.PersonaID,true);
            
        }
    }
}