using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TwentyFour.Scripts.Art.Effects;

namespace TwentyFour.Scripts.Art.UIEffect
{
    public class UIEffect : MonoBehaviour
    {
        [SerializeField] public List<UIEffectData> UIEffectDataList = new List<UIEffectData>();

        public UIEffectPlayTimingType PlayTimingType;
        public float CustomDelaySecond = 0f;
        public UnityEvent OnPlay;
        public UnityEvent OnEnd = new UnityEvent();

        void Awake()
        {
            if (!isActiveAndEnabled)
                return;
            foreach (UIEffectData uiEffectData in UIEffectDataList)
            {
                uiEffectData.Init();
            }

            if (PlayTimingType == UIEffectPlayTimingType.AfterCutSceneAnim)
            {
                AsyncLoadingSceneEffect.OnUnloadLoadingCompletedAction -= Play;

                AsyncLoadingSceneEffect.OnUnloadLoadingCompletedAction += Play;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            if (PlayTimingType == UIEffectPlayTimingType.Start)
                Play();
        }

        private void OnEnable()
        {
            if (PlayTimingType == UIEffectPlayTimingType.OnEnable)
                Play();
        }

        private void OnDisable()
        {

        }

        void ResetAll()
        {
            if (PlayTimingType == UIEffectPlayTimingType.OnEnable)
            {
                StopCoroutine(PlayEffect());
                foreach (UIEffectData uiEffectData in UIEffectDataList)
                {
                    uiEffectData.Reset();
                }
            }
        }

        private void OnDestroy()
        {
            AsyncLoadingSceneEffect.OnUnloadLoadingCompletedAction -= Play;
            StopAllCoroutines();
        }

        public void Play()
        {
            OnPlay?.Invoke();
            AsyncLoadingSceneEffect.OnUnloadLoadingCompletedAction -= Play;
            StopCoroutine(PlayEffect());
            StartCoroutine(PlayEffect());
        }

        IEnumerator PlayEffect()
        {
            ResetAll();

            yield return new WaitForSeconds(CustomDelaySecond);

            foreach (var effectData in UIEffectDataList)
            {
                effectData.Play();
                yield return new WaitForSeconds(effectData.Delay);
            }

            OnEnd?.Invoke();
        }
    }

    public enum UIEffectPlayTimingType
    {
        Start,
        OnEnable,
        AfterCutSceneAnim
    }
}