using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BagoumLib;
using BagoumLib.DataStructures;
using BagoumLib.Events;
using BagoumLib.Mathematics;
using Suzunoya.ADV;
using Suzunoya.ControlFlow;
using Suzunoya.Dialogue;
using Suzunoya.Entities;
using SuzunoyaUnity;
using SuzunoyaUnity.Derived;
using SuzunoyaUnity.Mimics;
using SuzunoyaUnity.Rendering;
using SuzunoyaUnity.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ADVDialogueBoxMimic : RenderedMimic, IPointerClickHandler, IScrollHandler {
    private struct LoadingChar {
        public float maxTime { get; }
        private float t;
        public LoadingChar(float maxTime) {
            this.maxTime = maxTime;
            this.t = 0;
        }

        /// <summary>
        /// </summary>
        /// <param name="dT"></param>
        /// <returns>True if the opacity changed, and if it did change, a byte indicating the new opacity.</returns>
        public (bool, byte) DoUpdateAsByte(float dT) {
            if (t > maxTime) return (false, default);
            t += dT;
            return (true, (t / maxTime).ToByte());
        }

        public (bool, float) DoUpdate(float dT) {
            if (t > maxTime) return (false, default);
            t += dT;
            return (true, Mathf.Clamp01(t / maxTime));
        }
    }
    
    
    public override Type[] CoreTypes => new[] {typeof(ADVDialogueBox)};

    public Canvas canvas = null!;
    public CanvasGroup cGroup = null!;
    public GraphicRaycaster raycaster = null!;
    public TMP_Text speaker = null!;
    public RubyTextMeshProUGUI mainText = null!;

    public GameObject speakerContainer = null!;
    public Image speakerIcon = null!;
    public Image nextOkIcon = null!;
    public DialogueBoxButton[] buttons = null!;

    public DialogueBoxButton? pauseButton;
    public DialogueBoxButton? autoplayButton;
    public DialogueBoxButton? skipButton;

    private readonly PushLerper<Color> textColor = new(0.1f, (a, b, t) => 
        Color.Lerp(a, b, Easers.EOutSine(t)));
    private const float nextOkLerpTime = 0.5f;
    private readonly PushLerperF<float> nextOkAlpha = new(nextOkLerpTime, Mathf.Lerp);
    private readonly DisturbedAnd raycastable = new();

    public float charLoadTime = 0.15f;
    public float charScaleInTime = 0.3f;
    public float charScaleInFrom = 1.2f;
    /// <summary>
    /// Time that must pass between successive scroll events being read.
    /// </summary>
    public float scrollWaitTime = 0.2f;
        
    protected ADVDialogueBox bound = null!;
    private readonly List<byte> charOpacity = new();
    private readonly List<LoadingChar> loadingOpacity = new();
    private readonly List<TMPAlphaController.Vertices> initialVertexPositions = new();
    private readonly List<float> charScaleIn = new();
    private readonly List<LoadingChar> loadingScaleIn = new();

    private float elapsedScrollWait = 0f;

    public override string SortingLayerFromPrefab => canvas.sortingLayerName;

    public override void _Initialize(IEntity ent) => Initialize((ent as ADVDialogueBox)!);

    private void UpdateCharProps(bool initial) {
        if (initial) {
            TMPAlphaController.GetVertices(mainText, initialVertexPositions);
        } else {
            TMPAlphaController.UpdateVertices(mainText, initialVertexPositions, charScaleIn, charScaleInFrom);
        }
        TMPAlphaController.ModifyAlphas(mainText, charOpacity, 0);
    }

    private void SetTextColor(Color c) {
        speaker.color = mainText.color = c;
        //Assigning mainText.color resets the alpha vertex overrides and triggers a lazy mesh redraw,
        // so we have to reapply alphas like this...
        mainText.onRebuild.Add(() => UpdateCharProps(false));
    }

    private void ClearText() {
        charOpacity.Clear();
        loadingOpacity.Clear();
        charScaleIn.Clear();
        loadingScaleIn.Clear();
        mainText.UnditedText = "";
        TMPAlphaController.ModifyAlphas(mainText, charOpacity, 0);
        didCharPropUpdate = false;
    }

    private void CheckCharProps() {
        if (didCharPropUpdate)
            UpdateCharProps(false);
        didCharPropUpdate = false;
    }

    private bool didCharPropUpdate = false;
    protected override void DoUpdate(float dT) {
        textColor.Update(dT);
        nextOkAlpha.Update(dT);
        for (int ii = 0; ii < buttons.Length; ++ii)
            buttons[ii].DoUpdate(dT);
        for (int ii = 0; ii < loadingOpacity.Count; ++ii) {
            var c = loadingOpacity[ii];
            var (upd, byt) = c.DoUpdateAsByte(dT);
            loadingOpacity[ii] = c;
            if (upd) {
                didCharPropUpdate = true;
                charOpacity[ii] = byt;
            }
        }
        for (int ii = 0; ii < loadingScaleIn.Count; ++ii) {
            var c = loadingScaleIn[ii];
            var (upd, fl) = c.DoUpdate(dT);
            loadingScaleIn[ii] = c;
            if (upd) {
                didCharPropUpdate = true;
                charScaleIn[ii] = fl;
            }
        }
        elapsedScrollWait += dT;
    }

    private void Update() {
        CheckCharProps();
    }

    private IDisposable? rgToken;
    public virtual void Initialize(ADVDialogueBox db) {
        base.Initialize(bound = db);
        db.Bind(this);

        raycastable.AddDisturbance(db.Container.InputAllowed);
        Listen(db.RenderGroup, rg => {
            rgToken?.Dispose();
            rgToken = null;
            if (rg != null) {
                rgToken = raycastable.AddDisturbance(rg.Visible);
            }
            if (rg is UnityRenderGroup urg) {
                canvas.worldCamera = urg.Camera;
            }
        });
        Listen(db.Speaker, obj => {
            bool anon = obj.flags.HasFlag(SpeakFlags.Anonymous);
            if (obj.speaker != null) {
                speakerContainer.SetActive(true);
                speaker.text = anon ? "???" : obj.speaker.Name;
                if (obj.speaker is SZYUCharacter sc) {
                    // ReSharper disable once AssignmentInConditionalExpression
                    if (speakerIcon.enabled = (sc.ADVSpeakerIcon != null && !anon))
                        speakerIcon.sprite = sc.ADVSpeakerIcon!;
                    //uiColor.Push(sc.UIColor);
                    //nextOkColor.Push(sc.UIColor, -nextOkLerpTime + 0.1f);
                    textColor.Push(sc.TextColor);
                }
            }
            if (obj.speaker == null) {
                speakerIcon.enabled = false;
                speakerContainer.SetActive(false);
                //uiColor.Unset();
                //nextOkColor.Unset();
                textColor.Unset();
                SetTextColor(Color.white);
            }
        });
        ClearText();
        Listen(db.DialogueCleared, _ => ClearText());
        Listen(db.DialogueStarted, op => {
            var sb = new StringBuilder();
            foreach (var frag in op.Line.Fragments) {
                sb.Append(frag switch {
                    SpeechFragment.Char c => c.fragment,
                    SpeechFragment.TagOpen to => TagToOpen(to.tag),
                    SpeechFragment.TagClose tc => TagToClose(tc.opener.tag),
                    _ => null
                });
            }
            mainText.UnditedText += sb.ToString();
            UpdateCharProps(true);
            didCharPropUpdate = true;
        });
        Listen(db.Dialogue, obj => {
            void AddC() {
                charOpacity.Add(0);
                loadingOpacity.Add(new(charLoadTime));
                charScaleIn.Add(0);
                loadingScaleIn.Add(new(charScaleInTime));
            }
            if (obj.frag is SpeechFragment.Char c)
                AddC();
            else if (obj.frag is SpeechFragment.TagClose { opener: { tag: SpeechTag.Furigana f } }) {
                //We load opacity for ruby characters at the end since RubyTMP sticks the characters
                // at the end, and it's difficult to deal with matching loadingChars indices otherwise.
                foreach (var _ in f.furigana)
                    AddC();
            } else if (obj.frag is SpeechFragment.RollEvent re)
                re.ev();
        });
        //dialogue finished effect?
        Listen(db.Container.AwaitingConfirm, icr => {
            if (icr == null)
                //https://github.com/dotnet/roslyn/issues/5835
                nextOkAlpha.Push(x => NextOkDisable(x), 0.1f);
            else
                nextOkAlpha.Push(x => NextOkEnable(x));
        });

        Listen(nextOkAlpha, f => nextOkIcon.color = nextOkIcon.color.WithA(f));
        Listen(textColor, SetTextColor);
        Listen(raycastable, v => raycaster.enabled = v);
        if (ServiceLocator.FindOrNull<ADVManager>() is { } advMan)
            tokens.Add(bound.MinimalState.AddDisturbance(advMan.VNExecutionStarting));
        
        if (!bound.Container.AutoplayFastforwardAllowed) {
            if (autoplayButton != null)
                autoplayButton.DisableButton();
            if (skipButton != null)
                skipButton.DisableButton();
        }
    }
    
    public virtual void Pause() => bound.Container.PauseGameplay();
    
    public virtual void Autoplay() {
        if (bound.Container.SkippingMode == null && bound.Container.AutoplayFastforwardAllowed)
            bound.Container.SetSkipMode(SkipMode.AUTOPLAY);
        else
            bound.Container.SetSkipMode(null);
    }
    
    public virtual void Skip() {
        if (bound.Container.SkippingMode == null && bound.Container.AutoplayFastforwardAllowed)
            bound.Container.SetSkipMode(SkipMode.FASTFORWARD);
        else
            bound.Container.SetSkipMode(null);
    }

    public virtual void OpenLog() => bound.Container.OpenLog();

    private static float NextOkDisable(float t) => 0;
    private static float NextOkEnable(float t) => .8f + 0.15f * Mathf.Sin(-3f * t);


    protected override void SetSortingLayer(int layer) => canvas.sortingLayerID = layer;

    protected override void SetSortingID(int id) => canvas.sortingOrder = id;

    protected override void SetVisible(bool visible) => canvas.enabled = visible;

    protected override void SetTint(Color c) => cGroup.alpha = c.a;


    private static string? TagToOpen(SpeechTag t) => t switch {
        SpeechTag.Color c => $"<color={c.color}>",
        SpeechTag.Furigana r => $"<ruby={r.furigana}>",
        SpeechTag.Unknown u => u.content == null ? $"<{u.name}>" : $"<{u.name}={u.content}>",
        _ => null
    };

    private static string? TagToClose(SpeechTag t) => t switch {
        SpeechTag.Color _ => "</color>",
        SpeechTag.Furigana _ => "</ruby>",
        SpeechTag.Unknown u => $"</{u.name}>",
        _ => null
    };

    protected override void OnDisable() {
        rgToken?.Dispose();
        base.OnDisable();
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (!bound.Active) return;
        if (eventData.button == PointerEventData.InputButton.Left)
            ((UnityVNState)bound.Container).ClickConfirmOrSkip();
        else
            Pause();
    }

    public void OnScroll(PointerEventData ev) {
        if (!bound.Active) return;
        if (ev.scrollDelta.y < 0 && elapsedScrollWait > scrollWaitTime) {
            if (((UnityVNState)bound.Container).ClickConfirmOrSkip())
                elapsedScrollWait = 0;
        }
    }
}