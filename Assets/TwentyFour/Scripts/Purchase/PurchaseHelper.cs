using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Economy;
using Unity.Passport.Runtime;
using Unity.UOS.TwentyFour.Common;

namespace TwentyFour.Scripts.Purchase
{
    public static class PurchaseHelper
    {
        public static Action OnPurchase;
        public static async Task<Transaction> VirtualPurchase(string productSlug, uint quantity,
            Dictionary<string, string> customData = null,Action<Exception> failedAction = null)
        {
            try
            {
                var purchase = await PassportFeatureSDK.Economy.VirtualPurchase(productSlug, quantity, customData);
                OnPurchase?.Invoke();
                return purchase;
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                failedAction?.Invoke(e);
                throw;
            }
            
        }
    }
}