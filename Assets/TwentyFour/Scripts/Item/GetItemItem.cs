using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetItemItem : MonoBehaviour
{
    public Image ItemIcon;
    public Text ItemName;
    public Text CountText;
    
    // Start is called before the first frame update
    public void Init(GetItemData data)
    {
        ItemName.text = data.DisplayName;
        ItemIcon.sprite = data.GetIcon();
        CountText.text = data.Count.ToString();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
