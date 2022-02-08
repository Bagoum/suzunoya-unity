using System;
using BagoumLib;
using BagoumLib.Events;
using Suzunoya.Entities;
using SuzunoyaUnity.Rendering;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace SuzunoyaUnity.Mimics {
public class RenderGroupMimic : Tokenized {
    public ArbitraryCapturer capturer = null!;
    public SpriteRenderer sr = null!;
    public Texture2D transparentTex = null!;
    public Texture2D defaultMaskTex = null!;
    private UnityRenderGroup rg = null!;
    private float baseOrthoSize;

    public void Initialize(UnityRenderGroup urg) {
        baseOrthoSize = capturer.Camera.orthographicSize;
        
        rg = urg;
        rg.Bind(this);
        //This maintains the Z-offset even when panning around carelessly
        tokens.Add(rg.ComputedLocation.AddDisturbance(new Evented<Vector3>(capturer.Camera.transform.localPosition._())));
        rg.RenderLayer.Value = sr.sortingLayerID;

        Listen(rg.NestedRenderGroup,
            r => gameObject.layer = r is UnityRenderGroup ur ?
                ur.LayerId :
                UnityRenderGroup.OutRenderLayerID);
        Listen(rg.ComputedLocation, _ => SetCameraLocation());
        Listen(rg.EulerAnglesD, v3 => capturer.Camera.transform.localEulerAngles = v3._());
        //Not listening to scale in v0.1
        Listen(rg.RenderLayer, layer => sr.sortingLayerID = layer);
        Listen(rg.Priority, i => sr.sortingOrder = i);
        Listen(rg.Visible, b => sr.enabled = b);
        Listen(rg.ComputedTint, c => sr.color = c._());
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
        capturer.Camera.transform.localPosition = (rg.ComputedLocation.Value + rg.ZoomTransformOffset)._();


}
}