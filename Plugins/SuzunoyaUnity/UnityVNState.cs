using BagoumLib.Cancellation;
using JetBrains.Annotations;
using Suzunoya.ControlFlow;
using Suzunoya.Data;
using Suzunoya.Display;
using SuzunoyaUnity.Rendering;
using UnityEngine;

namespace SuzunoyaUnity {
public interface IUnityVNState : IVNState {
    bool ClickConfirmAllowed { get; }
    void ClickConfirm();

    bool ClickConfirmOrSkip() {
        if (ClickConfirmAllowed) 
            return AwaitingConfirm.Value != null ? 
                UserConfirm() : 
                RequestSkipOperation();
        return false;
    }
}
public class UnityVNState : VNState, IUnityVNState {
    public UnityVNState(ICancellee extCToken, IInstanceData save) : 
        base(extCToken, save) { }
    
    protected override RenderGroup MakeDefaultRenderGroup() => new UnityRenderGroup(this, visible: true);

    public bool ClickConfirmAllowed { get; protected set; } = true;
    public void ClickConfirm() {
        if (ClickConfirmAllowed)
            UserConfirm();
    }

}
}