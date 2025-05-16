using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.UOS.TwentyFour.UOSGateway;

namespace TwentyFour.Scripts.Tests
{
    public class TestReconnect : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            DontDestroyOnLoad(this);
        }
   }
}