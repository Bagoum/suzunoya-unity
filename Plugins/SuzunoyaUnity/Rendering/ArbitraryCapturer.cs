using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuzunoyaUnity.Rendering {
public class ArbitraryCapturer : Tokenized {
    public Camera Camera { get; private set; } = null!;
    public RenderTexture Captured { get; private set; } = null!;

    private void Awake() {
        Camera = GetComponent<Camera>();
        Camera.targetTexture = Captured = RenderHelpers.DefaultTempRT();
    }

    protected override void BindListeners() {
        Listen(RenderHelpers.PreferredResolution, _ => RecreateTexture());
    }
    
    private void OnDestroy() {
        Captured.Release();
    }

    public void RecreateTexture() {
        Captured.Release();
        Camera.targetTexture = Captured = RenderHelpers.DefaultTempRT();
    }

    public void Kill() {
        Destroy(gameObject);
    }
}
}