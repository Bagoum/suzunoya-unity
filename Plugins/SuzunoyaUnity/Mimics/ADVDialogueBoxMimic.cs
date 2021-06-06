using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BagoumLib;
using BagoumLib.DataStructures;
using BagoumLib.Events;
using Suzunoya.Dialogue;
using Suzunoya.Entities;
using SuzunoyaUnity;
using SuzunoyaUnity.Derived;
using SuzunoyaUnity.Mimics;
using SuzunoyaUnity.Rendering;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ADVDialogueBoxMimic : RenderedMimic {
    private class LoadingChar {
        public readonly char? c;
        public readonly string? s;
        public float t;
        public LoadingChar(char? c, string? s, float t = 0) {
            this.c = c;
            this.s = s;
            this.t = 0;
        }

        public string Rendered(float maxTime) {
            if (c.Try(out var c_)) {
                return t >= maxTime ? $"{c_}" : $"<alpha=#{(t/maxTime).ToByte():X2}>{c_}</color>";
            } else {
                return t >= maxTime ? s! : $"<alpha=#{(t/maxTime).ToByte():X2}>{s}</color>";
            }
        }
    }
    public override Type[] CoreTypes => new[] {typeof(ADVDialogueBox)};

    public Canvas canvas = null!;
    public CanvasGroup cGroup = null!;
    public TMP_Text speaker = null!;
    public RubyTextMeshProUGUI mainText = null!;

    public GameObject speakerContainer = null!;
    public Image speakerIcon = null!;
    public Image[] recolorables = null!;

    private readonly PushLerper<Color> uiColor = new PushLerper<Color>(0.3f, Color.Lerp);
    private readonly PushLerper<Color> textColor = new PushLerper<Color>(0.3f, Color.Lerp);

    public float charLoadTime = 0.3f;
    private ADVDialogueBox bound = null!;
    private readonly StringBuilder accText = new StringBuilder();
    private readonly DMCompactingArray<LoadingChar> loadingChars = new DMCompactingArray<LoadingChar>();
    private readonly List<SpeechTag> openTags = new List<SpeechTag>();
    

    public override void _Initialize(IEntity entity) => Initialize((entity as ADVDialogueBox)!);

    private void SetUIColor(Color c) {
        for (int ii = 0; ii < recolorables.Length; ++ii)
            recolorables[ii].color = c.WithA(recolorables[ii].color.a);
    }
    private void SetTextColor(Color c) {
        speaker.color = mainText.color = c;
    }

    private void ClearText() {
        loadingChars.Empty();
        openTags.Clear();
        accText.Clear();
        lastLookahead = "";
        mainText.UnditedText = "";
    }

    private string lastLookahead = "";
    private void SetText(string? lookahead) {
        var rem = new StringBuilder();
        bool canSend = true;
        for (int ii = 0; ii < loadingChars.Count; ++ii) {
            if (loadingChars.ExistsAt(ii)) {
                var ratio = loadingChars[ii].t / charLoadTime;
                if (ratio >= 1 && canSend) {
                    accText.Append(loadingChars[ii].c);
                    accText.Append(loadingChars[ii].s);
                    loadingChars.Delete(ii);
                } else {
                    canSend = false;
                    rem.Append(loadingChars[ii].Rendered(charLoadTime));
                }
            }
        }
        loadingChars.Compact();
        rem.Append("<color=#00000000>");
        rem.Append(lastLookahead = lookahead ?? lastLookahead);
        foreach (var t in openTags) {
            if (TagToClose(t).Try(out var s))
                rem.Append(s);
        }
        mainText.UnditedText = accText.ToString() + rem.ToString();
    }

    protected override void DoUpdate(float dT) {
        uiColor.Update(dT);
        textColor.Update(dT);
        for (int ii = 0; ii < loadingChars.Count; ++ii)
            loadingChars[ii].t += dT;
    }

    private void Update() {
        if (loadingChars.Count > 0)
            SetText(null);
    }

    public void Initialize(ADVDialogueBox db) {
        bound = db;
        //As ADVDialogueBox is a trivial wrapper around DialogueBox, no bind is required.
        base.Initialize(db);
        
        Listen(db.Speaker, obj => {
            bool anon = obj.flags.HasFlag(SpeakFlags.Anonymous);
            if (obj.speaker != null) {
                speakerContainer.SetActive(true);
                speaker.text = anon ? "???" : obj.speaker.Name;
                if (obj.speaker is SZYUCharacter sc) {
                    // ReSharper disable once AssignmentInConditionalExpression
                    if (speakerIcon.enabled = (sc.ADVSpeakerIcon != null && !anon))
                        speakerIcon.sprite = sc.ADVSpeakerIcon!;
                    uiColor.Push(sc.UIColor);
                    textColor.Push(sc.TextColor);
                }
            }
            if (obj.speaker == null) {
                speakerIcon.enabled = false;
                speakerContainer.SetActive(false);
                SetTextColor(Color.white);
            }
        });
        ClearText();
        Listen(db.DialogueCleared, _ => ClearText());
        //dialogue started?
        Listen(db.Dialogue, obj => {
            if (obj.frag is SpeechFragment.Char c)
                loadingChars.Add(new LoadingChar(c.fragment, null));
            else if (obj.frag is SpeechFragment.TagOpen to) {
                loadingChars.Add(new LoadingChar(null, TagToOpen(to.tag), 9999f));
                openTags.Add(to.tag);
            } else if (obj.frag is SpeechFragment.TagClose tc) {
                loadingChars.Add(new LoadingChar(null, TagToClose(tc.opener.tag), 9999f));
                openTags.Remove(tc.opener.tag);
            } else if (obj.frag is SpeechFragment.RollEvent re) {
                re.ev();
            } else
                return;
            lastLookahead = obj.lookahead;
        });
        //dialogue finished effect?

        Listen(uiColor.OnChange, SetUIColor);
        Listen(textColor.OnChange, SetTextColor);
    }


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
}