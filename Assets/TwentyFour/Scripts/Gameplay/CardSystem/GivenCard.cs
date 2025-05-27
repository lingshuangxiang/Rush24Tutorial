using UnityEngine;
using UnityEngine.UI;
using TwentyFour.Scripts.Utilities;

namespace TwentyFour.Scripts.Gameplay.CardSystem
{
    public class GivenCard : MonoBehaviour
    //,IInitializePotentialDragHandler,IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] public Card cardModel;
        [SerializeField] public GameObject cardNum;
        public GameObject cardFaceGo;
        private Image cardFaceRenderer;
        private Text cardNumTextMeshPro;
        

        private Vector3 offset;
        private Vector3 originalPos;

        public RectTransform CardRect;
        public RectTransform CardStart;
        public RectTransform CardEnd;


        // Start is called before the first frame update
        void Start()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="card"></param>
        public void Init(Card card)
        {
            cardModel = card;
            LoadCardFace();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void LoadCardFace()
        {
            string path = "Easter/card_";
            path += cardModel.suit.ToString();
            cardNumTextMeshPro = cardNum.GetComponent<Text>(); 
            cardNumTextMeshPro.text = cardModel.text;

            if(cardModel.suit == Suit.Heart || cardModel.suit == Suit.Diamond)
            {
                cardNumTextMeshPro.color = CustomColors.CardRed;
            }
            else
            {
                cardNumTextMeshPro.color = CustomColors.CardDark;
            }
            cardFaceRenderer = cardFaceGo.GetComponent<Image>();
            cardFaceRenderer.sprite = Resources.Load(path, typeof(Sprite)) as Sprite;
        }
        
    }
}