using System;
using System.Collections.Generic;

namespace TwentyFour.Scripts.Gameplay.CardSystem
{
    [Serializable]
    public class Question
    {
        public List<Card> cards;

        public void ShuffleCardsSuit()
        {
            var suitedCards = new List<Card>(cards.Count);
            var noSuitCards = cards;
            foreach (var card in noSuitCards)
            {
                var newCard = new Card(card.number, card.index);
                suitedCards.Add(newCard);
            }
            cards = suitedCards;
        }
    }
}