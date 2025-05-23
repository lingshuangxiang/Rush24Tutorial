using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFour.Scripts.Gameplay.CardSystem
{
    public class GivenCard : MonoBehaviour
    {
        [SerializeField] public Card cardModel;
        [SerializeField] public GameObject cardNum;
        public GameObject cardFaceGo;
        private Image cardFaceRenderer;
        private TextMeshProUGUI cardNumTextMeshPro;

        private Vector3 offset;
        private Vector3 originalPos;
        
        public void Init(Card card)
        {
            cardModel = card;
            LoadCardFace();
        }

        public void LoadCardFace()
        {
            string path = "Easter/card_";
            path += cardModel.suit.ToString();
            cardNumTextMeshPro = cardNum.GetComponent<TextMeshProUGUI>(); 
            cardNumTextMeshPro.text = cardModel.text;

            if(cardModel.suit == Suit.Heart || cardModel.suit == Suit.Diamond)
            {
                cardNumTextMeshPro.color = Color.red;
            }
            else
            {
                cardNumTextMeshPro.color = Color.black;
            }
            cardFaceRenderer = cardFaceGo.GetComponent<Image>();
            cardFaceRenderer.sprite = Resources.Load(path, typeof(Sprite)) as Sprite;
        }
    }
}