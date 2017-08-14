/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2017 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

namespace GameCanvas
{
    public sealed class PrimitiveRenderer : UnityEngine.UI.Graphic
    {
        // Fields
        //----------------------------------------------------------

        private UnityEngine.UI.CanvasScaler mScaler;
        private UnityEngine.UIVertex[] mUIVertexQuad;

        // Unity methods
        //----------------------------------------------------------

        protected override void Awake()
        {
            UnityEngine.Debug.Log("Awake\n");
            base.Awake();

            initializeIfNeed();
        }

        protected override void OnCanvasGroupChanged()
        {
            UnityEngine.Debug.Log("OnCanvasGroupChanged\n");
            base.OnCanvasGroupChanged();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            UnityEngine.Debug.Log("OnRectTransformDimensionsChange\n");
            base.OnRectTransformDimensionsChange();
        }

        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
        {
            UnityEngine.Debug.Log("OnPopulateMesh\n");

            initializeIfNeed();

            UnityEngine.Vector2 pivot = rectTransform.pivot;
            UnityEngine.Vector2 resolution = mScaler.referenceResolution;
            UnityEngine.Vector2 lb, rt;
            lb.x = (0f - pivot.x) * resolution.x;
            lb.y = (0f - pivot.y) * resolution.y;
            rt.x = (1f - pivot.x) * resolution.x;
            rt.y = (1f - pivot.y) * resolution.y;
            mUIVertexQuad[0].position = new UnityEngine.Vector3(lb.x, lb.y);
            mUIVertexQuad[1].position = new UnityEngine.Vector3(lb.x, rt.y);
            mUIVertexQuad[2].position = new UnityEngine.Vector3(rt.x, rt.y);
            mUIVertexQuad[3].position = new UnityEngine.Vector3(rt.x, lb.y);

            vh.Clear();
            vh.AddUIVertexQuad(mUIVertexQuad);
        }

        public override UnityEngine.Texture mainTexture { get { return null; } }

        // Private methods
        //----------------------------------------------------------

        private void initializeIfNeed()
        {
            if (mScaler == null)
            {
                mScaler = GetComponent<UnityEngine.UI.CanvasScaler>();
            }
            if (mUIVertexQuad == null)
            {
                mUIVertexQuad = new UnityEngine.UIVertex[4];
                for (var i = 0; i < mUIVertexQuad.Length; ++i) mUIVertexQuad[i] = UnityEngine.UIVertex.simpleVert;
            }
        }
    }
}
