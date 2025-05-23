using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TwentyFour.Scripts.Features.Player
{
    public class CharAnimation : MonoBehaviour
    {
        public bool isCelebrating = false;

        public Animator anim;

        private void Awake()
        {
            anim = GetComponent<Animator>();
        }

        // Start is called before the first frame update
        void Start()
        {
            anim.SetFloat("BlinkInterval", Random.Range(0f, 1f));
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void PlayCelebrateAnimation()
        {
            if (anim != null)
            {
                anim.SetBool("IsLookingAround", false);
                anim.SetTrigger("Celebrate");
            }
        }

        public void PlayWorkingAnimation()
        {
            if (anim != null)
            {
                anim.SetTrigger("Working");
            }
        }

        public void PlayLookAroundAnimation()
        {
            if (anim != null)
            {
                anim.SetBool("IsLookingAround", true);
            }
        }

        public void StopLookAroundAnimation()
        {
            if (anim != null)
            {
                anim.SetBool("IsLookingAround", false);
            }
        }

        public void Idle()
        {
            StopLookAroundAnimation();
        }
    }
}