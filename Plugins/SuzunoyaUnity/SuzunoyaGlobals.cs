using System;
using SuzunoyaUnity.Scriptables;
using UnityEngine;

namespace SuzunoyaUnity {
public class SuzunoyaGlobals : MonoBehaviour {
    public static SuzunoyaGlobals Main { get; private set; } = null!;
    
    public GameReferences references = null!;

    public static GameReferences References => Main.references;
    public static PrefabReferences Prefabs => References.prefabs;

    private void Awake() {
        if (Main != null) {
            DestroyImmediate(gameObject);
            return;
        }
        Main = this;
    }
}
}