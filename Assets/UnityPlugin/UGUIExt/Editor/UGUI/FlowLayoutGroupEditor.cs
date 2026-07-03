using UnityEditor;
using UnityEngine;

namespace UnityPlugin.UGUIExt.UGUI
{
    [CustomEditor(typeof(FlowLayoutGroup))]
    public class FlowLayoutGroupEditor : Editor
    {
        SerializedProperty _paddingPropery;
        SerializedProperty _spacingPropery;
        SerializedProperty _childAlignmentPropery;
        SerializedProperty _rightToLeftPropery;
        SerializedProperty _childControlWidthPropery;
        SerializedProperty _childControlHeightPropery;
        SerializedProperty _childScaleWidthPropery;
        SerializedProperty _childScaleHeightPropery;


        GUILayoutOption[] _cloumnSize = { GUILayout.MinWidth(50), GUILayout.ExpandWidth(true) };

        void OnEnable()
        {
            _paddingPropery = serializedObject.FindProperty("m_Padding");
            _spacingPropery = serializedObject.FindProperty("m_Spacing");
            _childAlignmentPropery = serializedObject.FindProperty("m_ChildAlignment");
            _rightToLeftPropery = serializedObject.FindProperty("m_RightToLeft");
            _childControlWidthPropery = serializedObject.FindProperty("m_ChildControlWidth");
            _childControlHeightPropery = serializedObject.FindProperty("m_ChildControlHeight");
            _childScaleWidthPropery = serializedObject.FindProperty("m_ChildScaleWidth");
            _childScaleHeightPropery = serializedObject.FindProperty("m_ChildScaleHeight");
        }

        override public void OnInspectorGUI()
        {
            using (IMGUIUtils.ChangeCheck())
            {
                EditorGUILayout.PropertyField(_paddingPropery);
                EditorGUILayout.PropertyField(_spacingPropery);
                EditorGUILayout.PropertyField(_childAlignmentPropery);
                EditorGUILayout.PropertyField(_rightToLeftPropery);

                using (IMGUIUtils.PropertyHorizontal("Control Child Size"))
                {
                    _childControlWidthPropery.boolValue = EditorGUILayout.ToggleLeft(
                        IMGUIUtils.GetGUIContent("Width"),
                        _childControlWidthPropery.boolValue, _cloumnSize);

                    _childControlHeightPropery.boolValue = EditorGUILayout.ToggleLeft(
                        IMGUIUtils.GetGUIContent("Height"),
                        _childControlHeightPropery.boolValue, _cloumnSize);

                    GUILayoutUtility.GetRect(50, 0, _cloumnSize);
                }

                using (IMGUIUtils.PropertyHorizontal("Use Child Scale"))
                {
                    _childScaleWidthPropery.boolValue = EditorGUILayout.ToggleLeft(
                        IMGUIUtils.GetGUIContent("Width"),
                        _childScaleWidthPropery.boolValue, _cloumnSize);

                    _childScaleHeightPropery.boolValue = EditorGUILayout.ToggleLeft(
                        IMGUIUtils.GetGUIContent("Height"),
                        _childScaleHeightPropery.boolValue, _cloumnSize);

                    GUILayoutUtility.GetRect(50, 0, _cloumnSize);
                }
            }

            if (IMGUIUtils.IsChange) serializedObject.ApplyModifiedProperties();
        }
    }
}
