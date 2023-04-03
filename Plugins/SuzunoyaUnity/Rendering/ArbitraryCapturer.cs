using System;
using System.Collections;
using System.Collections.Generic;
using BagoumLib.Events;
using UnityEngine;

namespace SuzunoyaUnity.Rendering {
public class ArbitraryCapturer : Tokenized {
    public Camera Camera { get; private set; } = null!;
    public RenderTexture Captured { get; private set; } = null!;
    public Event<RenderTexture> RenderUpdated { get; } = new();

    private void Awake() {
        Camera = GetComponent<Camera>();
    }

    protected override void BindListeners() {
        Listen(RenderHelpers.PreferredResolution, RecreateTexture);
    }

    private void OnPostRender() {
        RenderUpdated.OnNext(Captured);
    }

    private void OnDestroy() {
        Captured.Release();
        RenderUpdated.OnCompleted();
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