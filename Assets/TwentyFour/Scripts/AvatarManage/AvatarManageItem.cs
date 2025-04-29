using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Economy;
using TwentyFour.Scripts.PersonaProperty;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AvatarManageItem : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    public Image Icon;
    public GameObject EquipedHint;

    public CharPartsInventoryItem ItemData;
    [HideInInspector]
    public AvatarManageInfo InfoPanel;
    [HideInInspector]
    public PlayerCharatorManager CharatorManager;

    public Action OnEquiped;

    public RectTransform SelectedHint;

    public void Init(CharPartsInventoryItem itemData,AvatarManageInfo infoPanel,PlayerCharatorManager charatorManager)
    {
        ItemData = itemData;
        InfoPanel = infoPanel;
        CharatorManager = charatorManager;
        Icon.sprite = InventoryHelper.GetItemIcon(itemData.Item.Namespace, itemData.Item.Resource.ResourceSlug);
        EquipedHint.SetActive(false);
        if (AvatarManagePanel.TempAvatar[ItemData.Item.Resource.Namespace] == ItemData.GetKey())
        {
            EquipedHint.SetActive(true);
            InfoPanel.gameObject.SetActive(true);
            InfoPanel.Name.text = ItemData.Item.Resource.DisplayName;
            InfoPanel.Description.text = ItemData.GetDesc();
            InfoPanel.EquipButton.gameObject.SetActive(false);
        }
        
        GetComponent<Button>().onClick.AddListener(RefreshInfoPanel);
        

    }

    void RefreshInfoPanel()
    {
        BGMManager.Instance.PlayAFX(AFXMusic.ButtonEffectClick);
        foreach (var item in transform.parent.GetComponentsInChildren<AvatarManageItem>())
        {
            item.SelectedHint.gameObject.SetActive(false);
        }
        //随机旋转一下z轴
        SelectedHint.transform.DOScale(1.4f, 0.2f).From(1f);
        SelectedHint.transform.DORotate(new Vector3(0,0,UnityEngine.Random.Range(-10,10)), 0.2f).From(new Vector3(0,0,0));
        SelectedHint.gameObject.SetActive(true);
        InfoPanel.gameObject.SetActive(true);
        InfoPanel.Name.text = ItemData.Item.Resource.DisplayName;
        InfoPanel.Description.text = ItemData.GetDesc();
        AvatarManagePanel.TempAvatar[ItemData.Item.Resource.Namespace] = ItemData.GetKey();
        CharatorManager.InitPlayerAvatar(AvatarManagePanel.TempAvatar);
        OnEquiped?.Invoke();
        InfoPanel.gameObject.SetActive(true);
        // InfoPanel.EquipButton.onClick.RemoveAllListeners();
        // InfoPanel.EquipButton.gameObject.SetActive(AvatarManagePanel.TempAvatar[ItemData.Item.Resource.Namespace] != ItemData.GetKey());
        // InfoPanel.EquipButton.onClick.AddListener((() =>
        // {
        //     BGMManager.Instance.PlayAFX(AFXMusic.ButtonEffectClick);
        //     AvatarManagePanel.TempAvatar[ItemData.Item.Resource.Namespace] = ItemData.GetKey();
        //     CharatorManager.InitPlayerAvatar(AvatarManagePanel.TempAvatar);
        //     OnEquiped?.Invoke();
        //     InfoPanel.gameObject.SetActive(true);
        //
        // }));
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {

        
    }
}
