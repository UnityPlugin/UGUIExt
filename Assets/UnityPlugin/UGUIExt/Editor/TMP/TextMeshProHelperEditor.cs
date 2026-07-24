#if UGUIEXT_TMP || UGUIEXT_UGUI_2
using UnityEditor;
using UnityEngine;
using UnityPlugin.EditorUtils;

namespace UnityPlugin.UGUIExt
{
    [CustomEditor(typeof(TextMeshProHelper))]
    public class TextMeshProHelperEditor : BaseEditor<TextMeshProHelper>
    {
        GUILayoutOption[] TOGGLE_LAYOUT = { GUILayout.Width(20) };

        public override void OnInspectorGUI()
        {
            var taregetProperty = serializedObject.FindProperty("target");
            var baseMaterialProperty = serializedObject.FindProperty("baseMaterial");
            var overrideOutlineProperty = serializedObject.FindProperty("overrideOutline");
            var outlineProperty = serializedObject.FindProperty("outline");
            var overrideUnderlayProperty = serializedObject.FindProperty("overrideUnderlay");
            var underlayProperty = serializedObject.FindProperty("underlay");

            using (IMGUI.Editable(false))
            {
                EditorGUILayout.PropertyField(taregetProperty);
            }

            using (IMGUI.ChangeCheck())
            {
                EditorGUILayout.PropertyField(baseMaterialProperty);

                using (IMGUI.PropertyHorizontal(outlineProperty.displayName))
                {
                    overrideOutlineProperty.boolValue = EditorGUILayout.Toggle(overrideOutlineProperty.boolValue, TOGGLE_LAYOUT);
                    if (overrideOutlineProperty.boolValue)
                    {
                        outlineProperty.colorValue = EditorGUILayout.ColorField(outlineProperty.colorValue);
                    }
                }

                using (IMGUI.PropertyHorizontal(underlayProperty.displayName))
                {
                    overrideUnderlayProperty.boolValue = EditorGUILayout.Toggle(overrideUnderlayProperty.boolValue, TOGGLE_LAYOUT);
                    if (overrideUnderlayProperty.boolValue)
                    {
                        underlayProperty.colorValue = EditorGUILayout.ColorField(underlayProperty.colorValue);
                    }
                }

#if UNITY_2022_3_OR_NEWER
                if (baseMaterialProperty.boxedValue == null)
#else
                if (baseMaterialProperty.objectReferenceValue == null)
#endif
                {
                    var text = IMGUI.GetGUIContent("Base Material is null, change Material Preset or select in Project");
                    if (text.image == null)
                    {
                        var tmp = EditorGUIUtility.TrTextContentWithIcon("", MessageType.Warning);
                        text.image = tmp.image;
                    }
                    EditorGUILayout.HelpBox(text);
                }

            }

            if (IMGUI.IsChange)
            {
                serializedObject.ApplyModifiedProperties();
                _target.UpdateMaterial();
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.Space();
            var key = _target.GetKey();
            var matDict = _target.GetAllMaterials();
            matDict.TryGetValue(key, out var mat);

            var count = 0;
            var refDict = _target.GetMaterialRef();
            if (mat) refDict.TryGetValue(mat, out count);

            EditorGUILayout.LabelField(IMGUI.GetGUIContent("Total Mateials"), IMGUI.GetGUIContent(matDict.Count.ToString()));

            using (var scope = IMGUI.Foldout("Current Material", true))
            {
                if (scope.fold)
                {
                    EditorGUILayout.ObjectField(IMGUI.GetGUIContent("Material"), mat, typeof(Material), false);
                    EditorGUILayout.LabelField(IMGUI.GetGUIContent("Ref"), IMGUI.GetGUIContent(count.ToString()));
                }
            }

            EditorGUILayout.Space();
            if (GUILayout.Button(IMGUI.GetGUIContent("Clear")))
            {
                _target.ClearDict();
            }
        }
    }
}
#endif