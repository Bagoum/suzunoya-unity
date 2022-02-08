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
/// Service that manages VNStates and listens to top-level VNState events to map them into the Unity world.
/// When this object is disabled, all managed VNStates should receive a DeleteAll.
/// </summary>
public interface IVNWrapper {
    ExecutingVN TrackVN(IVNState vn);
    IEnumerable<ExecutingVN> TrackedVNs { get; }
    public void UpdateAllVNSaves();
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
    public readonly AccEvent<DialogueLogEntry> backlog = new();
    public Action<VNLocation>? doBacklog = null;
    public bool Active { get; private set; }
        
    public ExecutingVN(IVNState vn) {
        this.vn = vn;
        this.tokens = new List<IDisposable>();
    }

    public void Log(DialogueOp op) {
        if (op.Flags.HasFlag(SpeakFlags.DontClearText) && backlog.Published.Count > 0)
            backlog.Published[^1].Extend(op);
        else
            backlog.OnNext(new DialogueLogEntry(op));
    }
    
    public void Destroy() {
        Active = false;
        foreach (var token in tokens)
            token.Dispose();
        vn.DeleteAll();
    }
}
public class VNWrapper : MonoBehaviour, IVNWrapper {

    public GameObject renderGroupMimic = null!;
    public GameObject[] entityMimics = null!;
    protected Transform tr { get; private set; } = null!;
    
    private readonly Dictionary<Type, GameObject> mimicTypeMap = new Dictionary<Type, GameObject>();
    private readonly DMCompactingArray<ExecutingVN> vns = new DMCompactingArray<ExecutingVN>();
    public IEnumerable<ExecutingVN> TrackedVNs => vns;

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
        evn.tokens.Add(vn.DialogueLog.Subscribe(evn.Log));
        //TODO AwaitingConfirm
        evn.tokens.Add(vn.Logs.Subscribe(Logging.Log));
        evn.tokens.Add(vn.VNStateActive.Subscribe(b => {
            if (!b)
                evn.Destroy();
        }));
        return evn;
    }

    
    

    public void DoUpdate(float dT, bool isConfirm, bool isFullSkip) {
        for (int ii = 0; ii < vns.Count; ++ii) {
            if (vns.ExistsAt(ii)) {
                var vn = vns[ii].vn;
                if (isConfirm) {
                    if (vn.AwaitingConfirm.Value != null)
                        vn.UserConfirm();
                    else
                        vn.RequestSkipOperation();
                } else if (isFullSkip)
                    vn.TryFullSkip();
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
        Logging.Log($"New VN entity {ent}");
        if (mimicTypeMap.TryGetValue(ent.GetType(), out var mimic))
            Instantiate(mimic, tr, false).GetComponent<BaseMimic>()._Initialize(ent);
        else
            NoHandling(ent);
    }

    private void NoHandling(IEntity ent) {
        if (ent.MimicRequested)
            Logging.Log(LogMessage.Error(new Exception($"Couldn't handle entity {ent} of type {ent.GetType()}")));
    }

    public void UpdateAllVNSaves() {
        foreach (var ev in vns)
            ev.vn.UpdateSavedata();
    }

    private void OnDisable() {
        for (int ii = 0; ii < vns.Count; ++ii) {
            if (vns.ExistsAt(ii))
                vns[ii].Destroy();
        }
        vns.Empty();
    }

    [ContextMenu("Print location")]
    public void PrintLocation() {
        foreach (var evn in vns) {
            Logging.Log($"VN {evn.vn} is at location {VNLocation.Make(evn.vn)}");
        }
    }
}
}