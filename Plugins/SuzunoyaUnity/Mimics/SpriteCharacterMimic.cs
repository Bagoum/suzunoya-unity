using System;
using System.Collections.Generic;
using BagoumLib.DataStructures;
using Suzunoya.Entities;
using SuzunoyaUnity.Derived;
using SuzunoyaUnity.Rendering;
using UnityEngine;
using UnityEngine.UI;
using Transform = UnityEngine.Transform;

namespace SuzunoyaUnity.Mimics {
public abstract class CharacterMimic : BaseMimic {
    public Sprite? ADVSpeakerIcon;
}
public class SpriteCharacterMimic : CharacterMimic {
    [Serializable]
    public struct EmoteVariant {
        public string emote;
        public Sprite sprite;
    }

    private Character chr = null!;
    public Transform tr { get; private set; } = null!;
    public SpriteRenderer sr = null!;
    public EmoteVariant[] emotes = null!;

    private Dictionary<string, Sprite> emoteMap = new Dictionary<string, Sprite>();

    private void Awake() {
        tr = transform;
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

    private void SetEmote(string? key) {
        sr.sprite = GetEmote(key);
    }

    
    public override void _Initialize(IEntity entity) => Initialize((entity as SZYUCharacter)!);
    public void Initialize(SZYUCharacter c) {
        chr = c;
        //Note this is a pretty trivial bind
        c.Bind(this);
        
        Listen(chr.Emote, SetEmote);
        
        Listen(chr.Location, v3 => tr.localPosition = v3._());
        Listen(chr.EulerAnglesD, v3 => tr.localEulerAngles = v3._());
        Listen(chr.Scale, v3 => tr.localScale = v3._());

        Listen(chr.RenderGroup, rg => {
            if (rg is UnityRenderGroup urg)
                gameObject.SetLayerRecursively(urg.LayerId);
        });
        Listen(chr.RenderLayer, l => sr.sortingLayerID = l);
        Listen(chr.SortingID, id => sr.sortingOrder = id);
        Listen(chr.Visible, b => sr.enabled = b);
        Listen(chr.Tint, t => sr.color = t._());
        Listen(chr.EntityActive, b => {
            if (!b) {
                Destroy(gameObject);
            }
        });
    }
}
}