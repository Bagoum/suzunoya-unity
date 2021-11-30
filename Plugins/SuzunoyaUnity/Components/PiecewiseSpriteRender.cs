using System.Collections.Generic;
using BagoumLib;
using SuzunoyaUnity;
using UnityEngine;

namespace SuzunoyaUnity.Components {
public abstract class PiecewiseRender : MonoBehaviour {
    public string ident = "";
    public abstract string SortingLayerFromPrefab { get; }
    public abstract void SetEmote(string? key);
    public abstract void SetSortingLayer(int layer);
    public abstract void SetSortingID(int id);
    public abstract void SetVisible(bool visible);
    public abstract void SetTint(Color c);
}

public class PiecewiseSpriteRender : PiecewiseRender {
    public SpriteRenderer sr = null!;
    public Vector2 offsetPx;
    
    public EmoteVariant[] emotes = null!;
    private readonly Dictionary<string, Sprite> emoteMap = new Dictionary<string, Sprite>();
    public override string SortingLayerFromPrefab => sr.sortingLayerName;
    
    private void Awake() {
        transform.localPosition = offsetPx * (1 / sr.sprite.pixelsPerUnit);
        for(int ii = 0; ii < emotes.Length; ++ii) {
            emotes[ii].emote = emotes[ii].emote.ToLower();
            emoteMap[emotes[ii].emote] = emotes[ii].sprite;
        }
    }


    public override void SetEmote(string? emote) {
        sr.sprite = Helpers.FindSprite(emote, emotes, emoteMap).Try(out var s) ? s : sr.sprite;
    }

    public override void SetSortingLayer(int layer) => sr.sortingLayerID = layer;

    public override void SetSortingID(int id) => sr.sortingOrder = id;

    public override void SetVisible(bool visible) => sr.enabled = visible;

    public override void SetTint(Color c) => sr.color = c;
}
}