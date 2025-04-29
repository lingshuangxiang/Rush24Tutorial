using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SimpleGuide : MonoBehaviour
{
    /// <summary>
    /// 将坐标转换成一个
    /// </summary>
    /// <param name="mtarget"></param>
    /// <param name="manchoredPosition"></param>
    /// <param name="targetTransform"></param>
    /// <param name="mfinalCamera"></param>
    /// <returns></returns>
   public static Vector2 GetAnchoredPosition(Camera mtarget, Vector2 manchoredPosition,RectTransform targetTransform,Camera mfinalCamera=null)
   {
       if (mfinalCamera == null)
           mfinalCamera = mtarget;
       Vector2 getscreenpoint= RectTransformUtility.WorldToScreenPoint(mtarget, manchoredPosition);
       Vector2 mouseUGUIPos = Vector2.zero;
       RectTransformUtility.ScreenPointToLocalPointInRectangle(targetTransform, getscreenpoint, mfinalCamera, out mouseUGUIPos);
       return mouseUGUIPos;
   }
   
   
    
    
    
   
   
}
[System.Serializable]
public class SimpleGuideEvent
{
    public GameObject guideMaskObject;
    public string GuideTriggerLocal;
    public List<GameObject> guideObjects = new List<GameObject>();
    public Button waitToTriggeredButton;
    public UnityEvent TriggeredEvent=new UnityEvent();
    
}
