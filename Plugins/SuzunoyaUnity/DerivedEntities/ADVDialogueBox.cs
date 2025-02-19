using System.Collections;
using System.Collections.Generic;
using BagoumLib.Events;
using Suzunoya.Entities;
using UnityEngine;

public class ADVDialogueBox : DialogueBox {
    /// <summary>
    /// True if the dialogue box should have a minimal amount of interactability (ie. no menu buttons or the like).
    /// </summary>
    public OverrideEvented<bool> MinimalState { get; } = new(false);
    /// <summary>
    /// True if the dialogue box can be interacted with.
    /// </summary>
    public Evented<bool> Active { get; } = new(true);
}