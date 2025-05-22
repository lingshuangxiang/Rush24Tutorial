using UnityEngine;
using UnityEngine.EventSystems;

namespace TwentyFour.Scripts.Utilities
{
    public class PersistentEventSystem : MonoBehaviour
    {
        private static PersistentEventSystem _instance;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 确保场景中只有一个EventSystem
            var eventSystems = FindObjectsOfType<EventSystem>();
            if (eventSystems.Length > 1)
            {
                for (int i = 1; i < eventSystems.Length; i++)
                {
                    Destroy(eventSystems[i].gameObject);
                }
            }
        }
    }
}