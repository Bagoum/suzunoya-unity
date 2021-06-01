using System;
using SuzunoyaUnity.Rendering;
using UnityEngine;

namespace SuzunoyaUnity.Mimics {
public class RenderGroupMimic : Tokenized {
    public ArbitraryCapturer capturer = null!;
    public SpriteRenderer sr = null!;
    private UnityRenderGroup rg = null!;

    public void Initialize(UnityRenderGroup urg) {
        rg = urg;
        rg.Bind(this);
        Listen(rg.EntityActive, b => {
            if (!b) {
                capturer.Kill();
                Destroy(gameObject);
            }
        });
    }

    private void Update() {
        rg.UpdatePB();
    }
}
}