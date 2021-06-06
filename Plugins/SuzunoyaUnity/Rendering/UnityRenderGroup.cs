using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using BagoumLib;
using BagoumLib.Cancellation;
using BagoumLib.DataStructures;
using BagoumLib.Mathematics;
using BagoumLib.Tasks;
using JetBrains.Annotations;
using Suzunoya;
using Suzunoya.ControlFlow;
using Suzunoya.Display;
using SuzunoyaUnity.Mimics;
using SuzunoyaUnity.Rendering;
using UnityEngine;

namespace SuzunoyaUnity.Rendering {

public class UnityRenderGroup : RenderGroup {
    private const int maxRenderGroups = 3;
    private string RenderGroupLayer(int i) => $"RenderGroup{i}";
    public const string OutRenderLayer = "UI";
    //public const string NullRenderLayer = "RenderGroupNull";
    private static readonly DMCompactingArray<UnityRenderGroup> allRGs = new DMCompactingArray<UnityRenderGroup>();

    /// <summary>
    /// Captures all objects in a render group (sharing the same camera layer)
    /// and renders them into a RenderTexture.
    /// </summary>
    private ArbitraryCapturer capturer = null!;
    /// <summary>
    /// Applies effects and transitions to the captured RenderTexture to render it to screen.
    /// </summary>
    private SpriteRenderer displayer = null!;
    private MaterialPropertyBlock pb = null!;
    private Material mat = null!;
    public Camera Camera => capturer.Camera;
    public RenderTexture Captured => capturer.Captured;

    private int layer;
    public int LayerId { get; private set; }
    private int layerMask;

    public UnityRenderGroup(IVNState container, string key = "$default", int priority = 0, 
        bool visible = false) : base(container, key, priority, visible) {
        AddToken(allRGs.Add(this));
    }

    public void Bind(RenderGroupMimic mimic) {
        capturer = mimic.capturer;
        displayer = mimic.sr;
        mat = displayer.material;
        mat.EnableKeyword(RenderGroupTransition.NO_TRANSITION_KW);
        displayer.GetPropertyBlock(pb = new MaterialPropertyBlock());
        UpdatePB();
        SetLayer(FindNextLayer());
    }

    public void UpdatePB() {
        pb.SetTexture(PropConsts.RGTex, Captured);
        displayer.SetPropertyBlock(pb);
    }

    private int FindNextLayer() {
        var opts = new HashSet<int>();
        for (int ii = 0; ii < maxRenderGroups; ++ii)
            opts.Add(ii);
        foreach (var m in allRGs) {
            if (m != this)
                opts.Remove(m.layer);
        }
        foreach (var x in opts)
            return x;
        throw new Exception($"No layers available for render group mimic {Key}");
    }

    private void SetLayer(int i) {
        layer = i;
        var layerName = RenderGroupLayer(i);
        LayerId = LayerMask.NameToLayer(layerName);
        layerMask = LayerMask.GetMask(layerName);
        capturer.Camera.cullingMask = layerMask;
    }

    public VNOperation DoTransition(RenderGroupTransition transition) => 
        this.MakeVNOp(ct => {
            ct = this.BindLifetime(ct);
            var done = WaitingUtils.GetCompletionAwaiter(out var t);
        if (transition is RenderGroupTransition.TwoGroup tg) {
            Run(BasicTwoWayTransition(tg, ct, done));
        } else
            throw new Exception($"Cannot handle transition {transition}<{transition.GetType()}>");
        return t;
    }, false); //Don't allow user skip on render transition

    private IEnumerator BasicTwoWayTransition(RenderGroupTransition.TwoGroup tg, ICancellee ct, Action<Completion> done) {
        mat.DisableKeyword(RenderGroupTransition.NO_TRANSITION_KW);
        mat.EnableKeyword(tg.KW);
        for (float t = 0; t < tg.time; t += Container.dT) {
            if (ct.Cancelled)
                break;
            pb.SetTexture(PropConsts.RGTex2, tg.target.Captured);
            pb.SetFloat(PropConsts.T, t / tg.time);
            yield return null;
        }
        mat.DisableKeyword(tg.KW);
        mat.EnableKeyword(RenderGroupTransition.NO_TRANSITION_KW);
        Visible.Value = false;
        tg.target.Visible.Value = true;
        done(ct.ToCompletion());
    }
    
}
}