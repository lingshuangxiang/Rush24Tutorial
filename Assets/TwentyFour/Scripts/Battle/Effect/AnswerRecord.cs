using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Passport.Runtime;
using Unity.UOS.TwentyFour;
using Unity.UOS.TwentyFour.Model;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
public class AnswerContainer
{
    [Tooltip("第一行的公式")] public string firstRow;
    [Tooltip("第二行的公式")] public string SecondRow;
    [Tooltip("第三行的公式")] public string thirdRow;

    public Stage targetStage;
}

public class AnswerRecord : MonoBehaviour
{
    public InGameManager StageInGameManager;
    public AnswerManager StageAnswerManager;
    
    public AnswerContainer currentAnswer; //当前选择的对象

    public List<AnswerContainer> allAnswers = new List<AnswerContainer>();
    public List<QuestionIndexButton> allChooseButtons = new List<QuestionIndexButton>();
    public List<CardGroup> twentyFourCardGroup = new List<CardGroup>();
    public TextMeshProUGUI firstRowText;
    public TextMeshProUGUI secondRowText;
    public TextMeshProUGUI thirdRowText;
    public bool isSettled = false;

    private void OnEnable()
    {
        StartCoroutine(WaitStagesToSetup());
    }

    IEnumerator WaitStagesToSetup()
    {
        var stages = StageManager.GetAllStages(GameMode.Battle);
        while (stages.Count == 0)
        {
            yield return
            stages = StageManager.GetAllStages(GameMode.Battle);
        }
        Setup(stages);
    }

    private void OnDisable()
    {
    }

    public void Setup(List<Stage> stages)
    {
        if (isSettled) 
            return;
        isSettled = true;
        for (int i = 0; i < stages.Count; i++)
        {
            AnswerContainer tempA = new AnswerContainer();
            tempA.targetStage = stages[i];
            allAnswers.Add(tempA);
        }

        currentAnswer = allAnswers[0];
    }

    public void SetIndex(int stageIndex)
    {
        Debug.Log("SetToCurrentStage:" + stageIndex);
        currentAnswer = allAnswers[stageIndex];
        InGameManager.currentStage = currentAnswer.targetStage;

        if (MuninnMessage.BattleData.resolvedStatus[stageIndex].resolved)
        {
            //hide operator
            BattleEffectManager.instance.HideOperators();
            
            //if I resolve this question
            if (MuninnMessage.BattleData.resolvedStatus[stageIndex].resolvedPersonaID
                .Equals(PassportSDK.CurrentPersona.PersonaID))
            {
                //re-show expressions
                firstRowText.text = currentAnswer.firstRow;
                secondRowText.text = currentAnswer.SecondRow;
                thirdRowText.text = currentAnswer.thirdRow;
            }
        }
        else
        {
            BattleEffectManager.instance.ResetCards();
            //clear answer paper
            StageAnswerManager.ResetAnswer();
        }
        
        //place cards back
        GetComponent<BattleEffectManager>().ResetCards();
        foreach (var cardGroup in twentyFourCardGroup)
        {
            cardGroup.ResetCard(currentAnswer.targetStage);
        }
    }


    /// <summary>
    ///随时更新题库的解答过程
    /// </summary>
    private void Update()
    {
        if (currentAnswer.firstRow != firstRowText.text)
        {
            currentAnswer.firstRow = firstRowText.text;
        }

        if (currentAnswer.SecondRow != secondRowText.text)
        {
            currentAnswer.SecondRow = secondRowText.text;
        }

        if (currentAnswer.thirdRow != thirdRowText.text)
        {
            currentAnswer.thirdRow = thirdRowText.text;
        }
    }
}