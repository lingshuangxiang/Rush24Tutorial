using System;
using System.Collections;
using System.Collections.Generic;
using Unity.UOS.TwentyFour;
using Unity.UOS.TwentyFour.UOSGateway;
using Unity.VisualScripting;
using UnityEngine;
using TwentyFour.Scripts.Utilities;

namespace Unity.UOS.TwentyFour
{
    public class MainSceneManager : MonoBehaviour
    {

        public void Logout()
        {
            ClientInitHelper.Logout();
        }

        // Start is called before the first frame update
        void Start()
        {
          
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
