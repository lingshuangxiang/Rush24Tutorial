using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.UOS.TwentyFour.Model;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.UOS.TwentyFour
{
    public class GivenOperator : MonoBehaviour, IPointerClickHandler
    {
        public TextMeshProUGUI textObject;

        [SerializeField] public AnswerManager answerManager;
        [SerializeField] public Operator operatorModel;

        private ButtonTextAdaptor[] btns;
        
        // Start is called before the first frame update
        void Start()
        {
            btns = transform.parent.GetComponentsInChildren<ButtonTextAdaptor>();
        }

        // Update is called once per frame
        void Update()
        {
            textObject.text = operatorModel.Symble();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(BattleEffectManager.IsCurrentQuestionResolved())
                return;
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
                }
            }
        }
    }
}