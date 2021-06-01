using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuzunoyaUnity.Rendering {
public class ArbitraryCapturer : MonoBehaviour {
    public Camera Camera { get; private set; } = null!;
    public RenderTexture Captured { get; private set; } = null!;

    private void Awake() {
        Camera = GetComponent<Camera>();
        Camera.targetTexture = Captured = Helpers.DefaultTempRT();
    }

    public void Draw(Transform tr, Mesh m, Material mat, MaterialPropertyBlock pb, int layer) =>
        UnityEngine.Graphics.DrawMesh(m, tr.localToWorldMatrix, mat, layer, Camera, 0, pb);

    private void OnDestroy() {
        Captured.Release();
    }

    public void RecreateTexture() {
        Captured.Release();
        Camera.targetTexture = Captured = Helpers.DefaultTempRT();
    }

    public void Kill() {
        Destroy(gameObject);
    }
}
}