using System;
using System.Collections.Generic;
using System.Linq;
using SuzunoyaUnity;
using SuzunoyaUnity.Components;
using SuzunoyaUnity.Mimics;
using UnityEngine;
using UnityEngine.Rendering;

namespace SuzunoyaUnity.Mimics {

public class PiecewiseCharacterMimic : SpriteIconCharacterMimic {
    public PiecewiseRender[] pieces = null!;
    /// <summary>
    /// This should also appear in the array.
    /// </summary>
    public PiecewiseRender defaultPiece = null!;
    private readonly Dictionary<string, PiecewiseRender> pieceMap = new Dictionary<string, PiecewiseRender>();

    public override string SortingLayerFromPrefab => sg.sortingLayerName;
    private SortingGroup sg = null!;

    protected override void Awake() {
        base.Awake();
        sg = GetComponent<SortingGroup>();
        for (int ii = 0; ii < pieces.Length; ++ii)
            pieceMap[pieces[ii].ident.ToLower()] = pieces[ii];
    }

    protected override void SetEmote(string? emote) {
        if (string.IsNullOrEmpty(emote)) {
            //Send default emote to all pieces
            base.SetEmote(emote);
            for (int ii = 0; ii < pieces.Length; ++ii)
                pieces[ii].SetEmote(null);
        } else if (emote!.IndexOf(':') > -1) {
            //Address a specific piece
            //Don't update the speaker emote in this case
            var ind = emote.IndexOf(':');
            var target = emote.Substring(0, ind);
            emote = emote.Substring(ind + 1);
            if (!pieceMap.TryGetValue(target.ToLower(), out var piece))
                throw new Exception($"Couldn't find character piece by key {target}");
            piece.SetEmote(emote);
        } else {
            //Address the default piece
            base.SetEmote(emote);
            defaultPiece.SetEmote(emote);
        }
    }

    protected override void SetSortingLayer(int layer) {
        sg.sortingLayerID = layer;
    }

    protected override void SetSortingID(int id) {
        sg.sortingOrder = id;
    }

    protected override void SetVisible(bool visible) {
        for (int ii = 0; ii < pieces.Length; ++ii) {
            pieces[ii].SetVisible(visible);
        }
    }

    protected override void SetTint(Color c) {
        for (int ii = 0; ii < pieces.Length; ++ii) {
            pieces[ii].SetTint(c);
        }
    }


    [ContextMenu("Copy values from existing script")]
    private void CopyValuesFromExisting() {
        foreach (var c in GetComponents<PiecewiseCharacterMimic>()) {
            if (c != this) {
                m_ADVSpeakerIcon = c.m_ADVSpeakerIcon;
                advIcons = c.advIcons.ToArray();
                pieces = c.pieces.ToArray();
                defaultPiece = c.defaultPiece;
                break;
            }
        }
    }
    
    [ContextMenu("Copy icons into default piece")]
    private void CopyIconsIntoDefault() {
        (defaultPiece as PiecewiseSpriteRender).emotes = advIcons.ToArray();
    }

    
}
}