using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Passport.Runtime.UI;

[Serializable]
public class MyCustomConvert
{
    public string OriginText;
    public string TargetText;   
}

public class SimpleMatchTextConvert : MonoBehaviour
{
    public Text TargetText;
    public List<MyCustomConvert> converts = new List<MyCustomConvert>();

    public void SetConvertedText(string text)
    {
        
        var tar = converts.Find(x => x.OriginText == text);
        if (text == "error")
        {
            var content = tar == null ? text : tar.TargetText;
            // UIMessage.Show(content, MessageType.Error);
        }

        
    }

    public void ClearConvertedText()
    {
        TargetText.text = string.Empty;
        TargetText.color = Color.white;

    }
    
}
