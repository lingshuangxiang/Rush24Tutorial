using DG.Tweening;
using UnityEngine;

namespace TwentyFour.Scripts.Art.UIEffect
{
    public class UIRotateEffect : MonoBehaviour
    {
        public int Duration = 10;

        private void OnEnable()
        {
            transform.DOLocalRotate(new Vector3(0, 0, 360), Duration, RotateMode.FastBeyond360).From(Vector3.zero)
                .SetEase(Ease.Linear).SetLoops(-1);

        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
