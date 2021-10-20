using BagoumLib.Cancellation;
using JetBrains.Annotations;
using Suzunoya.ControlFlow;
using Suzunoya.Data;
using Suzunoya.Display;
using SuzunoyaUnity.Rendering;
using UnityEngine;

namespace SuzunoyaUnity {
public interface IUnityVNState : IVNState {
    void ClickConfirm();
}
public class UnityVNState : VNState, IUnityVNState {
    public UnityVNState(ICancellee extCToken, InstanceData? save = null) : 
        base(extCToken, save) { }
    
    protected override RenderGroup MakeDefaultRenderGroup() => new UnityRenderGroup(this, visible: true);

    protected bool ClickConfirmAllowed { get; set; } = true;
    public void ClickConfirm() {
        if (ClickConfirmAllowed)
            UserConfirm();
    }

}
}