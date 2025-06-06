using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

namespace TwentyFour.Scripts.Utilities
{
    public class ButtonTextAdaptor : MonoBehaviour, IDeselectHandler
    {
        // public static readonly Color COLOR_DEFAULT = new Color(190 / 255f, 190 / 255f, 188 / 255f);
        private static readonly Color COLOR_HIGHLIGHTED = new Color(34 / 255f, 64 / 255f, 99 / 255f);

        [SerializeField] public Color DefaultTextColor = COLOR_HIGHLIGHTED;
        [SerializeField] public Color SelectedTextColor = Color.white;
        public bool DeselectOnClick = true;
        public bool DeselectOnOutsideClick = true;
        
        // private bool isSelected;
        private Shadow btnShadow;
        private TextMeshProUGUI text;

        public bool isSelected;
        
        public GameObject Bg;
        public GameObject BGSelected;
        
        public UnityEvent OnSelected;
        public UnityEvent OnDeselected;


        // Start is called before the first frame update
        protected void Start()
        {
            text = transform.GetComponentInChildren<TextMeshProUGUI>();
            isSelected = false;
            if (text != null)
            {
                text.color = DefaultTextColor;
            }
        }

        public void OnSelect()
        {
            if (!isSelected)
            {
                isSelected = true;
                if (BGSelected != null)
                {
                    BGSelected?.SetActive(true);
                }

                if (Bg != null)
                {
                    Bg?.SetActive(false);

                }
                OnSelected?.Invoke();
            
                //change text color
                if (text != null)
                {
                    text.color = SelectedTextColor;            
                }
            }
            else if (DeselectOnClick)
            {
                EventSystem.current.SetSelectedGameObject(null);
                OnDeselect();
            }
        
        }

        public void OnDeselect()
        {
            isSelected = false;
            OnDeselected?.Invoke();
            if (BGSelected != null)
            {
                BGSelected?.SetActive(false);
            }

            if (Bg != null)
            {
                Bg?.SetActive(true);

            }
        
            if (text != null)
            {
                text.color = DefaultTextColor;            
            }

            if (btnShadow != null)
            {
                btnShadow.enabled = true;
            }
        }

        // public void Deselect()
        // {
        //     OnDeselect();
        // }
        //

        public void OnDeselect(BaseEventData eventData)
        {
            if (DeselectOnOutsideClick)
            {
                OnDeselect();
            }
        }
    }
}