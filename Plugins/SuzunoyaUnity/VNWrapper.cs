using System;
using System.Collections.Generic;
using System.Linq;
using BagoumLib.Cancellation;
using Suzunoya.ControlFlow;
using Suzunoya.Data;
using Suzunoya.Display;
using Suzunoya.Entities;
using SuzunoyaUnity.Mimics;
using SuzunoyaUnity.Rendering;
using UnityEngine;

namespace SuzunoyaUnity {
public class VNWrapper : MonoBehaviour, IInterrogatorReceiver {
    private IVNState? vn;
    private readonly List<IDisposable> vnTokens = new List<IDisposable>();

    public GameObject renderGroupMimic = null!;
    public GameObject[] entityMimics = null!;
    private readonly Dictionary<Type, GameObject> mimicTypeMap = new Dictionary<Type, GameObject>();

    private void Awake() {
        foreach (var go in entityMimics) {
            foreach (var t in go.GetComponent<BaseMimic>().CoreTypes)
                mimicTypeMap[t] = go;
        }
    }

    public IVNState TrackVN(IVNState newVn) {
        if (vn != null)
            throw new Exception("VN already exists");
        vn = newVn;
        vnTokens.Add(vn.RenderGroupCreated.Subscribe(NewRenderGroup));
        vnTokens.Add(vn.EntityCreated.Subscribe(NewEntity));
        vnTokens.Add(vn.InterrogatorCreated.Subscribe(this));
        //TODO AwaitingConfirm
        vnTokens.Add(vn.Logs.Subscribe(Log.Unity));
        vnTokens.Add(vn.VNStateActive.Subscribe(b => {
            if (!b)
                ClearVN();
        }));
        return vn;
    }

    public void ClearVN() {
        foreach (var token in vnTokens)
            token.Dispose();
        vnTokens.Clear();
        if (vn != null) {
            vn.DeleteAll();
            vn = null;
        }
        
    }
    

    public void DoUpdate(float dT, bool isConfirm, bool isSkip) {
        if (isConfirm)
            vn?.Confirm();
        else if (isSkip)
            vn?.RequestSkipOperation();
        vn?.Update(dT);
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
        ClearVN();
    }
}
}