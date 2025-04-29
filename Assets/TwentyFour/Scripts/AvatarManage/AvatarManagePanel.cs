using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Economy;
using TwentyFour.Scripts.Metrics;
using TwentyFour.Scripts.PersonaProperty;
using Unity.Passport.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

public class AvatarManagePanel : MonoBehaviour
{
    public Toggle HeadButton;
    public Toggle EyeButton;
    public Toggle MouthButton;
    public Toggle HeadWearButton;
    public ToggleGroup TabGroup;

    public GameObject ItemPrefab;
    public RectTransform ItemParent;
    public AvatarManageInfo InfoPanel;

    
    List<CharPartsInventoryItem> CurrentTabItems = new List<CharPartsInventoryItem>();

    public PlayerCharatorManager CharatorManager;
    public static Dictionary<string,string> TempAvatar = new Dictionary<string, string>();
    public static Dictionary<string,string> OldAvatar = new Dictionary<string, string>();

    public CanvasGroup AvatarCanvas;
    
    public Button SaveButton;
    public Button ResetButton;
    public Button ExitButton;
    public GameObject ChangeAvatarPanel;

    public RectTransform HightLight;
    public RectTransform HightStart;
    public RectTransform HightEnd;


    public RectTransform AvatarStart;
    public RectTransform InfoStart;
    public RectTransform AvatarBoard;
    public RectTransform InfoBoard;
    Vector3 AvatarBoardPos;
    Vector3 InfoBoardPos;
    Vector3 AvatarPos;
    private void Awake()
    {
        AvatarBoardPos = AvatarBoard.position;
        InfoBoardPos = InfoBoard.position;
        AvatarPos = CharatorManager.gameObject.transform.localPosition;
    }

    void DoFadeIn()
    {
        //AvatarCanvas.interactable = false;
        AvatarBoard.position = AvatarBoardPos;
        InfoBoard.position = InfoBoardPos;
        AvatarBoard.DOMove(AvatarBoardPos, 0.3f).SetEase(Ease.InBack).From(AvatarStart.position).SetDelay(0.6f);
        InfoBoard.DOMove(InfoBoardPos, 0.3f).SetEase(Ease.InBack).From(InfoStart.position).SetDelay(0.6f).OnComplete((
            () =>
            {
                CharatorManager.transform.DOLocalMoveX(0, 0.3f).From(AvatarPos).SetEase(Ease.OutBack).SetDelay(0.5f);
                //AvatarCanvas.interactable = true;

            }));
    }
    private void OnEnable()
    {
        CharatorManager.transform.localPosition = AvatarPos;
        DoFadeIn();
        TempAvatar?.Clear();
        var localAvatar = PersonaPropertiesHelper.GetLocalProperties();
        if (localAvatar.TryGetValue(PersonaPropertyKeys.ActiveAvatarEyeKey, out var eye))
        {
            TempAvatar[PersonaPropertyKeys.ActiveAvatarEyeKey] = eye;
        }
        else
        {
            TempAvatar[PersonaPropertyKeys.ActiveAvatarEyeKey] = "CharBlackBall";
        }

        if (localAvatar.TryGetValue(PersonaPropertyKeys.ActiveAvatarHeadKey, out var head))
        {
            TempAvatar[PersonaPropertyKeys.ActiveAvatarHeadKey] = head;
        }
        else
        {
            TempAvatar[PersonaPropertyKeys.ActiveAvatarHeadKey] = "CharBlackBall";
        }

        if (localAvatar.TryGetValue(PersonaPropertyKeys.ActiveAvatarMouthKey, out var mouth))
        {
            TempAvatar[PersonaPropertyKeys.ActiveAvatarMouthKey] = mouth;
        }
        else
        {
            TempAvatar[PersonaPropertyKeys.ActiveAvatarMouthKey] = "CharBlackBall";
        }
        if (localAvatar.TryGetValue(PersonaPropertyKeys.ActiveAvatarHeadwearKey, out var headwear))
        {
            TempAvatar[PersonaPropertyKeys.ActiveAvatarHeadwearKey] = headwear;
        }
        else
        {
            TempAvatar[PersonaPropertyKeys.ActiveAvatarHeadwearKey] = "CharBlackBall";
        }
        OldAvatar = TempAvatar.ToDictionary(x=>x.Key,x=>x.Value);
        CharatorManager.InitPlayerAvatar(TempAvatar);
        InfoPanel.gameObject.SetActive(false);

        HeadButton.onValueChanged.AddListener(OnHeadClicked);
        EyeButton.onValueChanged.AddListener(OnEyeClicked);
        MouthButton.onValueChanged.AddListener(OnMouthClicked);
        HeadWearButton.onValueChanged.AddListener(OnHeadWearClicked);
        HeadButton.isOn = true;
        SaveButton.onClick.RemoveAllListeners();
        SaveButton.onClick.AddListener(SaveAvatar);
        SaveButton.interactable = false;

        
        ResetButton.onClick.RemoveAllListeners();
        ResetButton.onClick.AddListener(ResetAvatar);
        
        StopAllCoroutines();
        StartCoroutine(MoveHightLight());
    }

    

    private void ResetAvatar()
    {
        TempAvatar = OldAvatar.ToDictionary(x=>x.Key,x=>x.Value);
        CharatorManager.InitPlayerAvatar(TempAvatar);
        SaveButton.interactable = false;
        RefreshItems(currentKey);
    }

