using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.UOS.TwentyFour 
{
    public class HomeUI : MonoBehaviour
    {
        [SerializeField] private GameObject joinBattlePopup;
        public void JoinBattle()
        {
            joinBattlePopup.SetActive(true);
        }
    }
}