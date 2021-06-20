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
    public DisturbedProduct<Color> recolor = null!;
    public Image[] recoloredSprites = null!;
    public Image[] sprites = null!;
    public TextMeshProUGUI text = null!;
    
    public UnityEvent onClicked = null!;
    private ButtonState _state = ButtonState.Normal;
    private ButtonState State {
        get => _state;
        set {
            var r = StateToColor(_state = value);
            color.Push(new Color(r, r, r, 1));
        }
    }

    private readonly PushLerper<Color> color = 
        new PushLerper<Color>(0.12f, (a, b, t) => Color.Lerp(a, b, Easers.EIOSine(t)));

    private void Awake() {
        recolor = new DisturbedProduct<Color>(Color.white);
        color.Subscribe(c => {
            for (int ii = 0; ii < sprites.Length; ++ii)
                sprites[ii].color = c;
            text.color = c;
        });
        State = State;
        recolor.AddDisturbance(color);
        recolor.Subscribe(c => {
            for (int ii = 0; ii < recoloredSprites.Length; ++ii)
                recoloredSprites[ii].color = c;
        });
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