using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TwentyFour.Scripts.Art.Triggers
{
   public class SimpleAnimTrigger : MonoBehaviour
   {
      public string triggerName;
      public string PlayName;

      public void Trigger()
      {
         TriggeTarget(triggerName);
      }

      public void PlayTarget()
      {
         GetComponent<Animator>().Play(PlayName);
      }


      public void TriggeTarget(string _target)
      {
         // GameObject.GetComponent<Animator>().SetTrigger(_target);   
         GetComponent<Animator>().SetTrigger(_target);
      }
   }
}