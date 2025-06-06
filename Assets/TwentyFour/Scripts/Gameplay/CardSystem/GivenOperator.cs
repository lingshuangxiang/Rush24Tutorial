using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TwentyFour.Scripts.Gameplay.GameMode.StageMode;
using TwentyFour.Scripts.Utilities;

namespace TwentyFour.Scripts.Gameplay.CardSystem
{
    public class GivenOperator : MonoBehaviour, IPointerDownHandler
    {
        public TextMeshProUGUI textObject;

        [SerializeField] public AnswerManager answerManager;
        [SerializeField] public Operator operatorModel;

        private ButtonTextAdaptor[] btns;

        public Image Icon;
        public List<Sprite> IconSprites;
        // Start is called before the first frame update
        void Start()
        {
            btns = transform.parent.GetComponentsInChildren<ButtonTextAdaptor>();
            Icon.sprite = IconSprites[(int)operatorModel.name];
        }

        // Update is called once per frame
        void Update()
        {
            textObject.text = operatorModel.GetSymbol();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            answerManager.AddOperator(operatorModel);
            
            foreach (var btn in btns)
            {
                if (btn.gameObject != gameObject)
                {
                    btn.OnDeselect();
                }
                else
                {
                    if (btn.isSelected)
                    {
                        answerManager.RemoveOperator(operatorModel);
                    }
                    btn.OnSelect();
                }
            }
        }
    }
}