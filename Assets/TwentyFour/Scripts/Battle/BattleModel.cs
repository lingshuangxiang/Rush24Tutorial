using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.UOS.TwentyFour.Scripts.Battle.Model
{
    public enum BattleMode
    {
        None,
        OneOnOne,
        OneOnOneCustom,
        TwoOnTwo,
        TournamentOneOnOne,
    }

    [Serializable]
    public class BattleData
    {
        public int cardIndex;
        
    }
}