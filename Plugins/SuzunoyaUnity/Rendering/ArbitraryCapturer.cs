using System;
using System.Collections;
using System.Collections.Generic;
using BagoumLib.Events;
using UnityEngine;

namespace SuzunoyaUnity.Rendering {
public class ArbitraryCapturer : Tokenized {
    public Camera Camera { get; private set; } = null!;
    public Evented<RenderTexture> Captured { get; } = new(null!);

    private void Awake() {
        Camera = GetComponent<Camera>();
    }

    protected override void BindListeners() {
        Listen(RenderHelpers.PreferredResolution, RecreateTexture);
    }

    private void OnDestroy() {
        Captured.Value.Release();
        Captured.OnCompleted();
        Captured.OnNext(null!);
    }

    public void RecreateTexture((int w, int h) res) {
        if (Captured.Value != null)
            Captured.Value.Release();
        Camera.targetTexture = Captured.Value = RenderHelpers.DefaultTempRT(res);
    }

    public void Kill() {
        Destroy(gameObject);
    }
}
}