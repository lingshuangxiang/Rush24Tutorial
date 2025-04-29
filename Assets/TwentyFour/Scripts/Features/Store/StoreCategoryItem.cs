using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Economy;
using UnityEngine;
using UnityEngine.UI;

public class StoreCategoryItem : MonoBehaviour
{
    public Category CategoryData;
    public Text CategoryName;
    public Button CategoryButton;

    public Color SelectedColor;
    public Color NormalColor;
    public RectTransform CategoryItem;
    public RectTransform StartPos;
    public RectTransform EndPos;
    public Tweener InfoTweener;

    StorePanel ParentStorePanel;
    // Start is called before the first frame update
    public void Init(Category category,StorePanel storePanel)
    {
        ParentStorePanel = storePanel;
        Reset();
        CategoryData = category;
        CategoryName.text = category.DisplayName;
        CategoryButton.onClick.RemoveAllListeners();
        CategoryButton.onClick.AddListener(OnClick);

    }

    public void OnClick()
    {
        ParentStorePanel.StoreSlugName = CategoryData.SlugName;
        ParentStorePanel.ResetDetail();
        ParentStorePanel.RefreshItemList();
        ParentStorePanel.RefreshCategoryInfos();
        ResetOthers();
        InfoTweener = CategoryItem.DOLocalMoveX(EndPos.localPosition.x, 0.2f).From(StartPos.localPosition.x);
        CategoryButton.image.color = SelectedColor;
        
    }
    public void Reset()
    {
        InfoTweener?.Kill();
        CategoryItem.DOLocalMoveX(StartPos.localPosition.x, 0);
        CategoryButton.image.color = NormalColor;
    }
    void ResetOthers()
    {
        foreach (var item in transform.parent.GetComponentsInChildren<StoreCategoryItem>())
        {
            item.Reset();
        }
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
