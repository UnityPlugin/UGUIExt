using UnityEditor;

namespace UnityPlugin.UGUIExt
{
    [CustomEditor(typeof(EmptyGraphic))]
    public class EmptyGraphicEditor : Editor
    {
        override public void OnInspectorGUI()
        {
            IMGUIUtils.DrawPropertiesExcluding(serializedObject,
                "m_Material",
                "m_Color",
                "m_Maskable",
                "m_OnCullStateChanged");
        }
    }
}
