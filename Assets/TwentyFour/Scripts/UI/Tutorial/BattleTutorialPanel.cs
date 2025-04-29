using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleTutorialPanel : MonoBehaviour
{
    public List<GameObject> TutorialItems = new();
    public Button NextBtn;
    public Button PreviousBtn;
    public Button CompleteBtn;

    private int currentIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        NextBtn.onClick.AddListener(OnClickNextBtn);
        PreviousBtn.onClick.AddListener(OnClickPreviousBtn);
    }

    void OnEnable()
    {
        foreach (GameObject item in TutorialItems)
        {
            item.SetActive(false);
        }
        currentIndex = 0;
        TutorialItems[currentIndex].SetActive(true);
        NextBtn.gameObject.SetActive(true);
        PreviousBtn.gameObject.SetActive(false);
        CompleteBtn.gameObject.SetActive(false);
    }

    void OnClickNextBtn()
    {
        TutorialItems[currentIndex++].SetActive(false);

        if (currentIndex >= TutorialItems.Count - 1)
        {
            NextBtn.gameObject.SetActive(false);
            CompleteBtn.gameObject.SetActive(true);
            currentIndex = TutorialItems.Count - 1;
        }
        else
        {
            PreviousBtn.gameObject.SetActive(true);
        }

        TutorialItems[currentIndex].SetActive(true);
    }

    void OnClickPreviousBtn()
    {
        TutorialItems[currentIndex--].SetActive(false);
        if (currentIndex <= 0)
        {
            PreviousBtn.gameObject.SetActive(false);
            currentIndex = 0;
        }
        else
        {
            NextBtn.gameObject.SetActive(true);
            CompleteBtn.gameObject.SetActive(false);
        }

        TutorialItems[currentIndex].SetActive(true);
    }

    public void CheckShowBattleTutorialPanel()
    {
        if (PersonaPropertiesHelper.ShowBattleTutorial)
        {
            gameObject.SetActive(true);
        }
    }

    public void CheckBattleTutorial()
    {
        if (PersonaPropertiesHelper.ShowBattleTutorial)
        {
            TutorialManager.Instance.ShowTutorial("show_battle_tutorial", true);
            PersonaPropertiesHelper.ShowBattleTutorial = false;
        }
    }
}