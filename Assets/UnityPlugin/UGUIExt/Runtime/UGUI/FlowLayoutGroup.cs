using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlugin.Bridge;

namespace UnityPlugin.UGUIExt
{
    public class FlowLayoutGroup : LayoutGroup
    {
        [SerializeField] protected Vector2 m_Spacing = Vector2.zero;
        public Vector2 spacing { get { return m_Spacing; } set { SetProperty(ref m_Spacing, value); } }

        [SerializeField] protected bool m_ChildControlWidth = false;
        public bool childControlWidth { get { return m_ChildControlWidth; } set { SetProperty(ref m_ChildControlWidth, value); } }

        [SerializeField] protected bool m_ChildControlHeight = false;
        public bool childControlHeight { get { return m_ChildControlHeight; } set { SetProperty(ref m_ChildControlHeight, value); } }

        [SerializeField] protected bool m_ChildScaleWidth = false;
        public bool childScaleWidth { get { return m_ChildScaleWidth; } set { SetProperty(ref m_ChildScaleWidth, value); } }

        [SerializeField] protected bool m_ChildScaleHeight = false;
        public bool childScaleHeight { get { return m_ChildScaleHeight; } set { SetProperty(ref m_ChildScaleHeight, value); } }

        [SerializeField] protected bool m_RightToLeft = false;
        public bool rightToLeft { get { return m_RightToLeft; } set { SetProperty(ref m_RightToLeft, value); } }

        List<int> m_LineBreaks = new List<int>();
        List<float> m_LineHeights = new List<float>();

