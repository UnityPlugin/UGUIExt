using UnityEngine;
using UnityEngine.UI;

namespace UnityPlugin.UGUIExt
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class EmptyGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}
