using UnityEditor;
using UnityEngine;

namespace UnityPlugin.UGUIExt
{
    [CustomEditor(typeof(LayoutElementExt))]
    public class LayoutElementExtEditor : Editor
    {
        SerializedProperty m_IgnoreLayout;
        SerializedProperty m_MinWidth;
        SerializedProperty m_MinHeight;
        SerializedProperty m_PreferredWidth;
        SerializedProperty m_PreferredHeight;
        SerializedProperty m_MaxWidth;
        SerializedProperty m_MaxHeight;
        SerializedProperty m_FlexibleWidth;
        SerializedProperty m_FlexibleHeight;
        SerializedProperty m_BreakFlowPrevious;
        SerializedProperty m_BreakFlowNext;
        SerializedProperty m_LayoutPriority;

        RectTransform m_Rect;


        protected virtual void OnEnable()
        {
            m_IgnoreLayout = serializedObject.FindProperty("m_IgnoreLayout");
            m_MinWidth = serializedObject.FindProperty("m_MinWidth");
            m_MinHeight = serializedObject.FindProperty("m_MinHeight");
            m_PreferredWidth = serializedObject.FindProperty("m_PreferredWidth");
            m_PreferredHeight = serializedObject.FindProperty("m_PreferredHeight");
            m_MaxWidth = serializedObject.FindProperty("m_MaxWidth");
            m_MaxHeight = serializedObject.FindProperty("m_MaxHeight");
            m_FlexibleWidth = serializedObject.FindProperty("m_FlexibleWidth");
            m_FlexibleHeight = serializedObject.FindProperty("m_FlexibleHeight");
            m_BreakFlowPrevious = serializedObject.FindProperty("m_BreakFlowPrevious");
            m_BreakFlowNext = serializedObject.FindProperty("m_BreakFlowNext");
            m_LayoutPriority = serializedObject.FindProperty("m_LayoutPriority");

            m_Rect = (target as LayoutElementExt).transform as RectTransform;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_IgnoreLayout);

            if (!m_IgnoreLayout.boolValue)
            {
                EditorGUILayout.Space();

                EditorGUI.BeginChangeCheck();

                LayoutElementField(m_MinWidth, 0);
                LayoutElementField(m_MinHeight, 0);
                LayoutElementField(m_PreferredWidth, m_Rect.rect.width);
                LayoutElementField(m_PreferredHeight, m_Rect.rect.height);
                LayoutElementField(m_MaxWidth, m_Rect.rect.width);
                LayoutElementField(m_MaxHeight, m_Rect.rect.height);
                LayoutElementField(m_FlexibleWidth, 1);
                LayoutElementField(m_FlexibleHeight, 1);

                EditorGUILayout.PropertyField(m_BreakFlowPrevious);
                EditorGUILayout.PropertyField(m_BreakFlowNext);

                if (EditorGUI.EndChangeCheck())
                {
                    if (m_MaxWidth.floatValue > 0 && m_MinWidth.floatValue > 0 && m_MaxWidth.floatValue < m_MinWidth.floatValue)
                    {
                        m_MaxWidth.floatValue = m_MinWidth.floatValue;
                    }
                    if (m_MaxHeight.floatValue > 0 && m_MinHeight.floatValue > 0 && m_MaxHeight.floatValue < m_MinHeight.floatValue)
                    {
                        m_MaxHeight.floatValue = m_MinHeight.floatValue;
                    }
                }
            }

            EditorGUILayout.PropertyField(m_LayoutPriority);

            serializedObject.ApplyModifiedProperties();
        }

        void LayoutElementField(SerializedProperty property, float defaultValue)
        {
            var position = EditorGUILayout.GetControlRect();

            // Label
            var label = EditorGUI.BeginProperty(position, null, property);

            // Rects
            var fieldPosition = EditorGUI.PrefixLabel(position, label);

            var toggleRect = fieldPosition;
            toggleRect.width = 16;

            var floatFieldRect = fieldPosition;
            floatFieldRect.xMin += 16;

            // Checkbox
            EditorGUI.BeginChangeCheck();
            var enabled = EditorGUI.ToggleLeft(toggleRect, GUIContent.none, property.floatValue >= 0);
            if (EditorGUI.EndChangeCheck())
            {
                // This could be made better to set all of the targets to their initial width, but mimizing code change for now
                property.floatValue = enabled ? defaultValue : -1;
            }

            if (!property.hasMultipleDifferentValues && property.floatValue >= 0)
            {
                // Float field
                EditorGUIUtility.labelWidth = 4; // Small invisible label area for drag zone functionality
                EditorGUI.BeginChangeCheck();
                var newValue = EditorGUI.FloatField(floatFieldRect, new GUIContent(" "), property.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    property.floatValue = Mathf.Max(0, newValue);
                }
                EditorGUIUtility.labelWidth = 0;
            }

            EditorGUI.EndProperty();
        }
    }
}
