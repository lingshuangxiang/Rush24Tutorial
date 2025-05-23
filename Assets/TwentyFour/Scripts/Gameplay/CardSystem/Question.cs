using System;
using System.Collections.Generic;
using Unity.UOS.TwentyFour.Model;

namespace TwentyFour.Scripts.Gameplay.CardSystem
{
    [Serializable]
    public class Question
    {
        public List<Card> cards;

        public void ShuffleCardsSuit()
        {
            var suitedCards = new List<Card>(cards.Count);
            var NoSuitCards = cards;
            foreach (var card in NoSuitCards)
            {
                var newCard = new Card(card.number, card.index);
                suitedCards.Add(newCard);
            }
            cards = suitedCards;
        }
    }
}