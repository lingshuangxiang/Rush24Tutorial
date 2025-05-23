using DG.Tweening;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using TwentyFour.Scripts.Utilities;

namespace TwentyFour.Scripts.Art.TweenAnimation
{
    public class AnimationBounceInAndSwing : MonoBehaviour
    {
        [SerializeField] public Transform contentTransform;

        const float PencilDuration = 2f;

        private Sequence _mySequence, _pencilSequence;

        // Start is called before the first frame update
        void Start()
        {

        }

        void OnEnable()
        {
            _mySequence = TweenUtils.DOBounceInSequence(transform);
            _mySequence.AppendInterval(.5f);
            _mySequence.JoinCallback(RotatePencil);
            _mySequence.AppendInterval(PencilDuration);
            TweenUtils.DOZoomOutSequence(transform, _mySequence, () => { Destroy(gameObject); });
        }

        void RotatePencil()
        {
            float sepDuration = PencilDuration / 6;

            _pencilSequence = DOTween.Sequence();
            _pencilSequence.Append(contentTransform.DORotate(new Vector3(0, 0, -15), sepDuration));
            _pencilSequence.Append(contentTransform.DORotate(new Vector3(0, 0, 8), sepDuration));
            _pencilSequence.Append(contentTransform.DORotate(new Vector3(0, 0, -15), sepDuration));
            _pencilSequence.Append(contentTransform.DORotate(new Vector3(0, 0, 8), sepDuration));
            _pencilSequence.Append(contentTransform.DORotate(new Vector3(0, 0, -15), sepDuration));
            _pencilSequence.Append(contentTransform.DORotate(new Vector3(0, 0, 0), sepDuration));
        }

        private void OnDestroy()
        {
            _mySequence?.Kill();
            _pencilSequence?.Kill();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}