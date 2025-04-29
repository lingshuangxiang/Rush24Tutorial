using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Passport;
using TwentyFour.Scripts.PersonaProperty;
using Unity.Muninn.Model;
using Unity.Passport.Runtime;
using Unity.Passport.Runtime.Model;
using UnityEngine;
using UnityEngine.UI;
using Unity.UOS.TwentyFour.UOSGateway;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

/// <summary>
/// 本代码会根据提供的信息反馈指定玩家的名称，段位属性等内容,并且同时具备主机对玩家的操作空间
/// </summary>
public class PlayerPanelInfoUpdate : MonoBehaviour
{
    public Text PlayerName = null;
    public TiersBadge TierBadge;

    public Button KickButton;
    public GameObject IsMasterClient;
    public bool IsSelf;
    public PlayerCharatorManager PlayerCharator;

    private MuninnPlayer playerInfo;
    
    private void Awake()
    {
        if (IsSelf)
        {
            ShowSelfInfo();
        }
        else
        {
            MuninnManager.Singleton.OnMasterClientChangedAction += OnMasterClientChanged;
            KickButton?.onClick.AddListener(() =>
            {
                if (playerInfo != null)
                {
                    MuninnManager.Singleton.KickPlayer(playerInfo.SenderId);
                }
            });    
        }
    }

    private void OnDestroy()
    {
        KickButton?.onClick.RemoveAllListeners();
        if(MuninnManager.Singleton != null)
            MuninnManager.Singleton.OnMasterClientChangedAction -= OnMasterClientChanged;
    }

    private void ShowSelfInfo()
    {
        int score = 0;
        string tier = "石头";
        if (PersonaPropertiesHelper.MyLeaderboardScore != null)
        {
            tier = PersonaPropertiesHelper.MyLeaderboardScore.Tier;
            score = (int)PersonaPropertiesHelper.MyLeaderboardScore.Score;
        }
        PlayerName.text = PassportSDK.CurrentPersona.DisplayName;

        TierBadge.SetupBadge(true, score, tier);
        
        //load self avatar
        PlayerCharator.InitPlayerAvatar(PersonaPropertiesHelper.GetLocalProperties());
    }

    private void UpdateMasterClient()
    {
        //update tag
        if (playerInfo != null)
        {
            bool isMasterClientPanel = MuninnManager.Singleton.IsMasterClient(playerInfo);
            IsMasterClient?.SetActive(isMasterClientPanel);
            
            //update kick button
            KickButton?.gameObject.SetActive(MuninnManager.Singleton.IsMasterClient() && !isMasterClientPanel);
        }
    }

    public void ShowPlayerInfo(MuninnPlayer player)
    {
        playerInfo = player;
        if (player == null)
        {
            Logger.LogError("Player is null");
            return;
        }
        var room = MuninnManager.Singleton.GetMuninnRoomView();

        PlayerName.text = player.Name;

        var showTier = "石头";
        int score = 0;
        if (player.Properties.TryGetValue(PersonaPropertyKeys.BattleCurrentTierKey, out var tier))
            showTier = tier;
        if (player.Properties.TryGetValue(PersonaPropertyKeys.BattleCurrentScoreKey, out var scoreStr))
            score = int.Parse(scoreStr);
        TierBadge.SetupBadge(true, score, showTier);
        
        UpdateMasterClient();
        
        //load player avatar
        PlayerCharator.InitPlayerAvatar(player.Properties);
    }

    public void ShowPlayerInfo()
    {
        
    }

    private void OnMasterClientChanged(uint obj)
    {
        UpdateMasterClient();
    }
}
