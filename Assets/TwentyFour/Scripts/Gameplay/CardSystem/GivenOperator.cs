using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.UOS.TwentyFour.Model;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.UOS.TwentyFour;
namespace TwentyFour.Scripts.Gameplay.CardSystem
{
    public class GivenOperator : MonoBehaviour, IPointerClickHandler
    {
        public TextMeshProUGUI textObject;

        [SerializeField] public AnswerManager answerManager;
        [SerializeField] public Operator operatorModel;

        
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            textObject.text = operatorModel.Symble();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            answerManager.AddOperator(operatorModel);
        }
    }
}