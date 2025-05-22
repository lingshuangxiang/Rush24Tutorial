using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = TwentyFour.Scripts.Utilities.Logger;

public class AnimEventTest : MonoBehaviour
{
    public ParticleSystem ParticleconWin;
    public ParticleSystem ParticleconLos;
    public GameObject RotateEffect;
    public GameObject DestoryObj;
    public Action OnDestroyAction;
    [Header("变化时间区间")]
    public float AllTime;
    public void Awake()
    {
        //RotateEffect = GameObject.Find("Effect_Rotate_1");
    }
    //动画窗口使用的脚本
    public void PlayWinParticle()
    {
        ParticleconWin.Play();
    }
    public void PlayLostParticle()
    {
        ParticleconLos.Play();
    }
    public void DestroyItem()
    {
        //RotateEffect = GameObject.Find("Effect_Rotate_1");
        if (RotateEffect != null)
        {
            Destroy(RotateEffect);
        }
        else
        {
            Logger.Log("NOT FIND!");
        }
    }
    private SpriteRenderer sprite;
    private bool isFade = false;
    
    /// <summary>
    /// sprite color change progressing(why I can't use Chinese)
    /// </summary>
    public void AlphaAppear()
    {
        sprite = RotateEffect?.GetComponent<SpriteRenderer>();
       
        if(sprite == null) return;
        StopAllCoroutines();
        //StartCoroutine(Appear(isFade));
        isFade = true;
        Logger.Log(isFade);

    }

    IEnumerator Appear(bool isFade)
    {
        float timer = 0;
        while (isFade || sprite.color.a >= 0 && sprite.color.a <= 0.4f)
        {
            timer += Time.deltaTime;
            float progress = timer / AllTime;//sprite.
            if (isFade)
            {
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, sprite.color.a *( 1- progress));
                //Debug.Log($"the progress is {1 - progress}!");
            }
            else
            {
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, sprite.color.a * progress);
                
            }
            yield return new WaitForFixedUpdate();
        }
        

    }
    public void DestoryItem()
    {
        OnDestroyAction?.Invoke();
        //Destroy(DestoryObj);

    }

}

