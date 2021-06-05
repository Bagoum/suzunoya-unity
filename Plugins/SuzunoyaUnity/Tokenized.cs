using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuzunoyaUnity {
public class Tokenized : MonoBehaviour {
    protected readonly List<IDisposable> tokens = new List<IDisposable>();
    protected bool Enabled { get; private set; } = false;
    
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
        for (int ii = 0; ii < tokens.Count; ++ii) {
            tokens[ii].Dispose();
        }
        tokens.Clear();
    }
    
    protected virtual void BindListeners() { }
    
    protected void Listen<T>(IObservable<T> obs, Action<T> listener) =>
        tokens.Add(obs.Subscribe(listener));


    protected virtual void OnEnable() => EnableUpdates();

    protected virtual void OnDisable() => DisableUpdates();
}
}