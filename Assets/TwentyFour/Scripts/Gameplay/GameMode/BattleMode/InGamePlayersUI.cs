using System.Collections;
using System.Collections.Generic;
using Unity.UOS.TwentyFour.Scripts.Component;
using UnityEngine;
using Unity.Muninn.Model;
using Unity.UOS.TwentyFour.UOSGateway;

namespace Unity.UOS.TwentyFour.Scripts.Battle.UI
{
    public class InGamePlayersUI : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerListTransform;

        private static bool _initialized;

        public static InGamePlayersUI Singleton => _singleton;
        private static InGamePlayersUI _singleton;

        private void Start()
        {
            _initialized = true;
            _singleton = this;
            SetPlayers(MuninnManager.GetRoom().Players);
        }

        public static void SetPlayers(List<MuninnPlayer> players)
        {
            if (!_initialized) return;
            
            DestroyAll(Singleton.playerListTransform);
            foreach (var player in players)
            {
                var obj = Instantiate(Singleton.playerPrefab, Singleton.playerListTransform);
                var controller = obj.GetComponent<InGameAvatarUI>();
                controller.Init(player);
            }
        }

        private static void DestroyAll(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i -= 1)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
    }
}