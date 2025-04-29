using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.UOS.TwentyFour.UOSGateway;
using Unity.UOS.TwentyFour.Scripts.Battle.Model;

namespace TwentyFour.Scripts.Tests
{
    public class TestReconnect : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            DontDestroyOnLoad(this);
        }


        // just for test
        public void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.L))
            {
                // 离开房间
                MuninnManager.Singleton.LeaveRoom();
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                RoomManager.JoinRoom(BattleMode.OneOnOneCustom, MuninnManager.Singleton.RoomId);
            }
#endif

        }
    }
}