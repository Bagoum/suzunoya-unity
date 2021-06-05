using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BagoumLib.Cancellation;
using Suzunoya.ControlFlow;
using UnityEngine;

namespace SuzunoyaUnity {
//TODO extend with script id / instance data?
public abstract class BaseScript : Tokenized {
    public VNWrapper wrapper = null!;
    private readonly HashSet<(VNState, Cancellable)> scriptTokens = new HashSet<(VNState, Cancellable)>();

    public void Start() {
        _ = RunScript();
    }

    public async Task RunScript() {
        Log.Unity($"Started script {this}");
        var cT = new Cancellable();
        var vn = new UnityVNState(cT);
        scriptTokens.Add((vn, cT));
        wrapper.TrackVN(vn);
        try {
            await _RunScript(vn);
        } catch (Exception e) {
            if (e is OperationCanceledException && cT.Cancelled) {
                Log.Unity("VN object has been cancelled");
            } else {
                Log.UnityException(e);
            }
        }
        Log.Unity($"Done with running script {this}. Final state: {cT.ToCompletion()}");
        cT.Cancel();
        vn.DeleteAll();
    }

    protected abstract Task _RunScript(UnityVNState vn);


    public void HardCancel() {
        if (scriptTokens.Count == 0) return;
        foreach (var (vn, cT) in scriptTokens.ToArray()) {
            cT.Cancel();
            vn.DeleteAll();
        }
        scriptTokens.Clear();
    }

    protected override void OnDisable() {
        HardCancel();
        base.OnDisable();
    }
}
}