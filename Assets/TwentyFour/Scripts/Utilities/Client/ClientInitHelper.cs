using Unity.UOS.TwentyFour;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;

namespace TwentyFour.Scripts.Utilities
{
    public static class ClientInitHelper
    {
        public static void Init()
        {
            Application.targetFrameRate = 60;
            UIManager.Instance.Init();
        }

        public static void Logout()
        {
            //清除旧帐号数据
            UOSSave.Dispose();

            //退出登录
            GameRouter.BackAndLogout();
        }

    }
}
