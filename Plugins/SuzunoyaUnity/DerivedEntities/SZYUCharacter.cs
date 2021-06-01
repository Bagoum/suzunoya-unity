using Suzunoya.Dialogue;
using Suzunoya.Entities;
using SuzunoyaUnity.Mimics;
using SuzunoyaUnity.Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace SuzunoyaUnity.Derived {

public class SZYUCharacter : Character {
    public virtual Color TextColor => Color.white;
    public virtual Color UIColor => new Color(0.85f, 0.1f, 0.24f);
    public Sprite? ADVSpeakerIcon => mimic.ADVSpeakerIcon;

    public override SpeechSettings SpeechCfg => new SpeechSettings(40, SpeechSettings.DefaultOpsPerChar, 8,
        SpeechSettings.DefaultRollEventAllowed, null);

    private CharacterMimic mimic = null!;

    public void Bind(CharacterMimic mimic_) {
        mimic = mimic_;
    }
}

}