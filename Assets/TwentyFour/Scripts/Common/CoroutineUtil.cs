namespace TwentyFour.Scripts.Common
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;


    public class CoroutineUtil
    {
        private static CoroutineUtil _instance;

        public static CoroutineUtil Instance => _instance ??= new CoroutineUtil();

        private CoroutineUtilsProxy _proxy;

        private CoroutineUtilsProxy GetProxy()
        {
            if (_proxy == null)
            {
                var go = new GameObject("CoroutineUtilsProxy");
                GameObject.DontDestroyOnLoad(go);
                _proxy = go.AddComponent<CoroutineUtilsProxy>();
            }

            return _proxy;
        }

        private Dictionary<string, Coroutine> _managedRunningCoroutines;

        private Dictionary<string, Coroutine> ManagedRunningCoroutines =>
            _managedRunningCoroutines ??= new Dictionary<string, Coroutine>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routine"></param>
        /// <param name="key">pass the key that you want to stop the coroutine with</param>
        /// <returns></returns>
        public Coroutine StartCoroutine(IEnumerator routine, string key = null)
        {
            Coroutine c = GetProxy().StartCoroutine(routine);
            if (!string.IsNullOrEmpty(key))
                ManagedRunningCoroutines[key] = c;
            return c;
        }

        public void StopCoroutine(string key)
        {
            if (!ManagedRunningCoroutines.ContainsKey(key))
            {
                //unmanaged key
                return;
            }

            if (ManagedRunningCoroutines[key] == null)
            {
                ManagedRunningCoroutines.Remove(key);
                return;
            }

            GetProxy().StopCoroutine(ManagedRunningCoroutines[key]);
        }

        public void StopAllCoroutines()
        {
            if (_proxy != null)
            {
                _proxy.StopAllCoroutines();
            }

            _managedRunningCoroutines?.Clear();
        }
    }

    public class CoroutineUtilsProxy : MonoBehaviour
    {
    }
}