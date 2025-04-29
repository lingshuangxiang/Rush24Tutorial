using System;
using UnityEngine;

namespace Unity.UOS.TwentyFour.Common
{
    /// <summary>
    /// 单例模式
    /// </summary>
    public class Singleton<T> where T : class, new()
    {
        private static T _instance;

        public static T Reset()
        {
            _instance = new T();
            return _instance;
        }

        public static T Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new T();
                return _instance;
            }
        }
    }

    /// <summary>
    /// MonoBehaviour的单例模式
    /// </summary>
    public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static bool IsQuitting = false;
        public static T Reset()
        {
            instance = (T)FindObjectOfType(typeof(T));
            return instance;
        }

        public static T Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = (T)FindObjectOfType(typeof(T));
                if(instance == null && !IsQuitting)
                    instance = new GameObject(typeof(T).Name).AddComponent<T>();
                return instance;
            }
        }
        
        public virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void OnApplicationQuit()
        {
            IsQuitting = true;
        }
    }
}