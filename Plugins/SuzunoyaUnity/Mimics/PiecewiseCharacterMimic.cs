using System;
using System.Collections.Generic;
using SuzunoyaUnity;
using SuzunoyaUnity.Components;
using SuzunoyaUnity.Mimics;
using UnityEngine;

namespace SuzunoyaUnity.Mimics {

public class PiecewiseCharacterMimic : CharacterMimic {
    public PiecewiseRender[] pieces = null!;
    /// <summary>
    /// This should also appear in the array.
    /// </summary>
    public PiecewiseRender defaultPiece = null!;
    private readonly Dictionary<string, PiecewiseRender> pieceMap = new Dictionary<string, PiecewiseRender>();

    protected override void Awake() {
        base.Awake();
        for (int ii = 0; ii < pieces.Length; ++ii)
            pieceMap[pieces[ii].ident.ToLower()] = pieces[ii];
    }

    protected override void SetEmote(string? emote) {
        if (string.IsNullOrEmpty(emote)) {
            //Send default emote to all pieces
            for (int ii = 0; ii < pieces.Length; ++ii)
                pieces[ii].SetEmote(null);
        } else if (emote!.IndexOf(':') > -1) {
            //Address a specific piece
            var ind = emote.IndexOf(':');
            var target = emote.Substring(0, ind);
            emote = emote.Substring(ind + 1);
            if (!pieceMap.TryGetValue(target.ToLower(), out var piece))
                throw new Exception($"Couldn't find character piece by key {target}");
            piece.SetEmote(emote);
        } else {
            //Address the default piece
            defaultPiece.SetEmote(emote);
        }
    }

    protected override void SetSortingLayer(int layer) {
        for (int ii = 0; ii < pieces.Length; ++ii) {
            pieces[ii].SetSortingLayer(layer);
        }
    }

    protected override void SetSortingID(int id) {
        for (int ii = 0; ii < pieces.Length; ++ii) {
            pieces[ii].SetSortingID(id + ii);
        }
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
}
}