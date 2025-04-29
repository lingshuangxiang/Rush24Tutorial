using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.UOS.Common;
using Unity.UOS.TwentyFour;
using Unity.UOS.TwentyFour.Model;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class TutorialManager : GenericSingleton<TutorialManager>
{
    [Header("教学链")] public List<TutorialJob> TutorialJobs = new();
    [Header("教学字典")] public SerializableDictionary<string, TutorialJob> TutorialJobDictionary = new();

    [Header("UI控件")] public GameObject TutorialPanel;
    public RectTransform GuideGrid;
    public Button NextButton;
    public Text TutorialText;
    public TutorialCursor Cursor;
    public Camera UICamera;

    private readonly Dictionary<RectTransform, List<RectTransform>> layoutGroupRectDictionary = new();
    private int tutorialIndex = -1;
    private Button currentButton;
    private TutorialJob currentTutorialJob;
    private bool isSingleTutorialJob = false;

    public void ShowTutorial(string key, bool setProperties = false)
    {
        Logger.Log($"[Tutorial]开始单个教学 ：{key}");
        if (setProperties)
        {
            SetProperties(key);
        }

        isSingleTutorialJob = true;
        var job = TutorialJobDictionary.Get(key);
        if (job != null)
        {
            ShowTutorial(job);
        }
    }

    public async void SetProperties(string key)
    {
        try
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add(key, false.ToString());
            await PersonaPropertiesHelper.SetPersonaProperties(data);
        }
        catch (Exception e)
        {
            //Debug.Log(e);
        }
    }

    public void StartTutorial(int startIndex = 0)
    {
        Logger.Log($"[TutorialManager]从 {startIndex} 开始任务链");

        ShowTutorial(startIndex);
    }

    void ShowTutorial(int index)
    {
        if (TutorialJobs == null || index >= TutorialJobs.Count)
        {
            Logger.LogError("[TutorialManager]数组越界");
            return;
        }

        tutorialIndex = index;
        var currentJob = TutorialJobs[tutorialIndex];
        ShowTutorial(currentJob);
    }

    void Awake()
    {
        Init();
    }

    void Init()
    {
        NextButton.onClick.AddListener(CheckNextTutorialJob);
        foreach (var jobs in TutorialJobs)
        {
            if (!layoutGroupRectDictionary.ContainsKey(jobs.OriginParentTransform))
            {
                if (jobs.OriginParentTransform != null &&
                    jobs.OriginParentTransform.TryGetComponent<LayoutGroup>(out _))
                {
                    layoutGroupRectDictionary.Add(jobs.OriginParentTransform, new List<RectTransform>());
                    for (int i = 0; i < jobs.OriginParentTransform.childCount; i++)
                    {
                        layoutGroupRectDictionary[jobs.OriginParentTransform]
                            .Add(jobs.OriginParentTransform.GetChild(i).GetComponent<RectTransform>());
                    }
                }
            }
        }
    }

    void ResetLayout()
    {
        foreach (var group in layoutGroupRectDictionary)
        {
            for (int i = 0; i < group.Value.Count; i++)
            {
                group.Value[i].transform.SetSiblingIndex(i);
            }

            group.Key.GetComponent<LayoutGroup>().enabled = true;
        }
    }


    void ShowTutorial(TutorialJob job)
    {
        TutorialPanel.SetActive(true);
        currentTutorialJob = job;
        TutorialText.gameObject.SetActive(true);
        NextButton.gameObject.SetActive(!job.IsMaskingGuidance);
        GuideGrid.gameObject.SetActive(job.IsMaskingGuidance || job.TargetTransform != null);
        Cursor?.Display(job.ShowCursor);
        TutorialText.text = job.ContentText;


        if (job.TargetTransform != null)
        {
            var finalPosition = job.TargetTransform.position + job.Offset;
            GuideGrid.transform.DOMove(finalPosition, job.AnimDuration);
            GuideGrid.sizeDelta = job.TargetTransform.sizeDelta + Vector2.one * job.OutLineSize;

            //计算引导格子右下角的坐标
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(UICamera, finalPosition);
            var newPos = new Vector2(screenPos.x + GuideGrid.sizeDelta.x / 15,
                screenPos.y - GuideGrid.sizeDelta.y / 15);

            RectTransformUtility.ScreenPointToWorldPointInRectangle(TutorialPanel.GetComponent<RectTransform>(), newPos,
                UICamera, out var uiPos);
            Cursor?.DoMove(uiPos, job.AnimDuration);
        }

        if (!job.IsMaskingGuidance || job.TargetTransform == null || job.OriginParentTransform == null)
        {
            return;
        }


        job.TargetTransform.transform.parent = TutorialPanel.transform;

        if (job.OriginParentTransform
            .TryGetComponent<LayoutGroup>(out var layoutGroup))
        {
            layoutGroup.enabled = false;
        }

        if (job.TargetTransform.TryGetComponent<Button>(out var btn))
        {
            btn.onClick.AddListener(OnClickTargetButton);
            currentButton = btn;
        }
    }


    void OnClickTargetButton()
    {
        currentTutorialJob.TargetTransform.transform.parent = currentTutorialJob.OriginParentTransform.transform;
        if (currentButton != null)
        {
            currentButton.onClick.RemoveListener(OnClickTargetButton);
            currentButton = null;
        }

        CheckNextTutorialJob();
    }

    void CheckNextTutorialJob()
    {
        if (isSingleTutorialJob)
        {
            Logger.Log("[Tutorial]结束单个教学");
            TutorialPanel.SetActive(false);
            Cursor?.Display(false);
            TutorialText.gameObject.SetActive(false);
            DisposeData();
            ResetLayout();
        }
        else
        {
            tutorialIndex++;
            if (tutorialIndex < TutorialJobs.Count)
                ShowTutorial(tutorialIndex);
            else
            {
                Logger.Log("[TutorialManager]结束教学链");
                TutorialPanel.SetActive(false);
                Cursor.Display(false);
                TutorialText.gameObject.SetActive(false);
                DisposeData();
                ResetLayout();
            }
        }
    }

    void DisposeData()
    {
        currentTutorialJob = null;
        currentButton = null;
        tutorialIndex = -1;
        isSingleTutorialJob = false;
    }
}

[System.Serializable]
public class TutorialJob
{
    public bool IsMaskingGuidance;
    public bool ShowCursor;
    public string ContentText;
    public RectTransform TargetTransform;
    public RectTransform OriginParentTransform;
    public int OutLineSize = 50;
    public Vector3 Offset;
    public float AnimDuration = 0.5f;
    public UnityAction OnJobCompletedAction;
}