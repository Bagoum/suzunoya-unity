using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using BagoumLib.Events;
using BagoumLib.Mathematics;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SuzunoyaUnity.UI {
[Flags]
public enum ButtonState {
    Normal = 0,
    Hover = 1 << 0,
    Active = 1 << 1,
    Disabled = 1 << 2,
    
    All = Hover | Active | Disabled
}
public class DialogueBoxButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler {
    public Image[] sprites = null!;
    public TextMeshProUGUI text = null!;
    
    public UnityEvent onClicked = null!;
    private ButtonState _state = ButtonState.Normal;
    private ButtonState State {
        get => _state;
        set => color.Push(new Color(1, 1, 1, StateToColor(_state = value)));
    }
    public void DisableButton() => State |= ButtonState.Disabled;
    public void EnableButton() => State &= ButtonState.All ^ ButtonState.Disabled;

    private readonly PushLerper<Color> color = 
        new(0.12f, (a, b, t) => Color.Lerp(a, b, Easers.EIOSine(t)));

    protected virtual void Awake() {
        color.Subscribe(c => {
            for (int ii = 0; ii < sprites.Length; ++ii)
                sprites[ii].color = c;
            text.color = c;
        });
        State = State;
    }
    public void DoUpdate(float dT) {
        color.Update(dT);
    }

    private static float StateToColor(ButtonState s) => s switch {
        { } when s.HasFlag(ButtonState.Disabled) => 0.4f,
        { } when s.HasFlag(ButtonState.Active) => 1f,
        { } when s.HasFlag(ButtonState.Hover) => 0.8f,
        _ => 0.6f
    };
    
    public void OnPointerEnter(PointerEventData eventData) {
        //Debug.Log($"enter {gameObject.name}");
        State |= ButtonState.Hover;
    }

    public void OnPointerExit(PointerEventData eventData) => State &= (ButtonState.All ^ ButtonState.Hover);

    public void OnPointerDown(PointerEventData eventData) => State |= ButtonState.Active;

    public void OnPointerUp(PointerEventData eventData) => State &= (ButtonState.All ^ ButtonState.Active);
    
    public void OnPointerClick(PointerEventData eventData) {
        //Debug.Log("clicked");
        onClicked.Invoke();
    }
}
}