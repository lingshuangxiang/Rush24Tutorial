using System;
using System.Collections;
using System.Collections.Generic;
using TwentyFour.Scripts.Metrics;
using TwentyFour.Scripts.Tournament;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InBoxPanel : MonoBehaviour, LoopScrollPrefabSource, LoopScrollDataSource
{
    public GameObject InBoxItemPrefab;
    public LoopScrollRect InBoxScrollRect;
    public GameObject EmptyHint;
    public RectTransform MailDetail;
    public Text MailContentText;
    public Text FromText;
    public Button RedeemButton;
    public Text RedeemButtonText;
    public int SelectedIndex = -1;
    public Button DeleteAllButton;
    public Button RedeemAllButton;
    
    private void Awake()
    {

    }

    private void OnEnable()
    {
        UIManager.Instance.InBoxPanelInstance = this;

        UIManager.Instance.ShowCommonLoading("正在加载中...");
        StopCoroutine(ResetVertical());
        StartCoroutine(ResetVertical());
        StartCoroutine(FetchData());
        DeleteAllButton.onClick.RemoveAllListeners();
        DeleteAllButton.onClick.AddListener(OnDeleteAll);
        RedeemAllButton.onClick.RemoveAllListeners();
        RedeemAllButton.onClick.AddListener(OnRedeemAll);
        MailDetail.gameObject.SetActive(false);
        
    }

    private void OnRedeemAll()
    {
        UIManager.Instance.ShowCommonLoading("正在领取");
        StartCoroutine(RedeemAll());
    }

    private void OnDeleteAll()
    {
        UIManager.Instance.ShowPopUp("确认删除",$"确认删除所有已读邮件吗？",(
            () =>
            {
                UIManager.Instance.ShowCommonLoading("正在删除");
                StartCoroutine(DeleteAll());
            }));
        
    }

    private void OnDisable()
    {
        SelectedIndex = -1;
    }

    public void OnClickMailDetailClose()
    {
        MailDetail.gameObject.SetActive(false);
        SelectedIndex = -1;
        foreach (var item in InBoxScrollRect.GetComponentsInChildren<InBoxItem>())
        {
            item.Reset();
        }
    }
    public void RefreshPanel()
    {
        // 获取当前排行榜项的数量
        int targetCount = InBoxHelper.InBoxMessages.Count;
        var isEmpty = targetCount == 0;
        if(EmptyHint)
            EmptyHint.SetActive(isEmpty);
        InBoxScrollRect.gameObject.SetActive(!isEmpty);
        DeleteAllButton.interactable = !isEmpty;
        RedeemAllButton.interactable = !isEmpty;
        InBoxScrollRect.totalCount = targetCount;
        InBoxScrollRect.prefabSource = this;
        InBoxScrollRect.dataSource = this;
        InBoxScrollRect.RefillCells();
    }

    IEnumerator FetchData()
    {
        var fetch = InBoxHelper.ReceiveMessages().GetAwaiter();
        yield return new WaitUntil(() => fetch.IsCompleted);
        var data = InBoxHelper.ViewInbox().GetAwaiter();
        yield return new WaitUntil(() => data.IsCompleted);
        
        RefreshPanel();
        UIManager.Instance.HideCommonLoading();

    }

    bool canPlayGetItemEffect = false;
    IEnumerator RedeemAll()
    {
        canPlayGetItemEffect = true;
        UIManager.Instance.ShowCommonLoading("正在领取");
        MailDetail.gameObject.SetActive(false);
        SelectedIndex = -1;
        var redeemall = InBoxHelper.ConsumeAllMessages(RedeemFailed).GetAwaiter();
        yield return new WaitUntil(() => redeemall.IsCompleted);
        yield return FetchData();
        UIManager.Instance.ShowCommonLoading("正在领取");
        var itemList = new List<GetItemData>();
        foreach (var message in redeemall.GetResult().Messages)
        {
            foreach (var reward in message.Attachment)
            {
                var data = new GetItemData();
                data.DisplayName = reward.Resource.DisplayName;
                data.Slug = reward.ItemSlug;
                data.Count = reward.Quantity;
                data.Namespace = reward.Resource.Namespace;
                itemList.Add(data);
                MetricsHelper.TrackEvent(MetricsKeys.EVENT_REDEEM_ITEM,new Dictionary<string, object>()
                {
                    {MetricsKeys.PARAM_REWARD_TYPE,reward.ItemSlug},
                    {MetricsKeys.PARAM_FROM,message.Id},
                    {MetricsKeys.PARAM_REDEEM_TYPE,"inbox"}
                });
            }
        }
        
        if (canPlayGetItemEffect)
        {
            var updateInventory = InventoryHelper.ListPersonaInventory();
            yield return TournamentDataHelper.FetchTournamentData();
            yield return new WaitUntil(() => updateInventory.IsCompleted);
            
            UIManager.Instance.HideCommonLoading();
            if (itemList.Count > 0)
            {
                UIManager.Instance.ShowGetItemPanel(itemList);
            }
            
        }

    }
    void RedeemFailed(Exception e)
    {
        canPlayGetItemEffect = false;
        PassportException passportException = e as PassportException;
        UIMessage.Show(passportException?.ErrorMessage);
        UIManager.Instance.HideCommonLoading();
    }
    IEnumerator DeleteAll()
    {
        MailDetail.gameObject.SetActive(false);
        SelectedIndex = -1;
        var deleteall = InBoxHelper.DeleteAllMessages().GetAwaiter();
        yield return new WaitUntil(() => deleteall.IsCompleted);
        yield return FetchData();
        
        
    }
    IEnumerator ResetVertical()
    {
        InBoxScrollRect.vertical = false;
        yield return new WaitForSeconds(0.5f);
        InBoxScrollRect.vertical = true;
    }
    // Implement your own Cache Pool here. The following is just for example.
    Stack<Transform> pool = new Stack<Transform>();
    public GameObject GetObject(int index)
    {
        if (pool.Count == 0)
        {
            var item = Instantiate(InBoxItemPrefab, transform);
            return item;
        }
        Transform candidate = pool.Pop();
        candidate.gameObject.SetActive(true);
        return candidate.gameObject;
    }

    public void ReturnObject(Transform trans)
    {
        // Use `DestroyImmediate` here if you don't need Pool
        trans.SendMessage("ScrollCellReturn", SendMessageOptions.DontRequireReceiver);
        trans.gameObject.SetActive(false);
        trans.SetParent(transform, false);
        pool.Push(trans);
    }

    public void ProvideData(Transform transform, int idx)
    {
        transform.SendMessage("ScrollCellIndex", idx);
    }
}
