using Suzunoya.ControlFlow;
using UnityEngine;

namespace SuzunoyaUnity.Derived {
public class EmptyCharacter : SZYUCharacter {
    public override bool MimicRequested => false;
    public override Sprite? ADVSpeakerIcon => null;

    public override string Name { get; }

    public EmptyCharacter(string name, IVNState vn) {
        this.Name = name;
        this.Container = vn;
    }
}
}