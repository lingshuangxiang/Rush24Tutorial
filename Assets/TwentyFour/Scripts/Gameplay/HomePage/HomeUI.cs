using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TwentyFour.Scripts.Gameplay.HomePage
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