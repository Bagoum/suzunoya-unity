using System;
using BagoumLib;
using BagoumLib.Events;
using BagoumLib.Mathematics;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SuzunoyaUnity.UI {
[Flags]
public enum ButtonState : int {
    Normal = 0,
    Hover = 1 << 0,
    Active = 1 << 1,
    Disabled = 1 << 2,
    Hide = 1 << 3,
    
    All = Hover | Active | Disabled | Hide
}
public class DialogueBoxButton : Tokenized, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler {
    public Image[] sprites = null!;
    public TextMeshProUGUI text = null!;
    
    public UnityEvent onClicked = null!;
    protected Evented<ButtonState> State { get; } = new(ButtonState.Normal);
    
    /// <summary>
    /// By default, a button is interactable if <see cref="ButtonState"/> does not contain DISABLED or HIDE.
    /// <br/>However, there may be other conditions in subclasses that cause buttons to be non-interactable.
    /// <br/>Such other conditions can be bound to this event.
    /// </summary>
    protected DisturbedAnd IsInteractable { get; } = new(true);

    protected void FastSetState(ButtonState state) {
        color.Unset();
        State.Value = state;
    }
    public void DisableButton() => State.Value |= ButtonState.Disabled;
    public void EnableButton() => State.Value &= ButtonState.All ^ ButtonState.Disabled;

    private readonly PushLerper<Color> color = 
        new(0.15f, (a, b, t) => Color.Lerp(a, b, Easers.EIOSine(t)));

    protected override void BindListeners() {
        base.BindListeners();
        AddToken(IsInteractable.AddDisturbance(State.Select(s => (s & (ButtonState.Disabled|ButtonState.Hide)) is 0)));
        AddToken(State.Subscribe(s => color.Push(new Color(1, 1, 1, StateToColor(s)))));
        AddToken(color.Subscribe(c => {
            for (int ii = 0; ii < sprites.Length; ++ii)
                sprites[ii].color = c;
            text.color = c;
        }));
    }
    public void DoUpdate(float dT) {
        color.Update(dT);
    }

    private static float StateToColor(ButtonState s) => s switch {
        { } when s.HasFlag(ButtonState.Hide) => 0f,
        { } when s.HasFlag(ButtonState.Disabled) => 0.4f,
        { } when s.HasFlag(ButtonState.Active) => 1f,
        { } when s.HasFlag(ButtonState.Hover) => 0.85f,
        _ => 0.56f
    };
    
    public void OnPointerEnter(PointerEventData eventData) {
        //Debug.Log($"enter {gameObject.name}");
        State.Value |= ButtonState.Hover;
    }

    public void OnPointerExit(PointerEventData eventData) => 
        State.Value &= (ButtonState.All ^ ButtonState.Hover ^ ButtonState.Active);

    public void OnPointerDown(PointerEventData eventData) => State.Value |= ButtonState.Active;

    public void OnPointerUp(PointerEventData eventData) => State.Value &= (ButtonState.All ^ ButtonState.Active);
    
    public void OnPointerClick(PointerEventData eventData) {
        //Debug.Log("clicked");
        onClicked.Invoke();
    }
}
}