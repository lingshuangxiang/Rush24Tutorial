using System.Collections;
using System.Collections.Generic;
using TwentyFour.Scripts.PersonaProperty;
using UnityEngine;


public class KapibalaEventEntryButton : MonoBehaviour
{
    public PlayerCharatorManager CharatorManager;
    private string kapibaraKey = "CharKapibara";

    // Start is called before the first frame update
    void Start()
    {
        Dictionary<string,string> kapibaras = new Dictionary<string, string>();
        kapibaras[PersonaPropertyKeys.ActiveAvatarEyeKey] = kapibaraKey;
        kapibaras[PersonaPropertyKeys.ActiveAvatarMouthKey] = kapibaraKey;
        kapibaras[PersonaPropertyKeys.ActiveAvatarHeadKey] = kapibaraKey;
        CharatorManager.InitPlayerAvatar(kapibaras);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
