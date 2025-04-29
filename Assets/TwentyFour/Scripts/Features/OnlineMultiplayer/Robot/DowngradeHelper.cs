using TwentyFour.Scripts.RemoteConfig;
using UnityEngine;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

namespace Unity.UOS.TwentyFour.Robot
{
    public class DowngradeConfig
    {
        public int ContinuousFailCount = 5;
        public int ContinuousCompletelyFailCount = 2;
    }
    
    public class DowngradeHelper
    {
        private static int ContinuousFailCount = 0; // 连续失败次数
        private static int ContinuousCompletelyFailCount = 0; // 连续完败次数
        private static DowngradeConfig downgradeConfig = null;

        /// <summary>
        /// 增加失败次数
        /// </summary>
        private static void AddFail()
        {
            ContinuousFailCount += 1;
        }

        /// <summary>
        /// 增加完败次数
        /// </summary>
        private static void AddCompletelyFail()
        {
            ContinuousCompletelyFailCount += 1;
        }

        /// <summary>
        /// 清除失败次数
        /// </summary>
        private static void ClearFail()
        {
            ContinuousFailCount = 0;
        }

        /// <summary>
        /// 清除完败次数
        /// </summary>
        private static void ClearCompletelyFail()
        {
            ContinuousCompletelyFailCount = 0;
        }

        /// <summary>
        /// 是否需要降级
        /// </summary>
        /// <returns></returns>
        public static bool NeedDowngrade()
        {
            downgradeConfig ??= RemoteConfigHelper.GetJson<DowngradeConfig>(RemoteConfigKeys.DowngradeConfig);
            var needDowngrade = ContinuousFailCount >= downgradeConfig.ContinuousFailCount ||
                                ContinuousCompletelyFailCount >= downgradeConfig.ContinuousCompletelyFailCount;
            Logger.Log("【降级】配置 " + "连续失败：" + downgradeConfig.ContinuousFailCount + "连续完败" + downgradeConfig.ContinuousCompletelyFailCount);
            Logger.Log($"【降级】是否需要降级 {needDowngrade}");
            return needDowngrade;
        }
        

        /// <summary>
        /// 设置降级信息
        /// </summary>
        /// <returns></returns>
        public static void SetDowngradeInfo()
        {
            var fail = !BattleEffectManager.instance.IfMyTeamWin();
            var completelyFail = BattleEffectManager.instance.IfOtherTeamCompletelyWin();
            
            if (fail)
            {
                AddFail();
            }
            else
            {
                ClearFail();
            }

            if (completelyFail)
            {
                AddCompletelyFail();
            }
            else
            {
                ClearCompletelyFail();
            }
            
            Logger.Log($"【降级】本局： fail: {fail}  completelyFail: {completelyFail}");
        }
        
    }
}