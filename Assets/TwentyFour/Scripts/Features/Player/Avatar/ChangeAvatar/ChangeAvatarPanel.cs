using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ChangeAvatarPanel : MonoBehaviour
{
    public PlayerCharatorManager CharatorManager;
    public RectTransform Light;
    public RectTransform DrapeL;
    public RectTransform DrapeR;
    public RectTransform Drapes;
    public ParticleSystem StartEffect;
    public Button ExitButton;
    public Action OnExit;
    public void Init()
    {
        CharatorManager.InitPlayerAvatar(PersonaPropertiesHelper.GetLocalProperties());
    }

    public void PlayEffect()
    {
        StopAllCoroutines();
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        ExitButton.gameObject.SetActive(false);
        yield return new WaitForSeconds(1);
        DrapeL.DOScaleX(1, 0.5f).From(0.2f);
        DrapeR.DOScaleX(1, 0.5f).From(0.2f);
        yield return new WaitForSeconds(0.5f);
        Drapes.DoCommonShakeRotationZ(strength:5);
        yield return new WaitForSeconds(1f);
        Light.gameObject.SetActive(true);
        StartEffect.Play();
        Light.DOScale(1, 0.5f).From(0);
        Light.DOLocalRotate(new Vector3(0, 0, 360), 10, RotateMode.FastBeyond360).From(Vector3.zero).SetEase(Ease.Linear).SetLoops(-1);
        CharatorManager.InitPlayerAvatar(PersonaPropertiesHelper.GetLocalProperties());
        DrapeL.DOScaleX(0.2f, 0.3f).From(1);
        DrapeR.DOScaleX(0.2f, 0.3f).From(1);
        yield return new WaitForSeconds(1f);
        ExitButton.gameObject.SetActive(true);
        ExitButton.onClick.RemoveAllListeners();
        ExitButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            OnExit?.Invoke();
            OnExit = null;
        });
        ExitButton.GetComponentInChildren<Text>().DOFade(1, 1f).From(0);

        
    }
    private void OnEnable()
    {
        PlayEffect();
    }

    private void OnDisable()
    {
        CharatorManager.InitPlayerAvatar(PersonaPropertiesHelper.GetLocalProperties());

    }

    // Start is called before the first frame update
    void Start()
    {
        //Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
