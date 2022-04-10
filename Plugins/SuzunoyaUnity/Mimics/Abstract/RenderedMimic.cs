using BagoumLib;
using Suzunoya.Entities;
using SuzunoyaUnity.Rendering;
using UnityEngine;
using Transform = UnityEngine.Transform;

namespace SuzunoyaUnity.Mimics {
public abstract class RenderedMimic : BaseMimic {
    
    //public string defaultSortingLayer = "";
    public abstract string SortingLayerFromPrefab { get; }
    public Transform tr { get; private set; } = null!;
    
    protected virtual void Awake() {
        tr = transform;
    }
    
    public override void _Initialize(IEntity ent) => Initialize((ent as Rendered)!);
    public void Initialize(Rendered rd) {
        Listen(rd.OnUpdate, DoUpdate);
        Listen(rd.EntityActive, b => {
            if (!b)
                EntityDestroyed();
        });
        
        Listen(rd.ComputedLocation, v3 => tr.localPosition = v3._());
        Listen(rd.ComputedEulerAnglesD, v3 => tr.localEulerAngles = v3._());
        Listen(rd.ComputedScale, v3 => tr.localScale = v3._());

        rd.RenderLayer.Value = SortingLayer.NameToID(SortingLayerFromPrefab);
        Listen(rd.RenderGroup, rg => {
            if (rg is UnityRenderGroup urg)
                gameObject.SetLayerRecursively(urg.LayerId);
        });
        Listen(rd.RenderLayer, SetSortingLayer);
        Listen(rd.SortingID, SetSortingID);
        Listen(rd.Visible, SetVisible);
        Listen(rd.ComputedTint, t => SetTint(t._()));
    }
    
    protected virtual void DoUpdate(float dT) { }
    
    protected abstract void SetSortingLayer(int layer);
    protected abstract void SetSortingID(int id);
    protected abstract void SetVisible(bool visible);
    protected abstract void SetTint(Color c);

    protected virtual void EntityDestroyed() {
        Destroy(gameObject);
    }
}
}