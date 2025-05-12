using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetItemData
{
    public string DisplayName;
    public string Slug;
    public uint Count;
    public string Namespace;

    public Sprite GetIcon()
    {
        return InventoryHelper.GetItemIcon(Namespace, Slug);
    }

    public GetItemData(Achievement.ResourceDetail resourceDetail)
    {
        DisplayName = resourceDetail.DisplayName;
        Slug = resourceDetail.SlugName;
        Count = resourceDetail.Quantity;
        Namespace = resourceDetail.Namespace;
    }
    public GetItemData()
    {
        
    }
}

public class GetItemParams
{
    public string Title = "获得物品";
    public List<GetItemData> GetItemDataList = new();

}