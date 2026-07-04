using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace UnityPlugin.UGUIExt
{
    public class LayoutElementExt : LayoutElement
    {
        [SerializeField] private bool m_BreakFlow = false;
        [SerializeField] private float m_MaxWidth = -1;
        [SerializeField] private float m_MaxHeight = -1;

        public virtual bool breakFlow { get { return m_BreakFlow; } set { if (SetPropertyUtility.SetStruct(ref m_BreakFlow, value)) SetDirty(); } }

        public virtual float maxWidth { get { return m_MaxWidth; } set { if (SetPropertyUtility.SetStruct(ref m_MaxWidth, value)) SetDirty(); } }
        public virtual float maxHeight { get { return m_MaxHeight; } set { if (SetPropertyUtility.SetStruct(ref m_MaxHeight, value)) SetDirty(); } }

        public override float preferredWidth { get { return GetPreferredWidth(); } set { base.preferredWidth = value; } }

        public override float preferredHeight { get { return GetPreferredHeight(); } set { base.preferredHeight = value; } }


        float m_CachePreferredWidth;
        float m_CachePreferredHeight;
        int m_FrameCount;

        public float GetPreferredWidth()
        {
            var pw = base.preferredWidth;
            if (m_MaxWidth > 0)
            {
                if (pw < 0)
                {
                    CacheOtherLayoutElementPreferredSize();
                    pw = m_CachePreferredWidth;
                }
                return pw > m_MaxWidth ? m_MaxWidth : pw;
            }

            return pw;
        }

        public float GetPreferredHeight()
        {
            var ph = base.preferredHeight;
            if (m_MaxHeight > 0)
            {
                if (ph < 0)
                {
                    CacheOtherLayoutElementPreferredSize();
                    ph = m_CachePreferredHeight;
                }

                return ph > m_MaxHeight ? m_MaxHeight : ph;
            }
            return ph;
        }

        void CacheOtherLayoutElementPreferredSize()
        {
            var frameCount = Time.frameCount;
            if (m_FrameCount == frameCount) return;

            var components = ListPool<Component>.Get();
            GetComponents(typeof(ILayoutElement), components);

            var widthPriorty = int.MinValue;
            var heightPriorty = int.MinValue;
            m_CachePreferredWidth = -1;
            m_CachePreferredHeight = -1;
            for (var i = 0; i < components.Count; i++)
            {
                var layoutElement = components[i] as ILayoutElement;
                if (layoutElement.Equals(this)) continue;

                if (layoutElement.preferredWidth > 0 && layoutElement.layoutPriority > widthPriorty)
                {
                    m_CachePreferredWidth = layoutElement.preferredWidth;
                }
                if (layoutElement.preferredHeight > 0 && layoutElement.layoutPriority > heightPriorty)
                {
                    m_CachePreferredHeight = layoutElement.preferredHeight;
                }
            }

            var rect = (transform as RectTransform).rect;
            if (m_CachePreferredWidth < 0) m_CachePreferredWidth = rect.width;
            if (m_CachePreferredHeight < 0) m_CachePreferredHeight = rect.height;

            ListPool<Component>.Release(components);

            m_FrameCount = frameCount;
        }
    }
}
