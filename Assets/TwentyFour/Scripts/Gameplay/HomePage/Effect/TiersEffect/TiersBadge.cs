using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.UOS.TwentyFour.UOSGateway;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Logger = TwentyFour.Scripts.Utilities.Logger;

public class TiersBadge : MonoBehaviour
{
    public List<TierIcon> AllIcons = new List<TierIcon>();
    public Image ShowRank;
    public Sprite FilledStar, EmptyStar;
    public Transform StarParent, StarBackground;
    public GameObject LevelLabel;
    public Text subLevelText;

    public GameObject TiersParent;

    private Vector3 punchAnimSize = new Vector3(0.2f, 0.2f, 0.2f);

    public const int ScoreDiffBetweenSubLevel = 4; // （子）段位之间的分数差距
    public const int SubLevelCount = 3; // 每个等级有几个子等级
    
    public ParticleSystem StarParticles;

    public Action OnCompletedAction;
    
    Vector3 originalStarScale;

    private void OnEnable()
    {
        originalStarScale = StarParent.GetChild(0).transform.localScale;
    }

    void OnEffectEnd()
    {
        OnCompletedAction?.Invoke();
    }
    
    public Vector3 LabelPunchScale = new Vector3(0.2f, 0.2f, 0.2f);
    public float LabelDuration = 1f;
    void PlayAddStarAnim(int start, int end, Action callback = null)
    {
        if (StarParent == null) return;
        var childrenCount = StarParent.childCount;
        if (start >= childrenCount || end < 0 || start < 0)
        {
            callback?.Invoke();
            return;
        }

        if (start < end)
        {
            var targetStar = StarParent.GetChild(start);
            BGMManager.Instance.PlayAFX(AFXMusic.BattleStarUp);
            targetStar.DOPunchScale(punchAnimSize, 1f, 5).onComplete += () => { PlayAddStarAnim(++start, end, callback); };
            targetStar.gameObject.GetComponent<Image>().sprite = FilledStar;
        }
        else
        {
            callback?.Invoke();
        }
    }

    void PlayLowerStarAnim(int start, int end, Action callback = null)
    {
        if (StarParent == null) return;
        var childrenCount = StarParent.childCount;
        if (start > childrenCount || end < 0 || start < 0)
        {
            callback?.Invoke();
            return;
        }

        if (start > end)
        {
            var targetStar = StarParent.GetChild(start - 1);
            var starImage = targetStar.GetComponent<Image>();
            var color = starImage.color;
            targetStar.DOScale(1.5f, 1).From(originalStarScale.x).SetEase(Ease.Linear);
            var tweener = DOTween.ToAlpha(() => color, (x) => color = x, 0f, 1);
            tweener.onUpdate += () => { starImage.color = color; };
            tweener.onComplete += () =>
            {
                starImage.sprite = EmptyStar;
                color.a = 1;
                starImage.color = color;
                targetStar.localScale = originalStarScale;
                PlayLowerStarAnim(--start, end, callback);
            };
        }
        else
        {
            callback?.Invoke();
        }
    }
    
}

/// <summary>
/// 徽章数据
/// </summary>
[System.Serializable]
public class TierIcon
{
    public string targetName;
    public Sprite image;
}