using System;
using System.Collections;
using System.Collections.Generic;
using TwentyFour.Scripts.Accomplishment;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AccomplishmentPanel : MonoBehaviour, LoopScrollPrefabSource, LoopScrollDataSource
{
    public GameObject AccomplishmentItemPrefab;
    
    public LoopScrollRect AccomplishmentScrollRect;

    public GameObject EmptyHint;
    
    private void OnEnable()
    {
        StartCoroutine(StartFetchData());
    }

    IEnumerator StartFetchData()
    {
        UIManager.Instance.ShowCommonLoading("正在加载中");
        AccomplishmentScrollRect.totalCount = 0;
        AccomplishmentScrollRect.prefabSource = this;
        AccomplishmentScrollRect.dataSource = this;
        AccomplishmentScrollRect.RefillCells();
        yield return AccomplishmentHelper.GetData(Identity.persona.PersonaID);
        UIManager.Instance.HideCommonLoading();
        RefreshPanel();
    }
    void RefreshPanel()
    {
        // 获取当前排行榜项的数量
        int targetCount = AccomplishmentHelper.LocalAccomplishmentData.data.Count;
        var empty = targetCount == 0;
        EmptyHint.SetActive(empty);
        AccomplishmentScrollRect.gameObject.SetActive(!empty);
        AccomplishmentScrollRect.totalCount = targetCount;
        AccomplishmentScrollRect.prefabSource = this;
        AccomplishmentScrollRect.dataSource = this;
        AccomplishmentScrollRect.RefillCells();
    }
    // Implement your own Cache Pool here. The following is just for example.
    Stack<Transform> pool = new Stack<Transform>();
    public GameObject GetObject(int index)
    {
        if (pool.Count == 0)
        {
            return Instantiate(AccomplishmentItemPrefab);
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
