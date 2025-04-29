using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.UOS.TwentyFour.Model;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.UOS.TwentyFour
{
    public class GivenCard : MonoBehaviour
        //,IInitializePotentialDragHandler,IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] public Card cardModel;
        [SerializeField] public GameObject cardNum;
        public GameObject cardFaceGo;
        private Image cardFaceRenderer;
        private TextMeshProUGUI cardNumTextMeshPro;

        private Vector3 offset;
        private Vector3 originalPos;



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