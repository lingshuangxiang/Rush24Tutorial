using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using TwentyFour.Scripts.Gameplay.GameMode.StageMode;

namespace TwentyFour.Scripts.Gameplay.CardSystem
{
    public class GivenOperator : MonoBehaviour, IPointerClickHandler
    {
        public TextMeshProUGUI textObject;

        [SerializeField] public AnswerManager answerManager;
        [SerializeField] public Operator operatorModel;
        
        // Update is called once per frame
        void Update()
        {
            textObject.text = operatorModel.GetSymbol();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            answerManager.AddOperator(operatorModel);
        }
    }
}