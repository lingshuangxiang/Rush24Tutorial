
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TwentyFour.Scripts.Features.Player;

namespace TwentyFour.Scripts.Art.Effects
{
    public class SimpleSetParticelLoop : MonoBehaviour
    {
        public void SetLoop(bool particelLoop)
        {
            var main = GetComponent<ParticleSystem>().main;
            main.loop = particelLoop;
        }
    }
}