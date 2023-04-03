using BagoumLib.Cancellation;
using JetBrains.Annotations;
using Suzunoya.ControlFlow;
using Suzunoya.Data;
using Suzunoya.Display;
using SuzunoyaUnity.Rendering;
using UnityEngine;

namespace SuzunoyaUnity {

public class UnityVNState : VNState {
    public UnityVNState(ICancellee extCToken, IInstanceData save) : base(extCToken, save) { }
    
    protected override RenderGroup MakeDefaultRenderGroup() => Add(new UnityRenderGroup(visible: true));

    public virtual bool ClickConfirmOrSkip() => 
        AwaitingConfirm.Value != null ? UserConfirm() : RequestSkipOperation();
}
}