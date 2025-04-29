using UnityEngine;
using UnityEngine.UI;
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#endif

public class KeyboardManager : MonoBehaviour {
    public InputField inputField;
    private bool isKeyboardActive;

    public void ShowKeyboard() {
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        WX.ShowKeyboard(new ShowKeyboardOption {
            defaultValue = inputField.text,
            maxLength = 20,
            confirmType = "done"
        });
        WX.OnKeyboardInput(OnInput);
        WX.OnKeyboardConfirm(OnConfirm);
        isKeyboardActive = true;
#endif
    }
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
    private void OnInput(OnKeyboardInputListenerResult result) {
        inputField.text = result.value;
    }

    private void OnConfirm(OnKeyboardInputListenerResult result) {
        HideKeyboard();
    }
#endif
    public void HideKeyboard() {
#if UNITY_WEIXINMINIGAME && !UNITY_EDITOR
        WX.HideKeyboard((new HideKeyboardOption()));
        WX.OffKeyboardInput(OnInput);
        WX.OffKeyboardConfirm(OnConfirm);
        isKeyboardActive = false;
#endif
    }
}