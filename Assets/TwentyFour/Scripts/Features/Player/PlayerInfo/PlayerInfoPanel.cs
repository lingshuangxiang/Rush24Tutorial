using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Leaderboard;
using TMPro;
using TwentyFour.Scripts.PersonaProperty;
using Unity.Passport.Runtime;
using Unity.UOS.TwentyFour;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class PlayerInfoPanel : MonoBehaviour
{
    public Text PlayerName;
    public Text PlayerNameInHall;

    public TiersBadge TierBadge;

    public TextMeshProUGUI ClearStageCount;

    public TextMeshProUGUI AverageTime;
    public CanvasGroup PanelCanvasGroup;
    public GameObject ButtonLogout;

    public PlayerCharatorManager CharatorManager;

    public Toggle TogglBGM;
    public Toggle TogglAFX;

    public Button WeChatButton;

    public CanvasGroup InfoCanvasGroup;
    public RectTransform AvatarBoard;
    public RectTransform InfoBoard;
    Vector3 AvatarBoardPos;
    Vector3 InfoBoardPos;
    public RectTransform AvatarEnd;
    public RectTransform InfoEnd;

    public GameObject EditorAvatarButton;
    public GameObject SettingComponent;
    public Button AccomplishmentButton;
    private void Awake()
    {
        AvatarBoardPos = AvatarBoard.position;
        InfoBoardPos = InfoBoard.position;
    }

    public void DoFadeOut()
    {
        InfoCanvasGroup.interactable = false;
        AvatarBoard.DOMove(AvatarEnd.position, 0.4f).SetEase(Ease.InBack).From(AvatarBoard.position);
        InfoBoard.DOMove(InfoEnd.position, 0.4f).SetEase(Ease.InBack).From(InfoBoard.position);
        InfoCanvasGroup.DOFade(0, 0.4f).SetEase(Ease.InBack).OnComplete((() => { gameObject.SetActive(false); }));
    }

    private void OnEnable()
    {
        InfoCanvasGroup.interactable = true;

        AvatarBoard.position = AvatarBoardPos;
        InfoBoard.position = InfoBoardPos;
        InfoCanvasGroup.alpha = 1;
        SetFuncButtons(true);
        GetPlayerInfo();

    }

    public void ShowSelfInfo()
    {
        gameObject.SetActive(true);
    }

    void SetFuncButtons(bool enable)
    {
        WeChatButton.gameObject.SetActive(enable);
        EditorAvatarButton.SetActive(enable);
        SettingComponent.SetActive(enable);
        AccomplishmentButton.gameObject.SetActive(enable);
    }
    private void OnDisable()
    {

    }

    private void GetPlayerInfo()
    {
        TogglAFX.isOn = !BGMManager.Instance.AFXSource.mute;
        TogglBGM.isOn = !BGMManager.Instance.audioSource.mute;
        CharatorManager.InitPlayerAvatar(PersonaPropertiesHelper.GetLocalProperties());
        PlayerName.text = Identity.persona?.DisplayName;

        if (TiersHelper.LocalTierUserScoreData != null)
        {
            Logger.Log("LocalTierUserScoreData");
            var data = TiersHelper.LocalTierUserScoreData;
            TierBadge.SetupBadge(false, data.highestScore, data.highestTier);
            float average = 0f;
            if (data.totalResolved> 0)
                average = data.totalTime / data.totalResolved;
            ClearStageCount.text = data.totalResolved.ToString();
            AverageTime.text = Math.Round(average, 1).ToString("F1");
        }
        else
        {
            var (highestScore, tier) = PersonaPropertiesHelper.GetBattleHighestScoreAndTier();

            TierBadge.SetupBadge(false, highestScore, tier);

            var resolvedCount = PersonaPropertiesHelper.GetProperty(PersonaPropertyKeys.TotalSolvedCountKey, 0);
            var usedTime = PersonaPropertiesHelper.GetProperty(PersonaPropertyKeys.TotalTimeUsedKey, 0f);
            float average = 0f;
            if (resolvedCount > 0)
                average = usedTime / resolvedCount;
            ClearStageCount.text = resolvedCount.ToString();
            AverageTime.text = Math.Round(average, 1).ToString("F1");
        }
        
    }

    public void ShowPlayerInfo(string personaId,LeaderboardMemberScore score)
    {
        UIManager.Instance.StartCoroutine(GetPlayerInfoIEnumerator(personaId,score));
    }

    IEnumerator GetPlayerInfoIEnumerator(string personaId,LeaderboardMemberScore score)
    {
        UIManager.Instance.ShowCommonLoading("正在加载");

        var url =
            $"{UosAppConfigs.GetBaseStatelessUrl()}player_info?uniqueId={personaId}";
        //Debug.LogError(url);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Logger.LogError("Error: " + webRequest.error);
            }
            else
            {
                string jsonString = webRequest.downloadHandler.text;
                var data = JsonUtility.FromJson<TierUserScoreData>(jsonString);
                gameObject.SetActive(true);
                SetFuncButtons(false);
                PlayerName.text = score.DisplayName;
                CharatorManager.InitPlayerAvatar(score.PersonaProperties.ToDictionary(x => x.Key, x => x.Value));
                TierBadge.SetupBadge(false, data.highestScore, data.highestTier);
                float average = 0f;
                if (data.totalResolved> 0)
                    average = data.totalTime / data.totalResolved;
                ClearStageCount.text = data.totalResolved.ToString();
                AverageTime.text = Math.Round(average, 1).ToString("F1");
            }
            UIManager.Instance.HideCommonLoading();
        }

    }
}