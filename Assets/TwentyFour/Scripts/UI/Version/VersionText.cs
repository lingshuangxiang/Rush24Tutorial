using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VersionText : MonoBehaviour
{
    Text versionCode;
    // Start is called before the first frame update
    void Start()
    {
        versionCode = GetComponent<Text>();
        var version = Application.version;
        var env = UosAppConfigs.GetUosAppConfigs().CurrentEnv;
        if(env != "Prod")
            env = $"{env.ToLower()}-";
        else
            env = string.Empty;
        
        versionCode.text = $"{env}{version}";
    }

}
