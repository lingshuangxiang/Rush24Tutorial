using UnityEngine;

namespace TwentyFour.Scripts.Art.Effects
{
    public class StartAnimTrigger : MonoBehaviour
    {
        // Start is called before the first frame update
        public string StartAnim;

        public void OnEnable()
        {
            this.GetComponent<Animator>().Play(StartAnim);
        }
    }
}