using System;
using System.Collections;
using System.Collections.Generic;
using TwentyFour.Scripts.Metrics;
using TwentyFour.Scripts.Quest;
using TwentyFour.Scripts.RemoteConfig;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.UI;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.UI;

public class RedeemCodePanel : MonoBehaviour
{
    public Button PasteButton;
    public InputField InputCodeField;
    // Start is called before the first frame update
    void Start()
    {
        PasteButton.onClick.AddListener(OnPaste);
    }

    void OnPaste()
    {
        CopyPasteUtil.Paste(InputCodeField);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void Redeem()
    {
        if (string.IsNullOrEmpty(InputCodeField.text))
        {
            UIMessage.Show("兑换码为空！");
        }
        else
        {
            StartCoroutine(RedeemItem());
        }
    }
    private bool canPlayGetItemEffect;

    IEnumerator RedeemItem()
    {
        canPlayGetItemEffect = true;

        UIManager.Instance.HideCommonLoading();
        
        yield break;
    }
    
    void RedeemFailed(Exception e)
    {
        PassportException passportException = e as PassportException;
        UIMessage.Show(passportException?.ErrorMessage);
        canPlayGetItemEffect = false;
    }
}
