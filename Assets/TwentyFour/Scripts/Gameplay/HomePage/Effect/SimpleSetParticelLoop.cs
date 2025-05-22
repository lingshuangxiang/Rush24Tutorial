
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSetParticelLoop : MonoBehaviour
{
    public void SetLoop(bool particelLoop)
    {
      GetComponent<ParticleSystem>().loop = particelLoop;   
    }
}
