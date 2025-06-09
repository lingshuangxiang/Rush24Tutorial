using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace TwentyFour.Scripts.Features.Player
{
    public class Identity
    {
        private static string _defaultRealmID = "";
        

        [MenuItem("Tools/Clear")]
        public static void Clear()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}