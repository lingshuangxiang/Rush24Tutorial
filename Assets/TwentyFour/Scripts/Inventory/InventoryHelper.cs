using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Economy;
using Google.Protobuf.Collections;
using Token;
using TwentyFour.Scripts.PersonaProperty;
using TwentyFour.Scripts.Tournament;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.Model;
using Unity.VisualScripting;
using UnityEngine;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public static class InventoryHelper
{
    public static List<CharPartsInventoryItem> CharAvatarPartsList = new List<CharPartsInventoryItem>();
    public const string AvatarPartsKey = "avatar";
    const string TournamentTicketKey = "TournamentTicket";
    
    private const string DefaultHeadName = "神秘脑袋";
    private const string DefaultEyeName = "神秘眼睛";
    private const string DefaultMouthName = "空空如也的嘴巴";
    private const string DefaultHeadWearName = "空空如也的头饰";
    
    private const string DefaultHeadDesc = "默认头像";
    private const string DefaultEyeDesc = "默认眼睛";
    private const string DefaultMouthDesc = "空空如也的嘴巴";
    private const string DefaultHeadWearDesc = "空空如也的头饰";
    public static Action OnRedeemToken;
    public static Action OnInventoryUpdated;
    public static List<ExpandedInventoryItemSDK> ExpandedInventoryItems = new List<ExpandedInventoryItemSDK>();
    public static Dictionary<string,ExpandedInventoryItemSDK> ExpandedInventoryItemDic = new Dictionary<string, ExpandedInventoryItemSDK>();
    
    public static async Task<ListPersonaInventoryResponse> ListPersonaInventory(Action<Exception> onFailed = null)
    {
        try
        {
            ListPersonaInventoryResponse personaInventories = await PassportFeatureSDK.Economy.ListPersonaInventory();
            CharAvatarPartsList?.Clear();
            ExpandedInventoryItems?.Clear();
            ExpandedInventoryItems = personaInventories.Inventory;
            ExpandedInventoryItemDic = ExpandedInventoryItems.ToDictionary(x => x.Resource.ResourceSlug, x => x);

            foreach (var item in personaInventories.Inventory.Where(item => item.Namespace.Contains(AvatarPartsKey)))
            {
                CharAvatarPartsList?.Add(new CharPartsInventoryItem()
                {
                    Item = item,
                    CustomData = item.Resource.CustomData.ToDictionary(x => x.Key, x => x.Value),
                });
            }

            TournamentDataHelper.TournamentDatas?.Clear();
            foreach (var item in personaInventories.Inventory.Where(item =>
                         item.Resource.Namespace == TournamentTicketKey))
            {
                TournamentDataHelper.Add(item.Resource.CustomData["data"]);
            }

            if (TournamentDataHelper.TournamentDatas?.Count > 0)
            {
                for (int i = 0; i < TournamentDataHelper.TournamentDatas.Count; i++)
                {
                    var data = TournamentDataHelper.TournamentDatas[i];
                    if (!data.IsExpired())
                    {
                        TournamentData.Current = data;
                        break;
                    }
                }
            }

            CharAvatarPartsList?.Add(GenCharPartsInventoryItem(PersonaPropertyKeys.ActiveAvatarHeadKey, "default_head",
                DefaultHeadName, new Dictionary<string, string>()
                {
                    { "key", "CharBlackBall" },
                    { "desc", DefaultHeadDesc },
                }));
            CharAvatarPartsList?.Add(GenCharPartsInventoryItem(PersonaPropertyKeys.ActiveAvatarEyeKey, "default_eye",
                DefaultEyeName, new Dictionary<string, string>()
                {
                    { "key", "CharBlackBall" },
                    { "desc", DefaultEyeDesc },
                }));
            CharAvatarPartsList?.Add(GenCharPartsInventoryItem(PersonaPropertyKeys.ActiveAvatarMouthKey, "empty",
                DefaultMouthName, new Dictionary<string, string>()
                {
                    { "key", "CharBlackBall" },
                    { "desc", DefaultMouthDesc },
                }));
            CharAvatarPartsList?.Add(GenCharPartsInventoryItem(PersonaPropertyKeys.ActiveAvatarHeadwearKey,
                "empty",
                DefaultHeadWearName, new Dictionary<string, string>()
                {
                    { "key", "CharBlackBall" },
                    { "desc", DefaultHeadWearDesc },
                }));

            OnInventoryUpdated?.Invoke();
            return personaInventories;
        }
        catch (Exception e)
        {
            onFailed?.Invoke(e);
            Logger.LogError(e);
            throw;
        }
    }

    static CharPartsInventoryItem GenCharPartsInventoryItem(string nameSpace,string slug,string displayName,Dictionary<string, string> customData)
    {
        var info = new ExpandedInventoryItemSDK()
        {
            Namespace = nameSpace,
            Quantity = 1,
            Resource = new ResourceInfo()
            {
                ResourceSlug = slug,
                DisplayName = displayName,
                Namespace = nameSpace,
            }
        };
        var item = new CharPartsInventoryItem()
        {
            Item = info,
            CustomData = customData
        };
        return item;
    }

    public static bool IsSufficientQuantity(string slug, uint quantity)
    {
        foreach (var inventory in ExpandedInventoryItems)
        {
            if (inventory.Resource.ResourceSlug == slug)
            {
                return inventory.Quantity >= quantity;
            }
        }
        return false;
    }
    public static string GetKey(this CharPartsInventoryItem target)
    {
        return target.CustomData["key"];
    }

    public static string GetDesc(this CharPartsInventoryItem target)
    {
        return target.CustomData["desc"];
    }

    public static Sprite GetItemIcon(string NameSpace, string Slug)
    {
        var sprite = Resources.Load<Sprite>($"Icons/{NameSpace}/{Slug}");
        if (sprite == null)
        {
            sprite = Resources.Load<Sprite>($"Icons/{NameSpace}/Default");
        }

        return sprite;
    }
    public static Sprite GetItemSmallIcon(string NameSpace, string Slug)
    {
        var sprite = Resources.Load<Sprite>($"Icons/{NameSpace}/Small/{Slug}");
        if (sprite == null)
        {
            sprite = Resources.Load<Sprite>($"Icons/{NameSpace}/Default");
        }

        return sprite;
    }
    
    public static async Task<TokenInstanceResponse> RedeemToken(string code, string scope = "", Action<Exception> failedAction = null)
    {
        try
        {
            var token = await PassportFeatureSDK.Token.RedeemToken(code, scope);
            OnRedeemToken?.Invoke();
            return token;
        }
        catch (Exception e)
        {
            failedAction?.Invoke(e);
            Logger.LogError($"{e.Message} {e.Data}");
            throw;
        }
        
    }
}

public class CharPartsInventoryItem
{
    public ExpandedInventoryItemSDK Item;
    public Dictionary<string,string> CustomData;
    
    
}
