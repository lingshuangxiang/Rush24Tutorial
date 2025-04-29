using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.UOS.TwentyFour
{
    public class PageIndicator : MonoBehaviour
    {
        [SerializeField]
        public GameObject PointPrefab;
        
        int focusIndex = 0;
        private List<PageIndicatorPoint> points;
        
        public void Init(int total = 1, int current = 0)
        {
            focusIndex = current;
            points = new List<PageIndicatorPoint>();
            for (int i = 0; i < total; i++)
            {
                var pointGo = Instantiate(PointPrefab, transform);
                var point = pointGo.GetComponent<PageIndicatorPoint>();
                point.SetFocused(i == current);
                points.Add(point);
            }
        }

        public void UpdateFocus()
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i].SetFocused(i == focusIndex);
            }
        }
        
        public void GotoPreviousPage()
        {
            focusIndex = focusIndex > 1 ? focusIndex - 1 : 0;
            UpdateFocus();
        }
        
        public void GotoNextPage()
        {
            focusIndex = focusIndex < points.Count - 1 ? focusIndex + 1 : points.Count - 1;
            UpdateFocus();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}