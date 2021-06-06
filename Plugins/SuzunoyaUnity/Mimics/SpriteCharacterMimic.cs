using System;
using System.Collections.Generic;
using BagoumLib.DataStructures;
using Suzunoya.Entities;
using SuzunoyaUnity.Components;
using SuzunoyaUnity.Derived;
using SuzunoyaUnity.Rendering;
using UnityEngine;
using UnityEngine.UI;
using Transform = UnityEngine.Transform;

namespace SuzunoyaUnity.Mimics {
public abstract class CharacterMimic : RenderedMimic {
    public Sprite? ADVSpeakerIcon;
    public Character Char { get; private set; } = null!;
    private CharacterSpeakingDisturbance speakDisturb = null!;

    
    public override void _Initialize(IEntity entity) => Initialize((entity as SZYUCharacter)!);
    private void Initialize(SZYUCharacter c) {
        Char = c;
        c.Bind(this);
        speakDisturb = new CharacterSpeakingDisturbance(this);
        
        base.Initialize(Char);

        Listen(Char.Emote, SetEmote);
    }

    protected override void DoUpdate(float dT) {
        speakDisturb.DoUpdate(dT);
    }

    protected abstract void SetEmote(string? emote);
}
public class SpriteCharacterMimic : CharacterMimic {
    public SpriteRenderer sr = null!;
    public EmoteVariant[] emotes = null!;

    private readonly Dictionary<string, Sprite> emoteMap = new Dictionary<string, Sprite>();

    protected override void Awake() {
        base.Awake();
        for(int ii = 0; ii < emotes.Length; ++ii) {
            emotes[ii].emote = emotes[ii].emote.ToLower();
            emoteMap[emotes[ii].emote] = emotes[ii].sprite;
        }
    }

    private Sprite GetEmote(string? key) {
        key = (key ?? emotes[0].emote).ToLower();
        if (emoteMap.TryGetValue(key, out var em))
            return em;
        foreach (var emote in emotes) {
            if (emote.emote.StartsWith(key))
                return emote.sprite;
        }
        return emotes[0].sprite;
    }

    protected override void SetEmote(string? emote) {
        sr.sprite = GetEmote(emote);
    }

    protected override void SetSortingLayer(int layer) => sr.sortingLayerID = layer;

    protected override void SetSortingID(int id) => sr.sortingOrder = id;

    protected override void SetVisible(bool visible) => sr.enabled = visible;

    protected override void SetTint(Color c) => sr.color = c;
}
}