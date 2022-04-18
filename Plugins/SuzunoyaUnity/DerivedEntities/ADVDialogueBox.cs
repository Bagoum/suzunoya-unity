using System.Collections;
using System.Collections.Generic;
using BagoumLib.Events;
using Suzunoya.Entities;
using UnityEngine;

public class ADVDialogueBox : DialogueBox {
    /// <summary>
    /// True if the dialogue box should have a minimal amount of interactability (ie. no menu buttons or the like).
    /// </summary>
    public Evented<bool> MinimalState { get; } = new(false);
    public ADVDialogueBoxMimic Mimic { get; private set; } = null!;

    public void Bind(ADVDialogueBoxMimic mimic) {
        Mimic = mimic;
    }
}