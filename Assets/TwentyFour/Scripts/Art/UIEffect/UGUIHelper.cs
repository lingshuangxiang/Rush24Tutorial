using UnityEngine.UI;

namespace TwentyFour.Scripts.UIEffect
{
    public static class UGUIHelper
    {
        public static void InvokeOnClick(this Button button)
        {
            button.onClick?.Invoke();
        }
    }
}