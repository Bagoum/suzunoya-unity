using System;
using System.Collections;
using System.Collections.Generic;
using BagoumLib;
using SuzunoyaUnity;
using UnityEngine;

namespace SZYU.Examples {
public class ExampleVNUpdater : MonoBehaviour {
    public VNWrapper wrapper = null!;
    private readonly List<IDisposable> tokens = new();
    
    private void Awake() {
        //Redirect logs from the libraries to Debug.Log
        tokens.Add(Logging.Logs.RegisterListener(new TrivialLogListener(lm => {
            if (lm.Exception != null)
                Debug.LogException(lm.Exception);
            else
                Debug.Log(lm.Message);
        })));
        //Make VNWrapper accessible to other scripts
        tokens.Add(ServiceLocator.Register<IVNWrapper>(wrapper));
    }

    void Update() {
        //If the user is pressing Z, send a "confirm" to the underlying VN
        wrapper.DoUpdate(Time.deltaTime, Input.GetKeyDown(KeyCode.Z), false);
    }

    private void OnDisable() {
        tokens.DisposeAll();
    }
}
}