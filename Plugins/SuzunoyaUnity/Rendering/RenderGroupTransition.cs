using BagoumLib.Cancellation;
using JetBrains.Annotations;

namespace SuzunoyaUnity.Rendering {
public class RenderGroupTransition {
    public const string NO_TRANSITION_KW = "MIX_NONE";

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
        public override string KW => "MIX_FADE";
        
        public Fade(UnityRenderGroup? target, float time) : base(target, time) { }
    }

}
}