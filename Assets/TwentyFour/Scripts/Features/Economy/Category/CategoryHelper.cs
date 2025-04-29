using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Economy;
using Unity.Passport.Runtime;
using UnityEngine;
using Utils;

namespace TwentyFour.Scripts.Category
{
    public static class CategoryHelper
    {
        public static Dictionary<string,Economy.Category> LocalCategories = new Dictionary<string, Economy.Category>();

        public  static Dictionary<string,SearchProductsSDKResponse> LocalProducts = new Dictionary<string,SearchProductsSDKResponse>();
        public const string DefaultGOLDCategory = "GOLD_COIN_Store";
        public const string DefaultVITCategory = "VIT_STORE";
        
        public static bool DefaultCategoryUpdated = false;
        public static string DefaultCategoryUpdatedTime;
        public static List<string> IgnoredCategories = new List<string>()
        {
            DefaultVITCategory,
        };
        public static void Init()
        {
            //LocalProducts?.Clear();
        }
        public static async Task<ListCategoriesResponse> ListCategories(string displayName = null, uint start = 0,
            uint count = 10)
        {
            var categories = await PassportFeatureSDK.Economy.ListCategories(displayName, start, count);
            LocalCategories = categories.Categories.ToDictionary(x => x.SlugName, x => x);
            return categories;
        }

        public static bool CheckDefaultCategoryUpdated()
        {
            if (LocalCategories.TryGetValue(DefaultGOLDCategory, out var category))
            {
                var local_updated_time = LocalStorageUtil.GetString(LocalStorageKeys.DEFAULT_CATEGORY_UPDATED);
                DefaultCategoryUpdatedTime = category.ModifiedAt.ToString();
                DefaultCategoryUpdated = local_updated_time != DefaultCategoryUpdatedTime;
                
            }
            return DefaultCategoryUpdated;
        }

        public static async Task<SearchProductsSDKResponse> ListProducts(string categorySlug, uint start = 0,
            uint count = 10)
        {
            var products = await PassportFeatureSDK.Economy.ListProducts(categorySlug, start, count);
            // foreach (var p in products.Products)
            // {
            //     Debug.LogError(p.DisplayName);
            // }
            LocalProducts[categorySlug] = products;
            return products;
        }
    }
}