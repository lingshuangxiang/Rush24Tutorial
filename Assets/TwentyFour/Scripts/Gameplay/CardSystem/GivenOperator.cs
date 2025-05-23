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