        protected FlowLayoutGroup()
        { }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            CalcFlowH();
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputVertical()
        {
            CalcFlowV();
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void SetLayoutHorizontal()
        {
            SetChildrenH();
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void SetLayoutVertical()
        {
            SetChildrenV();
        }

        protected void CalcFlowH()
        {
            if (rectChildren.Count == 0)
                return;

            var combinedPadding = (float)padding.horizontal;
            var controlSize = m_ChildControlWidth;
            var useScale = m_ChildScaleWidth;
            var spacing = m_Spacing[0];

            var totalMin = 0f;
            var linePreferred = combinedPadding;
            var totalPreferred = linePreferred;

            var rectChildrenCount = rectChildren.Count;

            for (var i = 0; i < rectChildrenCount; i++)
            {
                var child = rectChildren[i];
                GetChildSizes(child, 0, controlSize, out var preferred);
                if (useScale) preferred *= child.localScale[0];

                IsFlowBreak(child, out var breakPrevious, out var breakNext);

                if (breakPrevious)
                {
                    totalPreferred = Mathf.Max(totalPreferred, linePreferred);
                    linePreferred = combinedPadding;
                }

                if (preferred > totalMin) totalMin = preferred;
                linePreferred += preferred + spacing;

                if (breakNext)
                {
                    totalPreferred = Mathf.Max(totalPreferred, linePreferred);
                    linePreferred = combinedPadding;
                }
            }

            totalPreferred = Mathf.Max(totalPreferred, linePreferred);
            if (rectChildren.Count > 0)
            {
                totalPreferred -= spacing;
            }

            SetLayoutInputForAxis(totalMin + combinedPadding, totalPreferred, -1, 0);
        }

        protected void CalcFlowV()
        {
            if (rectChildren.Count == 0)
                return;

            var combinedPadding = padding.vertical;
            var controlSize = m_ChildControlHeight;
            var useScale = m_ChildScaleHeight;
            var spacing = m_Spacing.y;

            var totalPreferred = (float)combinedPadding;

            m_LineHeights.Clear();

            var start = 0;
            for (var i = 0; i < m_LineBreaks.Count; i++)
            {
                var linePreferred = 0f;
                var end = m_LineBreaks[i];

                for (var j = start; j < end; j++)
                {
                    var child = rectChildren[j];
                    GetChildSizes(child, 1, controlSize, out var preferred);

                    var scaleFactor = useScale ? child.localScale[1] : 1;
                    var scaledPreferred = preferred * scaleFactor;

                    if (scaledPreferred > linePreferred) linePreferred = scaledPreferred;
                }

                m_LineHeights.Add(linePreferred);
                totalPreferred += linePreferred + spacing;

                start = end;
            }

            if (rectChildren.Count > 0)
            {
                totalPreferred -= spacing;
            }
            SetLayoutInputForAxis(totalPreferred, totalPreferred, -1, 1);
        }

        protected void SetChildrenH()
        {
            if (rectChildren.Count == 0)
                return;

            var controlSize = m_ChildControlWidth;
            var useScale = m_ChildScaleWidth;
            var spacing = m_Spacing[0];

            var alignmentOnAxis = GetAlignmentOnAxis(0);

            var rectWidth = rectTransform.rect.width - padding.horizontal;
            var rectChildrenCount = rectChildren.Count;

            var itemWidths = UnityListPool<float>.Get();
            itemWidths.Clear();
            var lineWidth = 0f;
            var lineCount = 0;
            m_LineBreaks.Clear();

            for (int i = 0; i < rectChildrenCount; i++)
            {
                var child = rectChildren[i];
                GetChildSizes(child, 0, controlSize, out var preferred);
                var scaleFactor = useScale ? child.localScale[0] : 1;
                var scaledPreferred = preferred * scaleFactor;

                if (scaledPreferred > rectWidth)
                {
                    scaledPreferred = rectWidth;
                    preferred = scaledPreferred / scaleFactor;
                }

                IsFlowBreak(child, out var breakPrevious, out var breakNext);

                if (lineCount > 0 && (lineWidth + scaledPreferred > rectWidth || breakPrevious))
                {
                    SetLineChildrenH(
                        lineWidth - spacing, rectWidth,
                        i - lineCount, itemWidths,
                        alignmentOnAxis, useScale, controlSize, spacing);

                    m_LineBreaks.Add(i);
                    itemWidths.Clear();
                    lineCount = 0;
                    lineWidth = 0;
                }

                lineWidth += scaledPreferred + spacing;
                lineCount++;
                itemWidths.Add(preferred);

                if (breakNext)
                {
                    SetLineChildrenH(
                        lineWidth - spacing, rectWidth,
                        i + 1 - lineCount, itemWidths,
                        alignmentOnAxis, useScale, controlSize, spacing);

                    m_LineBreaks.Add(i + 1);
                    itemWidths.Clear();
                    lineCount = 0;
                    lineWidth = 0;
                }
            }

            if (lineCount > 0)
            {
                SetLineChildrenH(
                    lineWidth - spacing, rectWidth,
                    rectChildrenCount - lineCount, itemWidths,
                    alignmentOnAxis, useScale, controlSize, spacing);

                m_LineBreaks.Add(rectChildrenCount);
                itemWidths.Clear();
            }

            UnityListPool<float>.Release(itemWidths);
        }

        protected void SetLineChildrenH(float lineWidth, float rectWidth, int startIndex, List<float> itemWidths, float alignmentOnAxis, bool useScale, bool controlSize, float spacing)
        {
            var pos = (float)padding.left;
            if (alignmentOnAxis >= 1) pos += rectWidth - lineWidth;
            else if (alignmentOnAxis > 0) pos += (rectWidth - lineWidth) * 0.5f;

            var start = startIndex;
            var end = startIndex + itemWidths.Count;
            var offset = 1;
            if (m_RightToLeft)
            {
                start = startIndex + itemWidths.Count - 1;
                end = startIndex - 1;
                offset = -1;
            }

            for (var j = start; j != end; j += offset)
            {
                var child = rectChildren[j];
                var scaleFactor = useScale ? child.localScale[0] : 1;
                var itemWidth = itemWidths[j - startIndex];
                if (controlSize)
                {
                    SetChildAlongAxisWithScale(child, 0, pos, itemWidth, scaleFactor);
                }
                else
                {
                    SetChildAlongAxisWithScale(child, 0, pos, scaleFactor);
                }
                pos += itemWidth + spacing;
            }
        }

        protected void SetChildrenV()
        {
            if (rectChildren.Count == 0)
                return;

            var controlSize = m_ChildControlHeight;
            var useScale = m_ChildScaleHeight;
            var spacing = m_Spacing[1];

            var pos = (float)padding.top;

            var alignmentOnAxis = GetAlignmentOnAxis(1);

            var start = 0;
            for (var i = 0; i < m_LineBreaks.Count; i++)
            {
                var end = m_LineBreaks[i];
                var lineHeight = m_LineHeights[i];
                for (var j = start; j < end; j++)
                {
                    var child = rectChildren[j];
                    GetChildSizes(child, 1, controlSize, out var preferred);

                    var scaleFactor = useScale ? child.localScale[1] : 1;
                    var scaledPreferred = preferred * scaleFactor;

                    var childOffset = 0f;
                    if (alignmentOnAxis >= 1) childOffset = lineHeight - scaledPreferred;
                    else if (alignmentOnAxis > 0) childOffset = (lineHeight - scaledPreferred) * 0.5f;

                    if (controlSize)
                    {
                        SetChildAlongAxisWithScale(child, 1, pos + childOffset, preferred, scaleFactor);
                    }
                    else
                    {
                        SetChildAlongAxisWithScale(child, 1, pos + childOffset, scaleFactor);
                    }
                }

                pos += lineHeight + spacing;
                start = end;
            }

        }

        protected void GetChildSizes(RectTransform child, int axis, bool controlSize, out float preferred)
        {
            if (!controlSize)
            {
                preferred = child.sizeDelta[axis];
            }
            else
            {
                preferred = LayoutUtility.GetPreferredSize(child, axis);
            }
        }

        protected void IsFlowBreak(RectTransform child, out bool previous, out bool next)
        {
            var components = UnityListPool<Component>.Get();
            child.GetComponents(typeof(LayoutElementExt), components);

            var count = components.Count;
            previous = false;
            next = false;

            for (var i = 0; i < count; i++)
            {
                var layoutElement = components[i] as LayoutElementExt;
                if (layoutElement.breakFlowNext)
                {
                    next = true;
                }
                if (layoutElement.breakFlowPrevious)
                {
                    previous = true;
                }
            }

            UnityListPool<Component>.Release(components);
        }
    }
}
