using BagoumLib.Cancellation;
using JetBrains.Annotations;
using Suzunoya.ControlFlow;
using Suzunoya.Data;
using Suzunoya.Display;
using SuzunoyaUnity.Rendering;
using UnityEngine;

namespace SuzunoyaUnity {
public class UnityVNState : VNState {
    public UnityVNState(ICancellee extCToken, InstanceData? save = null) : 
        base(extCToken, save) { }
    
    protected override RenderGroup MakeDefaultRenderGroup() => new UnityRenderGroup(this, visible: true);
}
}