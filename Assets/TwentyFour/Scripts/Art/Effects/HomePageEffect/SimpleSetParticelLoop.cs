
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TwentyFour.Scripts.Features.Player;

public class SimpleSetParticelLoop : MonoBehaviour
{
    public void SetLoop(bool particelLoop)
    {
      GetComponent<ParticleSystem>().loop = particelLoop;   
    }
}
