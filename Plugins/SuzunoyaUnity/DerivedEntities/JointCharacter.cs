﻿using System.Linq;
using BagoumLib.Culture;
using Suzunoya;
using Suzunoya.Dialogue;
using UnityEngine;

namespace SuzunoyaUnity.Derived {
public class JointCharacter : SZYUCharacter {
    public override bool MimicRequested => false;
    public readonly SZYUCharacter[] parts;
    public override Color TextColor => parts[0].TextColor;
    public override Color UIColor => parts[0].UIColor;
    public override Sprite? ADVSpeakerIcon => parts[0].ADVSpeakerIcon;
    public override SpeechSettings SpeechCfg => parts[0].SpeechCfg;
    public override LString Name => string.Join(" & ", parts.Select(p => p.Name));

    public JointCharacter(params SZYUCharacter[] parts) {
        this.parts = parts;
        Container = parts[0].Container;
    }
}
}