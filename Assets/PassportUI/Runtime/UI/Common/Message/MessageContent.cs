using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Passport.Runtime.UI
{
    public class MessageContent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private float _deltaTime;
        private float _duration = 100;

        Color errorColor = new Color(0.5f, 0.055f, 0.055f, 0.85f);
        private Color defaultColor;
        private void Awake()
        {
            defaultColor = GetComponent<Image>().color;
        }


        public void Show(string message, MessageType type, float duration)
        {
            GetComponent<Image>().color = type == MessageType.Error ? errorColor : defaultColor;
            text.text = message;
            _deltaTime = 0;
            _duration = duration;
        }
    }
}
