using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace TwentyFour.Scripts.Gameplay.CardSystem
{
    public enum Suit
    {
        Heart,Diamond,Spade,Club
    }
    
    [Serializable]
    public struct Card
    {
        public Suit suit;
        public int number;//1~13
        public string text
        {
            get
            {
                if (number == 1)
                {
                    return "A";
                }
                if (number == 11)
                {
                    return "J";
                }

                if (number == 12)
                {
                    return "Q";
                }

                if (number == 13)
                {
                    return "K";
                }

                return number.ToString();
            }
        }

        public int index;

        public Card(int number, int index) : this()
        {
            this.number = number;
            this.index = index;
            Random random = new Random();
            suit = (Suit)random.Next(4);
        }
    }   
}
