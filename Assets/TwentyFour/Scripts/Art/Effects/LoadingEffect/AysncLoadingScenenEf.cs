using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Logger = TwentyFour.Scripts.Utilities.Logger;
using Unity.UOS.TwentyFour.UOSGateway;

public class AysncLoadingScenenEf : MonoBehaviour
{
    public static AysncLoadingScenenEf instance;

    public static string WaitToLoadScene = "WaitToLoadScene";
    public static string WaitToUnLoadScene = "WaitToLoadScene";
    public static Action OnLoadOperationCompletedAction;
    
    public static Action OnUnloadLoadingCompletedAction;

    static bool isLoading;
    
    public Image LoadImage;

    public Ease InEase;
    public Ease OutEase;
    
    public float InDuration;
    public float OutDuration;

    private static bool fetchData;

    void PlayIn()
    {
        LoadImage.DoImagePixelsPerUnitMultiplier(1, 0, InDuration, InEase, (AfterMaskCurrentScene));
    }

    void PlayOut()
    {
        LoadImage.DoImagePixelsPerUnitMultiplier(0, 1, OutDuration, OutEase, (UnloadThisAnimScene));
    }

    private void OnEnable()
    {
        LoadImage.sprite = PlayerCharatorManager.GetLocalPlayerLoadingMaskSprite();
        PlayIn();
    }

    public static void LoadScene(string UnloadScene, string ToLoadScene,bool fetch = true)
    {
        if(isLoading) return;
        WaitToUnLoadScene = UnloadScene;
        WaitToLoadScene = ToLoadScene;
        fetchData = fetch;
        isLoading = true;
        SceneManager.LoadScene("LoadingSceneAnim", LoadSceneMode.Additive);
    }

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 当动画全部覆盖了现有场景后
    /// </summary>
    public void AfterMaskCurrentScene()
    {
        StartCoroutine(loadSequence());
    }

    private AsyncOperation _loadDestinationAsyncOperation;


    /// <summary>
    /// 全部的加载过程
    /// </summary>
    /// <returns></returns>
    IEnumerator loadSequence()
    {
        if(fetchData) FetchUserData();
        fetchData = false;
        Logger.Log("[AysncLoadingScenenEf] Fetch User Data");
        
        yield return LoadDestinationScene();
        yield return UnloadScene();
        Logger.Log("is loading scene Fx");
        PlayOut();
    }


    IEnumerator LoadDestinationScene()
    {
        _loadDestinationAsyncOperation = SceneManager.LoadSceneAsync(WaitToLoadScene, LoadSceneMode.Additive);
        _loadDestinationAsyncOperation.completed += OnLoadOperationComplete;
        while (_loadDestinationAsyncOperation.progress < 1)
        {
            yield return null;
        }
    }

    protected virtual void OnLoadOperationComplete(AsyncOperation obj)
    {
        OnLoadOperationCompletedAction?.Invoke();
    }
    
    
    protected virtual void OnUnLoadLoadingCompleted(AsyncOperation obj)
    {
        isLoading = false;
        OnUnloadLoadingCompletedAction?.Invoke();
    }
    

    IEnumerator UnloadScene()
    {
        AsyncOperation tempc = SceneManager.UnloadSceneAsync(WaitToUnLoadScene);
        while (tempc.progress < 1)
        {
            yield return null;
        }
    }

    void FetchUserData()
    {
        // var remoteConfigAwaiter = RemoteConfigHelper.GetDefaultRemoteConfig().GetAwaiter();
        // yield return new WaitUntil(() => remoteConfigAwaiter.IsCompleted);
        var leaderboardCount = 20;
        var count = leaderboardCount == 0 ? 20 : leaderboardCount;
    }

    /// <summary>
    /// 当动画加载完后卸载本过场动画场景,由动画调用
    /// </summary>
    public void UnloadThisAnimScene()
    {
        var op = SceneManager.UnloadSceneAsync("LoadingSceneAnim");
        op.completed += OnUnLoadLoadingCompleted;
    }
}