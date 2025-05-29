using DG.Tweening;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace TwentyFour.Scripts.Art.TweenAnimation
{
    public class AnimationRotateInfinite : MonoBehaviour
    {
        const float RotateDuration = 6f;

        private Tween _myTween;

        // Start is called before the first frame update
        void Start()
        {

        }

        void OnEnable()
        {
            if (_myTween == null)
            {
                _myTween = transform.DOLocalRotate(new Vector3(0, 0, 360), RotateDuration, RotateMode.FastBeyond360)
                    .From(Vector3.zero).SetEase(Ease.Linear).SetLoops(-1);
                _myTween.SetLoops(-1);
            }
            else
            {
                _myTween.Play();
            }
        }

        private void OnDisable()
        {
            // _myTween?.Kill();
        }

        private void OnDestroy()
        {
            _myTween?.Kill();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}