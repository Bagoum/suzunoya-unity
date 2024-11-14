using BagoumLib.Cancellation;
using JetBrains.Annotations;
using UnityEngine;

namespace SuzunoyaUnity.Rendering {
public static class CombinerKeywords {
    public const string FROM_ONLY = "MIX_FROM_ONLY";
    public const string TO_ONLY = "MIX_TO_ONLY";
    public const string WIPE_TEX = "MIX_WIPE_TEX";
    public const string WIPE1 = "MIX_WIPE1";
    public const string WIPEFROMCENTER = "MIX_WIPE_CENTER";
    public const string WIPEY = "MIX_WIPE_Y";
    public const string ALPHA = "MIX_ALPHA_BLEND";
    private static readonly string[] kws = {FROM_ONLY, TO_ONLY, ALPHA, WIPE_TEX, WIPE1, WIPEFROMCENTER, WIPEY};

    public static void Apply(Material mat, string keyword) {
        foreach (var kw in kws) mat.DisableKeyword(kw);
        mat.EnableKeyword(keyword);
    }
}

public class RenderGroupTransition {
    public abstract class TwoGroup : RenderGroupTransition {
        public abstract string KW { get; }
        public readonly UnityRenderGroup? target;
        public readonly float time;
        
        protected TwoGroup(UnityRenderGroup? target, float time) {
            this.target = target;
            this.time = time;
        }
    }

    public class Fade : TwoGroup {
        public override string KW => CombinerKeywords.ALPHA;
        
        public Fade(UnityRenderGroup? target, float time) : base(target, time) { }
    }

}
}