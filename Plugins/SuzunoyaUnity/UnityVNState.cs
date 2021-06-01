using BagoumLib.Cancellation;
using JetBrains.Annotations;
using Suzunoya.ControlFlow;
using Suzunoya.Data;
using Suzunoya.Display;
using SuzunoyaUnity.Rendering;
using UnityEngine;

namespace SuzunoyaUnity {
public class UnityVNState : VNState {
    public UnityVNState(ICancellee extCToken, string? scriptId = null, InstanceData? save = null) : 
        base(extCToken, scriptId, save) { }
    
    public override RenderGroup MakeDefaultRenderGroup() => new UnityRenderGroup(this, visible: true, priority: 10);
}
}