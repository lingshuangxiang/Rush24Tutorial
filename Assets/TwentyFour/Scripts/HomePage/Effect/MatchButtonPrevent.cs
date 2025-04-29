using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchButtonPrevent : MonoBehaviour
{
    
    public void ApperanceByState(string state)
    {
        if (state == "matched"||state == "awaitingAssignment")
        {
            this.GetComponent<Button>().GetComponent<Button>().interactable = false;
        }
        else
        {
            this.GetComponent<Button>().GetComponent<Button>().interactable = true;
        }
    }
}
