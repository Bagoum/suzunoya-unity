using System;
using System.Collections.Generic;
using System.Linq;
using BagoumLib.Cancellation;
using BagoumLib.DataStructures;
using Suzunoya.ControlFlow;
using Suzunoya.Data;
using Suzunoya.Display;
using Suzunoya.Entities;
using SuzunoyaUnity.Mimics;
using SuzunoyaUnity.Rendering;
using UnityEngine;

namespace SuzunoyaUnity {
public class VNWrapper : MonoBehaviour, IInterrogatorReceiver {
    private readonly DMCompactingArray<IVNState> vns = new DMCompactingArray<IVNState>();
    private readonly Dictionary<IVNState, List<IDisposable>> vnTokens = new Dictionary<IVNState, List<IDisposable>>();

    public GameObject renderGroupMimic = null!;
    public GameObject[] entityMimics = null!;
    private readonly Dictionary<Type, GameObject> mimicTypeMap = new Dictionary<Type, GameObject>();

    private void Awake() {
        foreach (var go in entityMimics) {
            foreach (var t in go.GetComponent<BaseMimic>().CoreTypes)
                mimicTypeMap[t] = go;
        }
    }

    public IVNState TrackVN(IVNState vn) {
        var tokens = vnTokens[vn] = new List<IDisposable>();
        tokens.Add(vns.Add(vn));
        tokens.Add(vn.RenderGroupCreated.Subscribe(NewRenderGroup));
        tokens.Add(vn.EntityCreated.Subscribe(NewEntity));
        tokens.Add(vn.InterrogatorCreated.Subscribe(this));
        //TODO AwaitingConfirm
        tokens.Add(vn.Logs.Subscribe(Log.Unity));
        tokens.Add(vn.VNStateActive.Subscribe(b => {
            if (!b)
                ClearVN(vn);
        }));
        return vn;
    }

    public void ClearVN(IVNState vn) {
        foreach (var token in vnTokens[vn])
            token.Dispose();
        vnTokens.Remove(vn);
        vn.DeleteAll();
    }
    

    public void DoUpdate(float dT, bool isConfirm, bool isSkip) {
        for (int ii = 0; ii < vns.Count; ++ii) {
            if (vns.ExistsAt(ii)) {
                var vn = vns[ii];
                if (isConfirm)
                    vn.Confirm();
                else if (isSkip)
                    vn.RequestSkipOperation();
                if (vn.VNStateActive.Value)
                    vn.Update(dT);
            }
        }
        vns.Compact();
    }

    private void NewRenderGroup(RenderGroup rg) {
        Log.Unity($"New render group {rg.Key}");
        if (rg is UnityRenderGroup urg)
            Instantiate(renderGroupMimic).GetComponent<RenderGroupMimic>().Initialize(urg);
        else
            NoHandling(rg);
        
    }
    
    private void NewEntity(IEntity ent) {
        Log.Unity($"New entity {ent}");
        if (mimicTypeMap.TryGetValue(ent.GetType(), out var mimic))
            Instantiate(mimic).GetComponent<BaseMimic>()._Initialize(ent);
        else
            NoHandling(ent);
    }

    private void NoHandling(IEntity ent) {
        Log.UnityError($"Couldn't handle entity {ent} of type {ent.GetType()}");
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