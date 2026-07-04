using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

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

        List<int> m_LineBreaks = new();
        List<float> m_LineWidths = new();
        List<float> m_LineHeights = new();

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

            var combinedPadding = padding.horizontal;
            var controlSize = m_ChildControlWidth;
            var useScale = m_ChildScaleWidth;
            var spacing = m_Spacing[0];

            var totalMin = 0f;
            var linePreferred = (float)combinedPadding;
            var totalPreferred = linePreferred;
            var lineCount = 0;

            var rectWidth = rectTransform.rect.width - combinedPadding;
            var rectChildrenCount = rectChildren.Count;

            m_LineBreaks.Clear();
            m_LineWidths.Clear();

            for (var i = 0; i < rectChildrenCount; i++)
            {
                var child = rectChildren[i];
                GetChildSizes(child, 0, controlSize, out var preferred);
                if (useScale) preferred *= child.localScale[0];

                if (lineCount > 0 && (linePreferred + preferred > rectWidth || IsFlowBreak(child)))
                {
                    totalPreferred = Mathf.Max(totalPreferred, linePreferred);

                    m_LineWidths.Add(linePreferred);
                    m_LineBreaks.Add(i);

                    linePreferred = combinedPadding;
                    lineCount = 0;
                }

                if (controlSize && preferred > rectWidth) preferred = rectWidth;
                if (preferred > totalMin && preferred < rectWidth) totalMin = preferred;

                linePreferred += preferred + spacing;
                lineCount++;

            }

            if (lineCount > 0)
            {
                totalPreferred = Mathf.Max(totalPreferred, linePreferred);

                m_LineWidths.Add(linePreferred);
                m_LineBreaks.Add(rectChildrenCount);
            }

            if (rectChildren.Count > 0)
            {
                totalPreferred -= spacing;
            }

            SetLayoutInputForAxis(totalMin, totalPreferred, -1, 0);
        }

        protected void CalcFlowV()
        {
            if (rectChildren.Count == 0)
                return;

            var combinedPadding = padding.vertical;
            var controlSize = m_ChildControlHeight;
            var useScale = m_ChildScaleHeight;
            var spacing = m_Spacing.y;

            var linePreferred = 0f;
            var totalPreferred = (float)combinedPadding;
            var lineCount = 0;

            var rectChildrenCount = rectChildren.Count;
            var lineIndex = 0;

            m_LineHeights.Clear();

            for (var i = 0; i < rectChildrenCount; i++)
            {
                var child = rectChildren[i];
                GetChildSizes(child, 1, controlSize, out var preferred);
                if (useScale) preferred *= child.localScale[1];

                if (lineIndex < m_LineBreaks.Count && i >= m_LineBreaks[lineIndex])
                {
                    totalPreferred += linePreferred + spacing;
                    m_LineHeights.Add(linePreferred);

                    lineIndex++;

                    linePreferred = 0f;
                    lineCount = 0;
                }

                if (preferred > linePreferred)
                {
                    linePreferred = preferred;
                }
                lineCount++;
            }

            if (lineCount > 0)
            {
                totalPreferred += linePreferred + spacing;
                m_LineHeights.Add(linePreferred);
            }

            if (rectChildren.Count > 0)
            {
                totalPreferred -= spacing;
            }
            SetLayoutInputForAxis(totalPreferred, totalPreferred, -1, 1);
        }

        protected void SetChildrenH()
        {
            if (rectChildren.Count == 0 || m_LineWidths.Count == 0)
                return;

            var controlSize = m_ChildControlWidth;
            var useScale = m_ChildScaleWidth;
            var spacing = m_Spacing[0];

            var alignmentOnAxis = GetAlignmentOnAxis(0);
            var pos = alignmentOnAxis switch
            {
                0 => padding.left,
                1 => rectTransform.rect.width - m_LineWidths[0] - padding.right,
                _ => (rectTransform.rect.width - m_LineWidths[0] - padding.horizontal) * 0.5f + padding.left,
            };

            if (m_RightToLeft) pos += m_LineWidths[0];

            var rectWidth = rectTransform.rect.width - padding.horizontal;
            var rectChildrenCount = rectChildren.Count;

            var lineIndex = 0;

            for (int i = 0; i < rectChildrenCount; i++)
            {
                var child = rectChildren[i];
                GetChildSizes(child, 0, controlSize, out var preferred);
                var scaleFactor = 1f;
                var scaledPreferred = preferred;
                if (useScale)
                {
                    scaleFactor = child.localScale[0];
                    scaledPreferred *= scaleFactor;
                }

                if (scaledPreferred > rectWidth)
                {
                    scaledPreferred = rectWidth;
                    preferred = scaledPreferred / scaleFactor;
                }

                if (m_RightToLeft) pos -= scaledPreferred;
                if (controlSize)
                {
                    SetChildAlongAxisWithScale(child, 0, pos, preferred, scaleFactor);
                }
                else
                {
                    SetChildAlongAxisWithScale(child, 0, pos, scaleFactor);
                }

                if (m_RightToLeft) pos -= spacing;
                else pos += scaledPreferred + spacing;

                if (lineIndex < m_LineBreaks.Count && i + 1 >= m_LineBreaks[lineIndex])
                {
                    lineIndex++;

                    if (lineIndex < m_LineWidths.Count)
                    {
                        pos = alignmentOnAxis switch
                        {
                            0 => padding.left,
                            1 => rectTransform.rect.width - m_LineWidths[lineIndex] - padding.right,
                            _ => (rectTransform.rect.width - m_LineWidths[lineIndex] - padding.horizontal) * 0.5f + padding.left,
                        };

                        if (m_RightToLeft) pos += m_LineWidths[lineIndex];
                    }
                }
            }
        }

        protected void SetChildrenV()
        {
            if (rectChildren.Count == 0 || m_LineHeights.Count == 0)
                return;

            var controlSize = m_ChildControlHeight;
            var useScale = m_ChildScaleHeight;
            var spacing = m_Spacing[1];

            var pos = (float)padding.top;

            var alignmentOnAxis = GetAlignmentOnAxis(1);

            var rectChildrenCount = rectChildren.Count;

            var lineIndex = 0;

            for (int i = 0; i < rectChildrenCount; i++)
            {
                var child = rectChildren[i];
                GetChildSizes(child, 1, controlSize, out var preferred);

                var scaleFactor = 1f;
                if (useScale)
                {
                    scaleFactor = child.localScale[1];
                    preferred *= scaleFactor;
                }

                var childOffset = 0f;
                if (lineIndex < m_LineWidths.Count)
                {
                    childOffset = alignmentOnAxis switch
                    {
                        0 => 0,
                        1 => m_LineHeights[lineIndex] - preferred,
                        _ => (m_LineHeights[lineIndex] - preferred) * 0.5f,
                    };
                }

                if (controlSize)
                {
                    SetChildAlongAxisWithScale(child, 1, pos + childOffset, preferred, scaleFactor);
                }
                else
                {
                    SetChildAlongAxisWithScale(child, 1, pos + childOffset, scaleFactor);
                }

                if (lineIndex < m_LineBreaks.Count && i + 1 >= m_LineBreaks[lineIndex])
                {
                    pos += m_LineHeights[lineIndex] + spacing;
                    lineIndex++;
                }
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

        protected bool IsFlowBreak(RectTransform child)
        {
            var components = ListPool<Component>.Get();
            child.GetComponents(typeof(LayoutElementExt), components);

            var count = components.Count;
            var result = false;
            for (var i = 0; i < count; i++)
            {
                var layoutElement = components[i] as LayoutElementExt;
                if (layoutElement.breakFlow)
                {
                    result = true;
                    break;
                }
            }

            ListPool<Component>.Release(components);
            return result;
        }
    }
}
