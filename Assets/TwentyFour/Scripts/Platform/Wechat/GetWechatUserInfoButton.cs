using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.UOS.TwentyFour.Wechat
{
    public class GetWechatUserInfoButton : MonoBehaviour
    {
        public Rect GetScreenPosition()
        {
            var rectTransform = gameObject.GetComponent<RectTransform>();
            // 获取 RectTransform 的四个角的世界坐标
            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);

            // 创建屏幕矩形
            var screenRect = new Rect(
                worldCorners[0].x,
                worldCorners[0].y,
                worldCorners[2].x - worldCorners[0].x,
                worldCorners[2].y - worldCorners[0].y);

            Debug.Log($"Screen Rect: {screenRect}");
            return screenRect;
        }
    }
}