using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuzunoyaUnity.Rendering {
public class ArbitraryCapturer : Tokenized {
    public Camera Camera { get; private set; } = null!;
    public RenderTexture Captured { get; private set; } = null!;

    private void Awake() {
        Camera = GetComponent<Camera>();
    }

    protected override void BindListeners() {
        Listen(RenderHelpers.PreferredResolution, RecreateTexture);
    }
    
    private void OnDestroy() {
        Captured.Release();
    }

    public void RecreateTexture((int w, int h) res) {
        if (Captured != null)
            Captured.Release();
        Camera.targetTexture = Captured = RenderHelpers.DefaultTempRT(res);
    }

    public void Kill() {
        Destroy(gameObject);
    }
}
}