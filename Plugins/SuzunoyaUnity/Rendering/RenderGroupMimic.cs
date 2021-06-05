using System;
using SuzunoyaUnity.Rendering;
using UnityEngine;

namespace SuzunoyaUnity.Mimics {
public class RenderGroupMimic : Tokenized {
    public ArbitraryCapturer capturer = null!;
    public SpriteRenderer sr = null!;
    private UnityRenderGroup rg = null!;
    private float baseOrthoSize;

    public void Initialize(UnityRenderGroup urg) {
        baseOrthoSize = capturer.Camera.orthographicSize;
        
        rg = urg;
        rg.Bind(this);
        rg.Location.Value = capturer.Camera.transform.localPosition._();

        Listen(rg.Location, _ => SetCameraLocation());
        Listen(rg.EulerAnglesD, v3 => capturer.Camera.transform.localEulerAngles = v3._());
        //Not listening to scale in v0.1
        Listen(rg.Priority, i => sr.sortingOrder = i);
        Listen(rg.Visible, b => sr.enabled = b);
        Listen(rg.Zoom, z => capturer.Camera.orthographicSize = baseOrthoSize / z);
        //Don't need ZoomTarget
        Listen(rg.ZoomTransformOffset, _ => SetCameraLocation());
        //Don't need RendererAdded
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
    
    private void SetCameraLocation() =>
        capturer.Camera.transform.localPosition = (rg.Location.Value + rg.ZoomTransformOffset)._();
}
}