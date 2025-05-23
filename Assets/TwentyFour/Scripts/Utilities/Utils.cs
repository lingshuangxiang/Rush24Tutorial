using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#endif

namespace TwentyFour.Scripts.Utilities
{
    public class Utils
    {
        public static byte[] ConvertToByteArray(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        
        public static T ConvertToObject<T>(byte[] byteArray)
        {
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(ms);
            }
        }
        
        public static bool IsInteger(float fValue)
        {
            return Mathf.Approximately(Mathf.Round(fValue), fValue); // 是个整数
        }

        public static void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEIXINMINIGAME && !UNITY_EDITOR
            WX.ExitMiniProgram(new ExitMiniProgramOption());
#else
            Application.Quit();
#endif
        }
    }
}