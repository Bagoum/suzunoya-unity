using System.Collections.Generic;
using BagoumLib;
using SuzunoyaUnity;
using UnityEditor;
using UnityEngine;

namespace SuzunoyaUnity.Components {
public abstract class PiecewiseRender : MonoBehaviour {
    public string ident = "";
    public abstract void SetEmote(string? key);
    public abstract void SetVisible(bool visible);
    public abstract void SetTint(Color c);
}

public class PiecewiseSpriteRender : PiecewiseRender {
    public SpriteRenderer sr = null!;
    public Vector2 offsetPx;
    
    public EmoteVariant[] emotes = null!;
    private readonly Dictionary<string, Sprite> emoteMap = new Dictionary<string, Sprite>();
    
    private void Awake() {
        Relocate();
        for(int ii = 0; ii < emotes.Length; ++ii) {
            emotes[ii].emote = emotes[ii].emote.ToLower();
            emoteMap[emotes[ii].emote] = emotes[ii].sprite;
        }
    }

    [ContextMenu("Relocate")]
    private void Relocate() {
#if UNITY_EDITOR
        Undo.RecordObject(this, "Update sprite positioning");
#endif
        transform.localPosition = offsetPx * (1 / sr.sprite.pixelsPerUnit);
    #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
    #endif
    }


    public override void SetEmote(string? emote) {
        sr.sprite = Helpers.FindSprite(emote, emotes, emoteMap).Try(out var s) ? s : sr.sprite;
    }

    public override void SetVisible(bool visible) => sr.enabled = visible;

    public override void SetTint(Color c) => sr.color = c;
}
}