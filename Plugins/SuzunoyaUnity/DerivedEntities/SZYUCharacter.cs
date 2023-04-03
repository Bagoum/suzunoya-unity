using System.Numerics;
using System.Reactive;
using System.Threading.Tasks;
using BagoumLib.Culture;
using BagoumLib.Tasks;
using Suzunoya;
using Suzunoya.ControlFlow;
using Suzunoya.Dialogue;
using Suzunoya.Entities;
using SuzunoyaUnity.Mimics;
using SuzunoyaUnity.Rendering;
using UnityEngine;

namespace SuzunoyaUnity.Derived {

public class SZYUCharacter : Character {
    public virtual Color TextColor => Color.white;
    public virtual Color UIColor => new Color(0.6f, 0.6f, 0.6f);
    public virtual Sprite? ADVSpeakerIcon => Mimic == null ? null : Mimic.ADVSpeakerIcon;

    public override SpeechSettings SpeechCfg { get; }

    public CharacterMimic? Mimic { get; private set; }

    public SZYUCharacter() {
        SpeechCfg = new(90, SpeechSettings.DefaultOpsPerChar, 8,
            SpeechSettings.DefaultRollEventAllowed, RollEventIfNotSkip);
    }

    public void Bind(CharacterMimic mimic) {
        Mimic = mimic;
    }

    public LazyAction SetEmote(string emote) => new LazyAction(() => Emote.Value = emote);

    public VNOperation EmoteSay(string emote, LString content, SpeakFlags flags = SpeakFlags.Default) =>
        new VNOperation(Container, _ => {
            Emote.Value = emote;
            return Task.CompletedTask;
        }).Then(Say(content, null, flags));

    public VNConfirmTask EmoteSayC(string emote, LString content, SpeakFlags flags = SpeakFlags.Default) =>
        EmoteSay(emote, content, flags).C;

    public VNConfirmTask ESayC(string emote, LString content, SpeakFlags flags = SpeakFlags.Default) =>
        EmoteSayC(emote, content, flags);

    private void RollEventIfNotSkip() {
        if (!Container.SkippingMode.SkipsOperations())
            RollEvent();
    }

    /// <summary>
    /// When a character is speaking dialogue, this function is called every few frames to allow
    ///  constructing a sound effect for the dialogue.
    /// </summary>
    public virtual void RollEvent() { }
}

}