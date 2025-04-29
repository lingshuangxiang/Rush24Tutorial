using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Leaderboard;
using TMPro;
using Unity.Passport.Runtime;
using Unity.UOS.TwentyFour.Model.Sync;
using Unity.UOS.TwentyFour.Scripts.Component;
using Unity.UOS.TwentyFour.UOSGateway;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

public class TiersBadge : MonoBehaviour
{
    public List<TierIcon> AllIcons = new List<TierIcon>();
    public Image ShowRank;
    public Sprite FilledStar, EmptyStar;
    public Transform StarParent, StarBackground;
    public GameObject LevelLabel;
    public Text subLevelText;

    public GameObject TiersParent;
    private int topTierScore = TiersHelper.TopTierScore;

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
    /// <summary>
    /// setup方法，配置是否显示星星，leaderboard.score是多少
    /// </summary>
    public void SetupBadge(bool ifShowStar, int leaderboardScore, string tierName)
    {
        StarBackground?.gameObject.SetActive(ifShowStar);
        if (!String.IsNullOrEmpty(tierName) && tierName?.Length > 2)
        {
            subLevelText.text = tierName.Substring(2).Length.ToString();
        }
        
        if (ifShowStar && StarParent != null)
        {
            if (leaderboardScore >= topTierScore)
            {
                //reached top tier, use levelLabel to show star count
                StarBackground?.gameObject.SetActive(false);
                StarParent?.gameObject.SetActive(false);
                subLevelText.gameObject.SetActive(false);
                LevelLabel.SetActive(true);
                LevelLabel.GetComponent<Text>().text = (leaderboardScore - topTierScore).ToString();
            }
            else
            {
                StarBackground?.gameObject.SetActive(true);
                StarParent?.gameObject.SetActive(true);
                subLevelText.gameObject.SetActive(true);
                LevelLabel.SetActive(false);
                
                //resolve tier stars
                int starScore = leaderboardScore % ScoreDiffBetweenSubLevel;
                Logger.Log("Star Score" + starScore);
                for (int i = 0; i < 3; i++)
                {
                    var star = StarParent.GetChild(i).gameObject;
                    star.SetActive(true);
                    star.GetComponent<Image>().sprite =
                        starScore > i ? FilledStar : EmptyStar;
                }

                // StarParent.gameObject.SetActive(true);
                // foreach (Transform va in StarParent) {
                //     va.gameObject.SetActive(true);
                // }
            }
        }
        else
        {
            StarBackground?.gameObject.SetActive(false);
            StarParent?.gameObject.SetActive(false);
            subLevelText.gameObject.SetActive(false);
            LevelLabel.SetActive(false);
        }

        

        //Logger.Log("Setup Tier" + tierName);
        TierIcon myTierIcon = AllIcons.Find(x => !String.IsNullOrEmpty(tierName) && tierName.Contains(x.targetName));

        if (myTierIcon?.image != null)
        {
            ShowRank.sprite = myTierIcon.image;
        }
    }

    public IEnumerator SetupBadgeWithEffect(int oldScore, int newScore, string oldTierName, string newTierName ,Action onCompletedAction = null)
    {
        OnCompletedAction = onCompletedAction;
        Logger.LogInfo(
            $"Setup Badge With Effect, Old Score: {oldScore}, New Score: {newScore}, Old Tier: {oldTierName}, New Tier: {newTierName}");
        SetupBadge(true, oldScore, oldTierName);
        
        yield return new WaitForSeconds(1.5f);
        if (TiersParent == null)
        {
            SetupBadge(true, newScore, newTierName);
            yield break;
        }

        var oldTier = TiersHelper.GetTier(oldTierName);
        var newTier = TiersHelper.GetTier(newTierName);
        int finalScore = newScore % ScoreDiffBetweenSubLevel;
        int startScore = oldScore % ScoreDiffBetweenSubLevel;
        if (newTier > oldTier) //升段
        {
            TiersParent.transform.DOScale(0, 0.5f).onComplete += () =>
            {
                StarParticles?.Play();
                var socre = newScore >= topTierScore ? newScore : TiersHelper.GetTier(newTierName);
                SetupBadge(true, socre, newTierName);
                TiersParent.transform.localScale = Vector3.one;
                TiersParent.transform.DOPunchScale(punchAnimSize, 1.5f, 5).onComplete += () =>
                {
                    PlayAddStarAnim(0, finalScore,OnEffectEnd);
                };
            };
        }
        else if (newTier < oldTier) //降段
        {
            if (oldScore >= topTierScore)
            {
                var socre = oldScore >= topTierScore ? oldScore : TiersHelper.GetTier(oldTierName);
                SetupBadge(true, socre, oldTierName);
                TiersParent.transform.DOScale(0, 1f).onComplete += () =>
                {
                    SetupBadge(true, newScore, newTierName);
                    TiersParent.transform.DOScale(1, 1f).OnComplete(OnEffectEnd);
                };
            }
            else
            {
                
                PlayLowerStarAnim(startScore, 0, () =>
                {
                    var socre = oldScore >= topTierScore ? oldScore : TiersHelper.GetTier(oldTierName);
                    SetupBadge(true, socre, oldTierName);
                    TiersParent.transform.DOScale(0, 1f).onComplete += () =>
                    {
                        SetupBadge(true, newScore, newTierName);
                        TiersParent.transform.DOScale(1, 1f).OnComplete(OnEffectEnd);
                    };
                });
            }
        }
        else
        {
            if (newScore >= topTierScore)
            {
                if (newScore > oldScore) //升星
                {
                    SetupBadge(true, newScore, newTierName);
                    LevelLabel.transform.DOPunchScale(LabelPunchScale,LabelDuration).OnComplete(OnEffectEnd);
                }
                else if (newScore < oldScore) //降星
                {
                    var text = LevelLabel.GetComponent<Text>();
                    text.DOFade(0, 1f).From(1).OnComplete((() =>
                    {
                        SetupBadge(true, newScore, newTierName);
                        text.DOFade(1, 1).From(0).OnComplete(OnEffectEnd);
                    }));
                }
                else
                {
                    SetupBadge(true, newScore, newTierName);
                    OnEffectEnd();
                }
            }
            else
            {
                if (newScore > oldScore) //升星
                {
                    PlayAddStarAnim(startScore, finalScore,OnEffectEnd);
                }
                else if (newScore < oldScore) //降星
                {
                    PlayLowerStarAnim(startScore, finalScore, (() =>
                    {
                        OnEffectEnd();
                        SetupBadge(true, newScore, newTierName);
                    }));
                }
                else
                {
                    OnEffectEnd();
                    SetupBadge(true, newScore, newTierName);
                }
            }
            
        }
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