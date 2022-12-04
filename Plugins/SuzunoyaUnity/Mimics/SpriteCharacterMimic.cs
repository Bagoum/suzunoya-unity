using System;
using System.Collections.Generic;
using BagoumLib;
using BagoumLib.DataStructures;
using Suzunoya.Entities;
using SuzunoyaUnity.Components;
using SuzunoyaUnity.Derived;
using SuzunoyaUnity.Events;
using SuzunoyaUnity.Rendering;
using UnityEngine;
using UnityEngine.UI;
using Transform = UnityEngine.Transform;

namespace SuzunoyaUnity.Mimics {
public class SpriteCharacterMimic : SpriteIconCharacterMimic {
    public SpriteRenderer sr = null!;
    public EmoteVariant[] emotes = null!;

    private readonly Dictionary<string, Sprite> emoteMap = new Dictionary<string, Sprite>();

    public override string SortingLayerFromPrefab => sr.sortingLayerName;

    protected override void Awake() {
        base.Awake();
        for(int ii = 0; ii < emotes.Length; ++ii) {
            emotes[ii].emote = emotes[ii].emote.ToLower();
            emoteMap[emotes[ii].emote] = emotes[ii].sprite;
        }
    }

    protected override void SetEmote(string? emote) {
        base.SetEmote(emote);
        sr.sprite = SuzunoyaUnity.Helpers.FindSprite(emote, emotes, emoteMap).Try(out var s) ? s : sr.sprite;
    }

    protected override void SetSortingLayer(int layer) => sr.sortingLayerID = layer;

    protected override void SetSortingID(int id) => sr.sortingOrder = id;

    protected override void SetVisible(bool visible) => sr.enabled = visible;

    protected override void SetTint(Color c) => sr.color = c;
}
}