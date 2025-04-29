using TMPro;
using Unity.UOS.TwentyFour.Model.Sync;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class QuestionIndexButton : MonoBehaviour
{
    public static readonly Color COLOR_CORRECT = new Color(76 / 255f, 175 / 255f, 80 / 255f);
    public static readonly Color BLUE_COLOR = new Color(96 / 255f, 143 / 255f, 255 / 255f);
    public static readonly Color RED_COLOR = new Color(252 / 255f, 78 / 255f, 92 / 255f);
    
    public int index;
    public Image BackgroundImage;
    [SerializeField] public GameObject CorrectIcon, OthersIcon;
    // public Color RedTeamColor = RED_COLOR;
    // public Color BlueTeamColor = BLUE_COLOR;
    public Color DefaultColor;
    public Color SelectedColor;
    public ButtonTextAdaptor TextAdaptor;
    
    // [SerializeField] public Button selectableButton;
    
    public UnityEvent<int> OnSelect;
    public UnityEvent OnDeselect;
    
    bool isSelected, isCorrect;
    
    
    // Start is called before the first frame update
    void Start()
    {
        //use last char of game object name as index
        index = int.Parse(gameObject.name.Substring(gameObject.name.Length - 1, 1));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        OnSelect?.Invoke(index);
    }

    public void Select()
    {
        //update background color;
        isSelected = true;
        RefreshBackgroundColor();
        
        //update text color;
        TextAdaptor?.OnSelect();
    }

    public void Deselect()
    {
        //update background color;
        isSelected = false;
        RefreshBackgroundColor();
        
        //update text color;
        TextAdaptor?.OnDeselect();
        
        OnDeselect?.Invoke();
    }

    private void RefreshBackgroundColor()
    {
        BackgroundImage.color = isSelected ? SelectedColor : DefaultColor;
    }

    public void SetCorrect(string teamsName = "")
    {
        isCorrect = true;
        
        if (teamsName == "")
            Logger.LogError("并未找到自己的队伍！");

        // var colors = selectableButton.colors;
        if (TeamTag.BLUE.ToString().Equals(teamsName))
        {
            DefaultColor = BattleEffectManager.instance.BlueTeamColor;
            SelectedColor = BattleEffectManager.instance.BlueTeamColor;
        }
        else if (TeamTag.RED.ToString().Equals(teamsName))
        {
            DefaultColor = BattleEffectManager.instance.RedTeamColor;
            SelectedColor = BattleEffectManager.instance.RedTeamColor;
        }
        else
        {
            DefaultColor = COLOR_CORRECT;
            SelectedColor = COLOR_CORRECT;
            //    分为蓝队和红队显示了
        }

        if (teamsName == MuninnManager.MyTeamTag.ToString())
        {
            CorrectIcon.SetActive(true);
        }
        else
        {
            OthersIcon.SetActive(true);
        }

        GetComponentInChildren<TextMeshProUGUI>()?.gameObject.SetActive(false);

        RefreshBackgroundColor();
    }
}
