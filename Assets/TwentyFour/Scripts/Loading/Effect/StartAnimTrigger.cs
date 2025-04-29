using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartAnimTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    public string StartAnim;

    public void OnEnable()
    {
        this.GetComponent<Animator>().Play(StartAnim);
    }
}
