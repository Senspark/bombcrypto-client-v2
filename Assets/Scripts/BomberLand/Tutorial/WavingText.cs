using TMPro;

using UnityEngine;

public class WavingText : MonoBehaviour {
    [SerializeField]
    private TMP_Text textComponent;
    
    private void Update() {
        textComponent.ForceMeshUpdate();
        var textInfo = textComponent.textInfo;
        for (var i = 0; i < textInfo.characterCount; ++i) {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) {
                continue;
            }
            var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            var x = verts[charInfo.vertexIndex].x;
            var y = Mathf.Sin(Time.time * 14f + x * 0.04f) * 2;

            for (var j = 0; j < 4; ++j) {
                var orig = verts[charInfo.vertexIndex + j];
                verts[charInfo.vertexIndex + j] =
                    orig + new Vector3(0, y, 0);
            }
        }
        
        for (var i = 0; i < textInfo.meshInfo.Length; ++i) {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            textComponent.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}