    private YieldInstruction waitSeconds = new WaitForSeconds(4f);
    IEnumerator MoveHightLight()
    {
        HightLight.gameObject.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        HightLight.gameObject.SetActive(true);

        HightLight.DOMove(HightEnd.position, 1f).From(HightStart.position).SetEase(Ease.Linear);
        yield return new WaitForSeconds(2f);
        while (true)
        {
            

            HightLight.DOMove(HightEnd.position, 1f).From(HightStart.position).SetEase(Ease.Linear);
            yield return waitSeconds;
        }
    }
    private void SaveAvatar()
    {
        StartCoroutine(UploadAvatar());
    }

    IEnumerator UploadAvatar()
    {
        uploadSucceed = true;
        AvatarBoard.gameObject.SetActive(false);
        InfoBoard.gameObject.SetActive(false);
        UIManager.Instance.ShowCommonLoading("正在保存形象");
        var p = PersonaPropertiesHelper.SetPersonaProperties(TempAvatar,failedcallback: OnUploadFailed);
        yield return new WaitUntil(() => p.IsCompleted);
        UIManager.Instance.HideCommonLoading();
        if (uploadSucceed)
        {
            var dic = new Dictionary<string, object>();
            foreach (var kv in TempAvatar)
            {
                dic[kv.Key] = kv.Value;
            }
            MetricsHelper.TrackEvent(MetricsKeys.EVENT_CHANGE_AVATAR, dic);
            SaveButton.interactable = false;
            ChangeAvatarPanel.GetComponent<ChangeAvatarPanel>().OnExit += OnExit;
            ChangeAvatarPanel?.gameObject.SetActive(true);
            OldAvatar = TempAvatar.ToDictionary(x=>x.Key,x=>x.Value);
            InfoPanel.EquipButton.gameObject.SetActive(false);
        }
        else
        {
            UIMessage.Show("保存失败，请稍后再试");
            AvatarBoard.gameObject.SetActive(true);
            InfoBoard.gameObject.SetActive(true);
        }
        
        
    }

    private bool uploadSucceed = false;
    void OnUploadFailed()
    {
        uploadSucceed = false;
    }

    private void OnExit()
    {
        AvatarBoard.gameObject.SetActive(true);
        InfoBoard.gameObject.SetActive(true);
        ExitButton.onClick.Invoke();
        
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        currentKey = PersonaPropertyKeys.ActiveAvatarHeadKey;
        HeadButton.isOn = false;
        EyeButton.isOn = false;
        MouthButton.isOn = false;
        HeadWearButton.isOn = false;
    }

    private void OnHeadClicked(bool v)
    {
        if (v)
        {
            //BGMManager.Instance.PlayAFX(AFXMusic.ButtonEffectClick);
            RefreshItems(PersonaPropertyKeys.ActiveAvatarHeadKey);
        }
        
    }

    private void OnEyeClicked(bool v)
    {
        if (v)
        {
            //BGMManager.Instance.PlayAFX(AFXMusic.ButtonEffectClick);
            RefreshItems(PersonaPropertyKeys.ActiveAvatarEyeKey);
        }
    }

    private void OnMouthClicked(bool v)
    {
        if (v)
        {
            //BGMManager.Instance.PlayAFX(AFXMusic.ButtonEffectClick);
            RefreshItems(PersonaPropertyKeys.ActiveAvatarMouthKey);
        }
    }
    private void OnHeadWearClicked(bool v)
    {
        if (v)
        {
            //BGMManager.Instance.PlayAFX(AFXMusic.ButtonEffectClick);
            RefreshItems(PersonaPropertyKeys.ActiveAvatarHeadwearKey);
        }
    }

    private string currentKey;
    void RefreshItems(string Key)
    {
        SaveButton.GetComponentInChildren<Text>().color = new Color(1, 1, 1, canSave() ? 1 : 0.15f);
        currentKey = Key;
        InfoPanel.gameObject.SetActive(false);

        int currentCount = ItemParent.childCount;
        for (int i = 0; i < currentCount; i++)
        {
            Destroy(ItemParent.GetChild(i).gameObject);
        }
        CurrentTabItems?.Clear();
        foreach (var item in InventoryHelper.CharAvatarPartsList)
        {
            if (item.Item.Namespace == Key)
            {
                CurrentTabItems?.Add(item);
            }
        }
        foreach (var item in CurrentTabItems)
        {
            var avatar = Instantiate(ItemPrefab, ItemParent).GetComponent<AvatarManageItem>();
            avatar.Init(item,InfoPanel,CharatorManager);
            avatar.OnEquiped += (() =>
            {
                RefreshItems(currentKey);
                SaveButton.GetComponentInChildren<Text>().color = new Color(1, 1, 1, canSave() ? 1 : 0.15f);
                SaveButton.interactable = canSave();
            });
        }
    }

    bool canSave()
    {
        if (TempAvatar[PersonaPropertyKeys.ActiveAvatarMouthKey] !=
            OldAvatar[PersonaPropertyKeys.ActiveAvatarMouthKey] ||
            TempAvatar[PersonaPropertyKeys.ActiveAvatarEyeKey] != OldAvatar[PersonaPropertyKeys.ActiveAvatarEyeKey] ||
            TempAvatar[PersonaPropertyKeys.ActiveAvatarHeadKey] != OldAvatar[PersonaPropertyKeys.ActiveAvatarHeadKey] ||
            TempAvatar[PersonaPropertyKeys.ActiveAvatarHeadwearKey]!= OldAvatar[PersonaPropertyKeys.ActiveAvatarHeadwearKey])
        {
            return true;
        }
        return false;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
