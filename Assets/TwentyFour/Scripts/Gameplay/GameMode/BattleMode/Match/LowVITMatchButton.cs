using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LowVITMatchButton : MonoBehaviour
{
    public Button Button;
    public RectTransform Label;

    private void OnEnable()
    {
        Button.onClick.RemoveAllListeners();
        Button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        Label.DoCommonShakeRotationZ();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
