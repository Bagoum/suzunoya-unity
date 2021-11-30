using System;
using BagoumLib;
using Suzunoya.Entities;
using SuzunoyaUnity.Derived;
using SuzunoyaUnity.Events;
using UnityEngine;

namespace SuzunoyaUnity.Mimics {
public abstract class CharacterMimic : RenderedMimic {
    public Sprite? m_ADVSpeakerIcon;
    public virtual Sprite? ADVSpeakerIcon => m_ADVSpeakerIcon;
    
    private Character entity = null!;
    private CharacterSpeakingDisturbance speakDisturb = null!;

    
    public override void _Initialize(IEntity ent) => Initialize((ent as SZYUCharacter)!);
    private void Initialize(SZYUCharacter c) {
        base.Initialize(entity = c);
        c.Bind(this);
        speakDisturb = new CharacterSpeakingDisturbance(this, c);
        
        Listen(entity.Emote, SetEmote);
    }

    protected override void DoUpdate(float dT) {
        speakDisturb.DoUpdate(dT);
    }

    protected abstract void SetEmote(string? emote);
}

public abstract class SpriteIconCharacterMimic : CharacterMimic {
    private Sprite? lastADVIcon = null;
    public override Sprite? ADVSpeakerIcon => (lastADVIcon == null) ? base.ADVSpeakerIcon : lastADVIcon;
    
    public EmoteVariant[] advIcons = Array.Empty<EmoteVariant>();

    protected override void SetEmote(string? emote) {
        if (advIcons.Length > 0)
            lastADVIcon = SuzunoyaUnity.Helpers.FindSprite(emote, advIcons).Try(out var s) ? s : lastADVIcon;
    }
}
}