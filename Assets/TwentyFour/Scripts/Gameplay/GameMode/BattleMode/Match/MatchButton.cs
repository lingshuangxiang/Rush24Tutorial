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
            OnSucceed?.Invoke();

        }
    }
}