using System.Collections;
using System.Collections.Generic;
using Unity.Muninn.Message;
using Unity.UOS.TwentyFour.Model.Sync;
using Unity.UOS.TwentyFour.Robot;
using Unity.UOS.TwentyFour.Scripts.Component;
using Unity.UOS.TwentyFour.UOSGateway;
using UnityEngine;
using MuninnPlayer = Unity.Muninn.Model.MuninnPlayer;

public class BattlePlayer : MonoBehaviour
{
    [SerializeField] public GameObject WorkingIcon;
    [SerializeField] public GameObject DoneIcon;
    [SerializeField] public GameObject EffectImage;
    [SerializeField] public Transform ActionTransform;
    [SerializeField] public PlayerCharatorManager PlayerCharator;

    private GameObject correctIconIns, workingIconIns;
    
    // Game Data
    public TeamTag Team;
    public string PersonaID;
    public string PlayerName;
    public int Score = 0;
    
    private string defaultPlayerName = "神秘人";
    
    // Start is called before the first frame update

    public void InitPlayerInfo()
    {
        var controller = GetComponent<InGameAvatarUI>();
        var mPlayer = MuninnManager.GetRoom()?.Players.Find(player => player.Id.Equals(PersonaID));
        if (mPlayer == null && MuninnManager.IsRobotRoom())
        {
            mPlayer = RobotHelper.Player;
        }
        PlayerName = string.IsNullOrEmpty(mPlayer?.Name) ? "神秘人" : mPlayer.Name;
        controller.Init(mPlayer);
        PlayerCharator.InitPlayerAvatar(mPlayer.Properties);
    }
    
    public void AnswerCorrect()
    {
        //update score
        Score++;
        
        //play effect
        if (correctIconIns == null)
        {
            Destroy(workingIconIns);
            correctIconIns = Instantiate(DoneIcon, ActionTransform);
            EffectImage.SetActive(true);
            PlayerCharator.PlayCelebrateAnimation();
        }
    }
    
    public void StartWorking()
    {
        if (workingIconIns == null)
        {
            workingIconIns = Instantiate(WorkingIcon, ActionTransform);
            PlayerCharator.PlayWorkingAnimation();
        }
    }


    public void OnClick()
    {
        // AnswerCorrect();
        // StartWorking();
    }
}
