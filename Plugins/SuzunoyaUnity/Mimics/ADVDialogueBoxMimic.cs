using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BagoumLib;
using BagoumLib.DataStructures;
using BagoumLib.Events;
using BagoumLib.Mathematics;
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

public class ADVDialogueBoxMimic : RenderedMimic, IPointerClickHandler {
    private readonly struct CharOrString {
        public readonly char? c;
        public readonly string? s;
        public CharOrString(char? c, string? s) {
            this.c = c;
            this.s = s;
        }

        public static CharOrString Char(char c) => new CharOrString(c, null);
        public static CharOrString Str(string s) => new CharOrString(null, s);
        public static CharOrString? MaybeStr(string? s) => s == null ? null : (CharOrString?)new CharOrString(null, s);
    }
    private class LoadingChar {
        public readonly CharOrString cs;
        public float t;
        public LoadingChar(CharOrString cs, float t = 0) {
            this.cs = cs;
            this.t = 0;
        }

        public string Rendered(float maxTime) {
            float ratio = Easers.EOutSine(t / maxTime);
            if (cs.c.Try(out var c_)) {
                return t >= maxTime ? $"{c_}" : $"<alpha=#{ratio.ToByte():X2}>{c_}</color>";
            } else {
                return t >= maxTime ? cs.s! : $"<alpha=#{ratio.ToByte():X2}>{cs.s}</color>";
            }
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
    public Image[] recolorables = null!;
    public DialogueBoxButton[] buttons = null!;

    private readonly PushLerper<Color> uiColor = new PushLerper<Color>(.25f, Color.Lerp);
    private readonly PushLerper<Color> nextOkColor = new PushLerper<Color>(.1f, Color.Lerp);
    private readonly PushLerper<Color> textColor = new PushLerper<Color>(0.4f, Color.Lerp);
    private const float nextOkLerpTime = 0.5f;
    private readonly PushLerperF<float> nextOkAlpha = new PushLerperF<float>(nextOkLerpTime, Mathf.Lerp);
    private readonly DisturbedAnd raycastable = new DisturbedAnd(true);

    public float charLoadTime = 0.3f;
    private ADVDialogueBox bound = null!;
    private readonly Queue<(SpeechFragment frag, CharOrString text)> remainingText = new Queue<(SpeechFragment, CharOrString)>();
    private readonly StringBuilder accText = new StringBuilder();
    private readonly DMCompactingArray<LoadingChar> loadingChars = new DMCompactingArray<LoadingChar>();


    public override string SortingLayerFromPrefab => canvas.sortingLayerName;
    public override void _Initialize(IEntity entity) => Initialize((entity as ADVDialogueBox)!);

    private void SetUIColor(Color c) {
        for (int ii = 0; ii < recolorables.Length; ++ii)
            recolorables[ii].color = c.WithA(recolorables[ii].color.a);
        for (int ii = 0; ii < buttons.Length; ++ii)
            buttons[ii].recolor.Value = c;
    }
    private void SetTextColor(Color c) {
        speaker.color = mainText.color = c;
    }

    private void ClearText() {
        loadingChars.Empty();
        //openTags.Clear();
        accText.Clear();
        remainingText.Clear();
        //lastLookahead = "";
        mainText.UnditedText = "";
    }

    //private string lastLookahead = "";
    private void SetText(string? lookahead) {
        var rem = new StringBuilder();
        void AddCS(StringBuilder sb, CharOrString cs) {
            if (cs.c != null)
                sb.Append(cs.c.Value);
            if (cs.s != null)
                sb.Append(cs.s);
        }
        bool canSend = true;
        for (int ii = 0; ii < loadingChars.Count; ++ii) {
            if (loadingChars.ExistsAt(ii)) {
                var lc = loadingChars[ii];
                var ratio = lc.t / charLoadTime;
                if (ratio >= 1 && canSend) {
                    AddCS(accText, lc.cs);
                    loadingChars.Delete(ii);
                } else {
                    canSend = false;
                    rem.Append(loadingChars[ii].Rendered(charLoadTime));
                }
            }
        }
        loadingChars.Compact();
        rem.Append("<color=#00000000>");
        //rem.Append(lastLookahead = lookahead ?? lastLookahead);
        /*foreach (var t in openTags) {
            if (TagToClose(t).Try(out var s))
                rem.Append(s);
        }*/
        foreach (var after in remainingText) {
            AddCS(rem, after.Item2);
        }
        mainText.UnditedText = accText.ToString() + rem.ToString();
    }

    protected override void DoUpdate(float dT) {
        uiColor.Update(dT);
        nextOkColor.Update(dT);
        textColor.Update(dT);
        nextOkAlpha.Update(dT);
        for (int ii = 0; ii < loadingChars.Count; ++ii)
            loadingChars[ii].t += dT;
        for (int ii = 0; ii < buttons.Length; ++ii)
            buttons[ii].DoUpdate(dT);
    }

    private void Update() {
        if (loadingChars.Count > 0)
            SetText(null);
    }

    private IDisposable? rgToken;
    public void Initialize(ADVDialogueBox db) {
        bound = db;
        //As ADVDialogueBox is a trivial wrapper around DialogueBox, no bind is required.
        base.Initialize(db);


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
            remainingText.Clear();
            foreach (var frag in op.Line.Fragments) {
                var lc = frag switch {
                    SpeechFragment.Char c => CharOrString.Char(c.fragment),
                    SpeechFragment.TagOpen to => CharOrString.MaybeStr(TagToOpen(to.tag)),
                    SpeechFragment.TagClose tc => CharOrString.MaybeStr(TagToClose(tc.opener.tag)),
                    _ => null
                };
                if (lc != null) {
                    remainingText.Enqueue((frag, lc.Value));
                }
            }
        });
        Listen(db.Dialogue, obj => {
            if (obj.frag is SpeechFragment.Char || 
                (obj.frag is SpeechFragment.TagOpen to && TagToClose(to.tag) != null) ||
                (obj.frag is SpeechFragment.TagClose tc && TagToClose(tc.opener.tag) != null)) {
                if (remainingText.Peek().frag != obj.frag)
                    throw new Exception("Mismatched dialogue");
                loadingChars.Add(new LoadingChar(remainingText.Dequeue().text, obj.frag switch {
                    SpeechFragment.Char _ => 0f,
                    _ => 9999f
                }));
            } else if (obj.frag is SpeechFragment.RollEvent re) {
                re.ev();
            } else
                return;
            //lastLookahead = obj.lookahead;
        });
        //dialogue finished effect?
        Listen(db.Container.AwaitingConfirm, icr => {
            if (icr == null)
                nextOkAlpha.Push(NextOkDisable, 0.1f);
            else
                nextOkAlpha.Push(NextOkEnable);
        });
        
        uiColor.Push(new Color(0.6f, 0.14f, 0.18f));
        nextOkColor.Push(new Color(0.6f, 0.14f, 0.18f));

        Listen(nextOkAlpha, f => nextOkIcon.color = nextOkIcon.color.WithA(f));
        Listen(uiColor, SetUIColor);
        Listen(nextOkColor, c => nextOkIcon.color = c.WithA(nextOkIcon.color.a));
        Listen(textColor, SetTextColor);
        Listen(raycastable, v => raycaster.enabled = v);
    }

    public virtual void Pause() => bound.Container.PauseGameplay();

    public virtual void Skip() {
        if (bound.Container.UserSkipMode == Autoskip.None)
            bound.Container.SetAutoskip(Autoskip.All);
        else
            bound.Container.SetAutoskip(Autoskip.None);
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
        SpeechTag.Color c => "</color>",
        SpeechTag.Furigana r => "</ruby>",
        SpeechTag.Unknown u => $"</{u.name}>",
        _ => null
    };

    protected override void OnDisable() {
        rgToken?.Dispose();
        base.OnDisable();
    }

    public void OnPointerClick(PointerEventData eventData) => bound.Container.UserConfirm();
}