using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Leaderboard;
using TMPro;
using TwentyFour.Scripts.RemoteConfig;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TiersLeaderboard : MonoBehaviour, LoopScrollPrefabSource, LoopScrollDataSource
{

    public GameObject PlayerRankInfoPrefab;
    public GameObject EmptyHint;
    public LoopScrollRect LeaderboardScrollRect;
    public string LeaderboardSlugName;
    private void OnEnable()
    {
        StopCoroutine(ResetVertical());
        StartCoroutine(ResetVertical());
        UpdateTiersLeaderboard();
    }

    /// <summary>
    /// 重新刷新和显示leaderBoard
    /// </summary>
    public void UpdateTiersLeaderboard()
    {
        // 获取当前排行榜项的数量
        int targetCount = 0;
        if (TiersHelper.AllLeaderboardScoresResponse.TryGetValue(LeaderboardSlugName,
                out var leaderboardScoresResponse))
        {
            targetCount = leaderboardScoresResponse.Scores.Count;
        }

        var isEmpty = targetCount == 0;
        if(EmptyHint)
            EmptyHint.SetActive(isEmpty);
        LeaderboardScrollRect.gameObject.SetActive(!isEmpty);
        LeaderboardScrollRect.totalCount = targetCount;
        LeaderboardScrollRect.prefabSource = this;
        LeaderboardScrollRect.dataSource = this;
        LeaderboardScrollRect.RefillCells();

    }

    IEnumerator ResetVertical()
    {
        LeaderboardScrollRect.vertical = false;
        yield return new WaitForSeconds(0.5f);
        LeaderboardScrollRect.vertical = true;
    }
    

    // Implement your own Cache Pool here. The following is just for example.
    Stack<Transform> pool = new Stack<Transform>();
    public GameObject GetObject(int index)
    {
        if (pool.Count == 0)
        {
            var item = Instantiate(PlayerRankInfoPrefab, transform);
            item.GetComponent<LeaderBoardItem>().LeaderboardSlugName = LeaderboardSlugName;
            return item;
        }
        Transform candidate = pool.Pop();
        candidate.gameObject.GetComponent<LeaderBoardItem>().LeaderboardSlugName = LeaderboardSlugName;
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

    void Start()
    {
        
    }

}
