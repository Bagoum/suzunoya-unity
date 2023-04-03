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
using Object = UnityEngine.Object;

namespace SuzunoyaUnity.Rendering {
public class UnityRenderGroupMask {
    public readonly Texture2D mask;
    public readonly bool destroyOnDone;
    private bool destroyed;

    public UnityRenderGroupMask(Func<float, float, float> filter, int w = 160, int h = 90, bool destroyOnDone = true) {
        mask = Utils.CreateMask(w, h, filter);
        this.destroyOnDone = destroyOnDone;
    }

    public UnityRenderGroupMask(Texture2D mask, bool destroyOnDone) {
        this.mask = mask;
        this.destroyOnDone = destroyOnDone;
    }

    public void Done() {
        if (destroyOnDone && !destroyed) {
            Object.Destroy(mask);
            Logging.Logs.Log("Destroyed render group mask due to out-of-scope");
        }
        destroyed = true;
    }
}
public class UnityRenderGroup : RenderGroup {
    private const int maxRenderGroups = 5;
    private string RenderGroupLayer(int i) => $"RenderGroup{i}";
    public const string OutRenderLayer = "UI";
    public static int OutRenderLayerID => LayerMask.NameToLayer(OutRenderLayer);
    //public const string NullRenderLayer = "RenderGroupNull";
    private static readonly DMCompactingArray<UnityRenderGroup> allRGs = new();
    private RenderGroupMimic mimic = null!;
    public int LayerId { get; private set; }

    public UnityRenderGroupMask Mask { get; private set; } = new UnityRenderGroupMask(null!, false);

    public void SetMask(UnityRenderGroupMask? mask) {
        Mask.Done();
        Mask = mask ?? new UnityRenderGroupMask(mimic.defaultMaskTex, false);
        pb.SetTexture(PropConsts.MaskTex, Mask.mask);
    }

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
    private int layer;
    private int layerMask;
    public Camera Camera => capturer.Camera;
    public RenderTexture Captured => capturer.Captured;

    

    public UnityRenderGroup(string key = "$default", int priority = 0, 
        bool visible = false) : base(key, priority, visible) {
        AddToken(allRGs.Add(this));
    }

    public void Bind(RenderGroupMimic mimic_) {
        mimic = mimic_;
        capturer = mimic.capturer;
        displayer = mimic.sr;
        mat = displayer.material;
        mat.EnableKeyword(RenderGroupTransition.NO_TRANSITION_KW);
        displayer.GetPropertyBlock(pb = new MaterialPropertyBlock());
        SetMask(null);
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

    public VNOperation DoTransition(RenderGroupTransition transition, bool reverse = false) => 
        this.MakeVNOp(_ct => {
            var ct = this.BindLifetime(_ct);
            var done = WaitingUtils.GetCompletionAwaiter(out var t);
        if (transition is RenderGroupTransition.TwoGroup tg) {
            Run(BasicTwoWayTransition(tg, reverse, ct, done));
        } else
            throw new Exception($"Cannot handle transition {transition}<{transition.GetType()}>");
        return t;
    }, false); //Don't allow user skip on render transition

    private IEnumerator BasicTwoWayTransition(RenderGroupTransition.TwoGroup tg, bool reverse, ICancellee ct, Action<Completion> done) {
        mat.DisableKeyword(RenderGroupTransition.NO_TRANSITION_KW);
        mat.EnableKeyword(tg.KW);
        Visible.Value = true;
        for (float t = 0; t < tg.time; t += Container.dT) {
            if (ct.Cancelled)
                break;
            pb.SetTexture(PropConsts.RGTex2, tg.target == null ? mimic.transparentTex : (Texture)tg.target.Captured);
            pb.SetFloat(PropConsts.T, reverse ? (1 - t / tg.time) : t / tg.time);
            yield return null;
        }
        mat.DisableKeyword(tg.KW);
        mat.EnableKeyword(RenderGroupTransition.NO_TRANSITION_KW);
        Visible.Value = reverse;
        if (tg.target != null)
            tg.target.Visible.Value = !reverse;
        done(ct.ToCompletion());
    }

    public override void Delete() {
        Mask.Done();
        base.Delete();
    }
}
}