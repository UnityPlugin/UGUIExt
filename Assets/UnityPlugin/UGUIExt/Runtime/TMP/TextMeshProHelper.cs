using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UnityPlugin.UGUIExt
{
    [RequireComponent(typeof(TMP_Text))]
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class TextMeshProHelper : MonoBehaviour
    {
        public struct FontMaterialKey : IEquatable<FontMaterialKey>
        {
            public Material fontMaterial;
            public Color? outlineColor;
            public Color? underlayColor;

            public bool IsValid()
            {
                return fontMaterial && outlineColor != null || underlayColor != null;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = 17;
                    hash = hash * 31 + fontMaterial?.GetHashCode() ?? 0;
                    hash = hash * 31 + outlineColor.GetHashCode();
                    hash = hash * 31 + underlayColor.GetHashCode();
                    return hash;
                }
            }

            public override bool Equals(object obj)
            {
                return obj is FontMaterialKey key && Equals(key);
            }

            public bool Equals(FontMaterialKey other)
            {
                if (fontMaterial != other.fontMaterial) return false;
                if (outlineColor != other.outlineColor) return false;
                if (underlayColor != other.underlayColor) return false;

                return true;
            }
        }

        static readonly int PROPERTY_OUTLINECOLOR = Shader.PropertyToID("_OutlineColor");
        static readonly int PROPERTY_UNDERLINECOLOR = Shader.PropertyToID("_UnderlayColor");

        static Dictionary<FontMaterialKey, Material> _materialDict;
        static Dictionary<Material, int> _materialRefDict;

        [SerializeField] TMP_Text target;
        [SerializeField] Material baseMaterial;
        [SerializeField] bool overrideOutline;
        [SerializeField] Color outline = new Color(0, 0, 0, 1);
        [SerializeField] bool overrideUnderlay;
        [SerializeField] Color underlay = new Color(0, 0, 0, 0.5f);

        FontMaterialKey _key;

        void OnEnable()
        {
            UpdateMaterial();
        }

        void OnDisable()
        {
            RemoveMaterial();
        }

        public void UpdateMaterial()
        {
            if (_materialDict == null) _materialDict = new Dictionary<FontMaterialKey, Material>();
            if (_materialRefDict == null) _materialRefDict = new Dictionary<Material, int>();

            if (target == null || target.gameObject != gameObject)
            {
                target = gameObject.GetComponent<TMP_Text>();
            }

            RemoveMaterial();
            UpdateKey();

            if (!_key.IsValid() || baseMaterial == null) return;

            if (!_materialDict.TryGetValue(_key, out var mat) || mat == null)
            {
                var c1 = "00000000";
                var c2 = "00000000";
                mat = new Material(baseMaterial);

                if (_key.outlineColor != null && mat.HasProperty(PROPERTY_OUTLINECOLOR))
                {
                    c1 = ColorUtility.ToHtmlStringRGBA(_key.outlineColor.Value);
                    mat.SetColor(PROPERTY_OUTLINECOLOR, _key.outlineColor.Value);
                }
                if (_key.underlayColor != null && mat.HasProperty(PROPERTY_UNDERLINECOLOR))
                {
                    c2 = ColorUtility.ToHtmlStringRGBA(_key.underlayColor.Value);
                    mat.SetColor(PROPERTY_UNDERLINECOLOR, _key.underlayColor.Value);
                }

                mat.name = $"{baseMaterial.name}_{c1}_{c2}";
                _materialDict[_key] = mat;
                if (_materialRefDict.ContainsKey(mat)) _materialRefDict[mat]++;
                else _materialRefDict[mat] = 1;
            }

            if (mat) target.fontSharedMaterial = mat;
        }

        void UpdateKey()
        {
            if (target == null || target.font == null) return;

            if (baseMaterial == null) baseMaterial = target.fontSharedMaterial;
            if (baseMaterial == null) baseMaterial = target.font.material;

            _key.fontMaterial = baseMaterial;

            if (overrideOutline) _key.outlineColor = outline;
            else _key.outlineColor = null;

            if (overrideUnderlay) _key.underlayColor = underlay;
            else _key.underlayColor = null;
        }

        void RemoveMaterial()
        {
            if (target == null || baseMaterial == null) return;

            var mat = target.fontSharedMaterial;
            if (mat && _materialRefDict.ContainsKey(mat))
            {
                _materialRefDict[mat]--;
                if (_materialRefDict[mat] <= 0)
                {
                    _materialRefDict.Remove(mat);
                    _materialDict.Remove(_key);
                }
            }

            target.fontSharedMaterial = baseMaterial;
        }

#if UNITY_EDITOR
        public void ClearDict()
        {
            _key.fontMaterial = baseMaterial;
            if (_materialDict != null) _materialDict.Clear();
            if (_materialRefDict != null) _materialRefDict.Clear();
        }

        public FontMaterialKey GetKey()
        {
            return _key;
        }

        public Dictionary<FontMaterialKey, Material> GetAllMaterials()
        {
            return _materialDict;
        }

        public Dictionary<Material, int> GetMaterialRef()
        {
            return _materialRefDict;
        }
#endif

    }
}