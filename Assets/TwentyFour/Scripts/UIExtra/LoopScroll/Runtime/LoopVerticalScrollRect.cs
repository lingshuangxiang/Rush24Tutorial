using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Loop Vertical Scroll Rect", 51)]
    [DisallowMultipleComponent]
    public class LoopVerticalScrollRect : LoopScrollRect
    {
        // 构造函数，设置滚动方向为垂直方向
        protected LoopVerticalScrollRect()
        {
            direction = LoopScrollRectDirection.Vertical;
        }

        // 获取元素的尺寸
        protected override float GetSize(RectTransform item, bool includeSpacing)
        {
            float size = includeSpacing ? contentSpacing : 0;
            if (m_GridLayout != null)
            {
                size += m_GridLayout.cellSize.y;
            }
            else
            {
                size += LoopScrollSizeUtils.GetPreferredHeight(item);
            }
            size *= m_Content.localScale.y;
            return size;
        }

        // 获取向量的Y维度
        protected override float GetDimension(Vector2 vector)
        {
            return vector.y;
        }
        
        // 获取向量的绝对Y维度
        protected override float GetAbsDimension(Vector2 vector)
        {
            return vector.y;
        }

        // 根据值获取向量
        protected override Vector2 GetVector(float value)
        {
            return new Vector2(0, value);
        }

        // 初始化函数
        protected override void Awake()
        {
            base.Awake();
            if (m_Content)
            {
                GridLayoutGroup layout = m_Content.GetComponent<GridLayoutGroup>();
                if (layout != null && layout.constraint != GridLayoutGroup.Constraint.FixedColumnCount)
                {
                    Debug.LogError("[LoopScrollRect] unsupported GridLayoutGroup constraint");
                }
            }
        }

        // 更新元素
        protected override bool UpdateItems(ref Bounds viewBounds, ref Bounds contentBounds)
        {
            bool changed = false;

            // 如果内容超出视图范围，向上滚动
            if ((viewBounds.size.y < contentBounds.min.y - viewBounds.max.y) && itemTypeEnd > itemTypeStart)
            {
                int maxItemTypeStart = -1;
                if (totalCount >= 0)
                {
                    maxItemTypeStart = Mathf.Max(0, totalCount - (itemTypeEnd - itemTypeStart));
                }
                float currentSize = contentBounds.size.y;
                float elementSize = (currentSize - contentSpacing * (CurrentLines - 1)) / CurrentLines;
                ReturnToTempPool(true, itemTypeEnd - itemTypeStart);
                itemTypeStart = itemTypeEnd;

                int offsetCount = Mathf.FloorToInt((contentBounds.min.y - viewBounds.max.y) / (elementSize + contentSpacing));
                if (maxItemTypeStart >= 0 && itemTypeStart + offsetCount * contentConstraintCount > maxItemTypeStart)
                {
                    offsetCount = Mathf.FloorToInt((float)(maxItemTypeStart - itemTypeStart) / contentConstraintCount);
                }
                itemTypeStart += offsetCount * contentConstraintCount;
                if (totalCount >= 0)
                {
                    itemTypeStart = Mathf.Max(itemTypeStart, 0);
                }
                itemTypeEnd = itemTypeStart;

                float offset = offsetCount * (elementSize + contentSpacing);
                m_Content.anchoredPosition -= new Vector2(0, offset + (reverseDirection ? 0 : currentSize));
                contentBounds.center -= new Vector3(0, offset + currentSize / 2, 0);
                contentBounds.size = Vector3.zero;

                changed = true;
            }

            // 如果内容超出视图范围，向下滚动
            if ((viewBounds.min.y - contentBounds.max.y > viewBounds.size.y) && itemTypeEnd > itemTypeStart)
            {
                float currentSize = contentBounds.size.y;
                float elementSize = (currentSize - contentSpacing * (CurrentLines - 1)) / CurrentLines;
                ReturnToTempPool(false, itemTypeEnd - itemTypeStart);
                itemTypeEnd = itemTypeStart;

                int offsetCount = Mathf.FloorToInt((viewBounds.min.y - contentBounds.max.y) / (elementSize + contentSpacing));
                if (totalCount >= 0 && itemTypeStart - offsetCount * contentConstraintCount < 0)
                {
                    offsetCount = Mathf.FloorToInt((float)(itemTypeStart) / contentConstraintCount);
                }
                itemTypeStart -= offsetCount * contentConstraintCount;
                if (totalCount >= 0)
                {
                    itemTypeStart = Mathf.Max(itemTypeStart, 0);
                }
                itemTypeEnd = itemTypeStart;

                float offset = offsetCount * (elementSize + contentSpacing);
                m_Content.anchoredPosition += new Vector2(0, offset + (reverseDirection ? currentSize : 0));
                contentBounds.center += new Vector3(0, offset + currentSize / 2, 0);
                contentBounds.size = Vector3.zero;

                changed = true;
            }
            
            // 如果视图底部超出内容底部，添加新元素到末尾
            if (viewBounds.min.y < contentBounds.min.y + m_ContentBottomPadding)
            {
                float size = NewItemAtEnd(), totalSize = size;
                while (size > 0 && viewBounds.min.y < contentBounds.min.y + m_ContentBottomPadding - totalSize)
                {
                    size = NewItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }
            // 如果元素数量不满足约束条件，添加新元素到末尾
            else if ((itemTypeEnd % contentConstraintCount != 0) && (itemTypeEnd < totalCount || totalCount < 0))
            {
                NewItemAtEnd();
            }

            // 如果视图顶部超出内容顶部，添加新元素到开头
            if (viewBounds.max.y > contentBounds.max.y - m_ContentTopPadding)
            {
                float size = NewItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.max.y > contentBounds.max.y - m_ContentTopPadding + totalSize)
                {
                    size = NewItemAtStart();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            // 如果视图底部超出内容底部一定阈值，删除末尾元素
            if (viewBounds.min.y > contentBounds.min.y + threshold + m_ContentBottomPadding)
            {
                float size = DeleteItemAtEnd(), totalSize = size;
                while (size > 0 && viewBounds.min.y > contentBounds.min.y + threshold + m_ContentBottomPadding + totalSize)
                {
                    size = DeleteItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            // 如果视图顶部超出内容顶部一定阈值，删除开头元素
            if (viewBounds.max.y < contentBounds.max.y - threshold - m_ContentTopPadding)
            {
                float size = DeleteItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.max.y < contentBounds.max.y - threshold - m_ContentTopPadding - totalSize)
                {
                    size = DeleteItemAtStart();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            // 如果内容有变化，清空临时池
            if (changed)
            {
                ClearTempPool();
            }

            return changed;
        }
    }
}