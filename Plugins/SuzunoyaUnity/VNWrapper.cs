using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BagoumLib;
using BagoumLib.Cancellation;
using BagoumLib.DataStructures;
using BagoumLib.Events;
using Suzunoya.ControlFlow;
using Suzunoya.Data;
using Suzunoya.Display;
using Suzunoya.Entities;
using SuzunoyaUnity.Derived;
using SuzunoyaUnity.Mimics;
using SuzunoyaUnity.Rendering;
using UnityEngine;
using Transform = UnityEngine.Transform;

namespace SuzunoyaUnity {
/// <summary>
/// Service that manages VNStates and listens to top-level VNState events.
/// When this object is disabled, all managed VNStates should receive a DeleteAll.
/// </summary>
public interface IVNWrapper {
    ExecutingVN TrackVN(IVNState vn);
}

public class DialogueLogEntry {
    public readonly Sprite? speakerSprite;
    public readonly string speakerName;
    public readonly VNLocation? location;
    //This may be updated by AlsoSay
    public string readableSpeech;
    public Color textColor = Color.white;
    public Color uiColor = new Color(0.6f, 0.6f, 0.6f);

    public DialogueLogEntry(DialogueOp op) {
        var anon = op.Flags.HasFlag(SpeakFlags.Anonymous);
        this.location = op.Location;
        this.speakerName = !anon ? (op.Speaker?.Name ?? "") : "???";
        this.readableSpeech = op.Line.Readable;
        speakerSprite = null;
        if (op.Speaker is SZYUCharacter ch) {
            if (!anon)
                speakerSprite = ch.ADVSpeakerIcon;
            textColor = ch.TextColor;
            uiColor = ch.UIColor;
        }
    }

    public void Extend(DialogueOp nxt) {
        readableSpeech += nxt.Line.Readable;
    }
}
public class ExecutingVN {
    public readonly IVNState vn;
    public readonly List<IDisposable> tokens;
    public readonly AccEvent<DialogueLogEntry> backlog = new AccEvent<DialogueLogEntry>();
    public Action<VNLocation>? doBacklog = null;
        
    public ExecutingVN(IVNState vn) {
        this.vn = vn;
        this.tokens = new List<IDisposable>();
    }

    public void Log(DialogueOp op) {
        if (op.Flags.HasFlag(SpeakFlags.DontClearText) && backlog.Published.Count > 0)
            backlog.Published[backlog.Published.Count - 1].Extend(op);
        else
            backlog.OnNext(new DialogueLogEntry(op));
    }
}
public class VNWrapper : MonoBehaviour, IInterrogatorReceiver, IVNWrapper {

    public GameObject renderGroupMimic = null!;
    public GameObject[] entityMimics = null!;
    protected Transform tr { get; private set; } = null!;
    
    private readonly Dictionary<Type, GameObject> mimicTypeMap = new Dictionary<Type, GameObject>();
    private readonly DMCompactingArray<ExecutingVN> vns = new DMCompactingArray<ExecutingVN>();

    protected virtual void Awake() {
        tr = transform;
        foreach (var go in entityMimics) {
            foreach (var t in go.GetComponent<BaseMimic>().CoreTypes)
                mimicTypeMap[t] = go;
        }
    }

    public virtual ExecutingVN TrackVN(IVNState vn) {
        var evn = new ExecutingVN(vn);
        evn.tokens.Add(vns.Add(evn));
        evn.tokens.Add(vn.RenderGroupCreated.Subscribe(NewRenderGroup));
        evn.tokens.Add(vn.EntityCreated.Subscribe(NewEntity));
        evn.tokens.Add(vn.InterrogatorCreated.Subscribe(this));
        evn.tokens.Add(vn.DialogueLog.Subscribe(evn.Log));
        //TODO AwaitingConfirm
        evn.tokens.Add(vn.Logs.Subscribe(Logging.Log));
        evn.tokens.Add(vn.VNStateActive.Subscribe(b => {
            if (!b)
                ClearVN(evn);
        }));
        return evn;
    }

    private static void ClearVN(ExecutingVN vn) {
        foreach (var token in vn.tokens)
            token.Dispose();
        vn.vn.DeleteAll();
    }
    

    public void DoUpdate(float dT, bool isConfirm, bool isSkip) {
        for (int ii = 0; ii < vns.Count; ++ii) {
            if (vns.ExistsAt(ii)) {
                var vn = vns[ii].vn;
                if (isConfirm)
                    vn.UserConfirm();
                else if (isSkip)
                    vn.RequestSkipOperation();
                if (vn.VNStateActive.Value)
                    vn.Update(dT);
            }
        }
        vns.Compact();
    }

    private void NewRenderGroup(RenderGroup rg) {
        Logging.Log($"New render group {rg.Key}");
        if (rg is UnityRenderGroup urg)
            Instantiate(renderGroupMimic, tr, false).GetComponent<RenderGroupMimic>().Initialize(urg);
        else
            NoHandling(rg);
        
    }
    
    private void NewEntity(IEntity ent) {
        Logging.Log($"New entity {ent}");
        if (mimicTypeMap.TryGetValue(ent.GetType(), out var mimic))
            Instantiate(mimic, tr, false).GetComponent<BaseMimic>()._Initialize(ent);
        else
            NoHandling(ent);
    }

    private void NoHandling(IEntity ent) {
        if (ent.MimicRequested)
            Logging.Log(LogMessage.Error(new Exception($"Couldn't handle entity {ent} of type {ent.GetType()}")));
    }

    public void OnNext<T>(IInterrogator<T> data) {
        if (data is ChoiceInterrogator<T> choices) {
            Debug.Log(string.Join(", ", choices.Choices.Select(x => $"{x.value}: {x.description}")));
        }
    }

    private void OnDisable() {
        for (int ii = 0; ii < vns.Count; ++ii) {
            if (vns.ExistsAt(ii))
                ClearVN(vns[ii]);
        }
        vns.Empty();
    }
}
}