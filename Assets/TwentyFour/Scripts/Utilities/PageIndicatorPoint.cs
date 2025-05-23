using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TwentyFour.Scripts.Utilities
{
    public class PageIndicatorPoint : MonoBehaviour
    {
        [SerializeField]
        public GameObject FocusGameObject;

        public void SetFocused(bool toFocused = true)
        {
            FocusGameObject.SetActive(toFocused);
        }
    }
}