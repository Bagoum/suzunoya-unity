using System.Threading.Tasks;
using BagoumLib.Culture;
using Suzunoya;
using Suzunoya.ControlFlow;
using Suzunoya.Dialogue;
using Suzunoya.Entities;
using SuzunoyaUnity.Mimics;
using SuzunoyaUnity.Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace SuzunoyaUnity.Derived {

public class SZYUCharacter : Character {
    public virtual Color TextColor => Color.white;
    public virtual Color UIColor => new Color(0.6f, 0.6f, 0.6f);
    public virtual Sprite? ADVSpeakerIcon => mimic.ADVSpeakerIcon;

    public override SpeechSettings SpeechCfg => new SpeechSettings(90, SpeechSettings.DefaultOpsPerChar, 8,
        SpeechSettings.DefaultRollEventAllowed, Container.SkipGuard(RollEvent));

    private CharacterMimic mimic = null!;

    public void Bind(CharacterMimic mimic_) {
        mimic = mimic_;
    }

    public LazyAwaitable SetEmote(string emote) => new LazyAction(() => Emote.Value = emote);

    public VNOperation EmoteSay(string emote, LString content, SpeakFlags flags = SpeakFlags.Default) =>
        new VNOperation(Container, null, _ => {
            Emote.Value = emote;
            return Task.CompletedTask;
        }).Then(Say(content, null, flags));

    public VNConfirmTask EmoteSayC(string emote, LString content, SpeakFlags flags = SpeakFlags.Default) =>
        EmoteSay(emote, content, flags).C;

    public VNConfirmTask ESayC(string emote, LString content, SpeakFlags flags = SpeakFlags.Default) =>
        EmoteSayC(emote, content, flags);
    

    public virtual void RollEvent() { }
}

}