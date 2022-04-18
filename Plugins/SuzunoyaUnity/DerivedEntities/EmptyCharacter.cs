using BagoumLib.Culture;
using Suzunoya.ControlFlow;
using UnityEngine;

namespace SuzunoyaUnity.Derived {
public class EmptyCharacter : SZYUCharacter {
    public override bool MimicRequested => false;
    public override Sprite? ADVSpeakerIcon => null;

    public EmptyCharacter(LString name, IVNState vn) {
        this.Name = name;
        this.Container = vn;
    }
}
}