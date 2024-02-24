using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using BagoumLib;
using BagoumLib.Cancellation;
using BagoumLib.DataStructures;
using BagoumLib.Mathematics;
using BagoumLib.Tasks;
using Suzunoya;
using Suzunoya.ControlFlow;
using Suzunoya.Data;
using SuzunoyaUnity;
using SuzunoyaUnity.Derived;
using UnityEditor;
using UnityEngine;

namespace SZYU.Examples {
public class ExampleVNScript : MonoBehaviour {
    private VNState vn = null!;
    private readonly List<IDisposable> tokens = new();
    public TextAsset? loadFrom;
    public TextAsset saveTo = null!;

    void Start() {
        var lifetimeToken = new Cancellable();
        tokens.Add(lifetimeToken);
        //This contains settings and the like, you should save it somewhere persistent based on your game setup
        var globalData = new GlobalData();
        var instanceData = loadFrom == null ?
            new InstanceData(globalData) :
            InstanceData.Deserialize<InstanceData>(loadFrom.text, globalData);

        vn = new UnityVNState(lifetimeToken, instanceData);
        //Note that this is a naive form of save/load that does not work in some edge cases;
        // the generalized handling in the ADV utilities fixes this.
        if (instanceData.Location is { } loadTo)
            vn.LoadToLocation(loadTo, instanceData);

        //This allows the data objects to be mimicked on screen, and also allows them to receive update events
        ServiceLocator.Find<IVNWrapper>().TrackVN(vn);

        _ = RunSomeCode().Execute().ContinueWithSync();
    }

    [ContextMenu("Run Script")]
    public void RunScript() {
        _ = RunSomeCode().Execute().ContinueWithSync();
    }
    
    private BoundedContext<Unit> RunSomeCode() => new(vn, "exampleContent", async () => {
        using var alice = vn.Add(new ExampleCharacter());
        alice.LocalLocation.Value = new(-2, 0, 0);
        alice.Tint.Value = new(1f, 0.8f, 0.8f);
        using var dialogueBox = vn.Add(new ADVDialogueBox());
        await alice.Say("Hello world").C;
        //The player needs to press Z (as configured in ExampleVNUpdater.Update) or
        // click on the screen (as configured in ADVDialogueBoxMimic.OnPointerClick)
        // to advance a confirmation task (.C)
        await alice.EmoteSay("happy", "Foo bar").C;
        await alice.MoveBy(new(5, 0, 0), 2f, Easers.EOutSine);
        using var bob = vn.Add(new ExampleCharacter2());
        bob.LocalLocation.Value = new(-2, 0, 0);
        bob.Tint.Value = new(0.8f, 0.8f, 1f, 0f);
        await bob.FadeTo(1f, 1f).And(bob.Say("Lorem ipsum dolor sit amet")).C;
        return default;
    });

    private void OnDisable() {
        tokens.DisposeAll();
    }

#if UNITY_EDITOR
    [ContextMenu("Save")]
    public void Save() {
        File.WriteAllText(AssetDatabase.GetAssetPath(saveTo), Serialization.SerializeJson(vn.UpdateInstanceData()));
    }
#endif

}
}