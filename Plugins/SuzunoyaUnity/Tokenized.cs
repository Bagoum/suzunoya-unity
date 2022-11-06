using System;
using System.Collections.Generic;
using BagoumLib;
using UnityEngine;

namespace SuzunoyaUnity {
public class Tokenized : MonoBehaviour {
    protected readonly List<IDisposable> tokens = new List<IDisposable>();
    protected bool Enabled { get; private set; } = false;

    public void AddToken(IDisposable token) => tokens.Add(token);
    
    /// <summary>
    /// Safe to call twice.
    /// </summary>
    protected void EnableUpdates() {
        if (!Enabled) {
            BindListeners();
            Enabled = true;
        }
    }

    protected void DisableUpdates() {
        tokens.DisposeAll();
        Enabled = false;
    }
    
    protected virtual void BindListeners() { }
    
    public void Listen<T>(IObservable<T> obs, Action<T> listener) =>
        tokens.Add(obs.Subscribe(listener));


    protected virtual void OnEnable() => EnableUpdates();

    protected virtual void OnDisable() => DisableUpdates();
}
}