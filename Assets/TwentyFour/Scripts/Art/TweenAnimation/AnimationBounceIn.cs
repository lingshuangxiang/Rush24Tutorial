using DG.Tweening;
using UnityEngine;
using TwentyFour.Scripts.Utilities;

namespace TwentyFour.Scripts.Art.TweenAnimation
{
    public class AnimationBounceIn : MonoBehaviour
    {
        [SerializeField] public float lifetime = 3f;

        private Sequence _mySequence;

        // Start is called before the first frame update
        void Start()
        {

        }

        void OnEnable()
        {
            _mySequence = TweenUtils.DOBounceInSequence(transform);
            if (lifetime > 0)
            {
                _mySequence.AppendInterval(lifetime);
                TweenUtils.DOZoomOutSequence(transform, _mySequence, () => { Destroy(gameObject); });
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {
            _mySequence?.Kill();
        }
    }
}