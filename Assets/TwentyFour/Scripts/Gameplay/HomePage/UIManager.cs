using System;
using System.Collections;
using System.Collections.Generic;
using Unity.UOS.Common;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : GenericSingleton<UIManager>
{
    private PopUpPanel popUpPanel;
    private CommonLoadingPanel commonLoadingPanel;
    public PlayerInfoPanel PlayerInfoPanelInstance;
    private GetItemPanel getItemPanelInstance;
    Canvas canvas;

    public void Init()
    {
        if (!gameObject.TryGetComponent<Canvas>(out canvas))
        {
            canvas = gameObject.AddComponent<Canvas>();
            var graphics = canvas.gameObject.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            scaler.referenceResolution = new Vector2(2560, 1440);
        }
        
    }
    public void ShowPopUp(string title, string description, Action onConfirm = null, Action onCancel = null, bool cancelButtonActive = true)
    {
        if (popUpPanel == null)
        {
            var go = Instantiate(Resources.Load<GameObject>("UI/Popup_Panel"), transform);
            DontDestroyOnLoad(go);
            popUpPanel = go.GetComponent<PopUpPanel>();
        }
        canvas.worldCamera = Camera.main;
        popUpPanel.CancelButton.gameObject.SetActive(cancelButtonActive);
        popUpPanel?.Show(title, description, onConfirm, onCancel);
        
    }

    public void ShowCommonLoading(string text)
    {
        if (commonLoadingPanel == null)
        {
            var go = Instantiate(Resources.Load<GameObject>("UI/Loading_Panel"), transform);
            DontDestroyOnLoad(go);
            commonLoadingPanel = go.GetComponent<CommonLoadingPanel>();
            
        }
        canvas.worldCamera = Camera.main;
        commonLoadingPanel?.Show(text);
    }

    public void ShowGetItemPanel(GetItemParams data)
    {
        if (getItemPanelInstance == null)
        {
            var go = Instantiate(Resources.Load<GameObject>("UI/GetItemPanel"), transform);
            DontDestroyOnLoad(go);
            getItemPanelInstance = go.GetComponent<GetItemPanel>();
        }
        canvas.worldCamera = Camera.main;
        getItemPanelInstance?.PlayEffect(data);
    }
    public void ShowGetItemPanel(List<GetItemData> data,string title = "获得物品")
    {
        var dataParam = new GetItemParams()
        {
            GetItemDataList = data,
            Title = title,
        };
        ShowGetItemPanel(dataParam);
    }

    public void HideCommonLoading()
    {
        if(commonLoadingPanel != null)
            commonLoadingPanel.Hide();
    }
}
