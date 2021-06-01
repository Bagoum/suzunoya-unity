using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuzunoyaUnity {
public class Tokenized : MonoBehaviour {
    protected readonly List<IDisposable> tokens = new List<IDisposable>();

    protected void Listen<T>(IObservable<T> obs, Action<T> listener) =>
        tokens.Add(obs.Subscribe(listener));
    
    private void OnDestroy() {
        foreach (var t in tokens)
            t.Dispose();
        tokens.Clear();
    }
}
}