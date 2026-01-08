using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Game.UI.Custom {
    public class InvertedMaskImage : Image
    {
        private static readonly int StencilComp = Shader.PropertyToID("_StencilComp");

        public override Material materialForRendering
        {
            get
            {
                var result = new Material(base.materialForRendering);
                result.SetInt(StencilComp, (int)CompareFunction.NotEqual);
                return result;
 
            }
        }
    }
}