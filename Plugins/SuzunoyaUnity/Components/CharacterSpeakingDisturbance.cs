using System.Linq;
using BagoumLib.DataStructures;
using BagoumLib.Events;
using BagoumLib.Mathematics;
using SuzunoyaUnity.Derived;
using SuzunoyaUnity.Mimics;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace SuzunoyaUnity.Components {
public class CharacterSpeakingDisturbance {
    private readonly PushLerper<Vector3> heightOffset = 
        new PushLerper<Vector3>(0.3f, (a,b,t) => Vector3.Lerp(a, b, Easers.EOutSine(t)));
    private readonly PushLerper<FColor> tintMul = 
        new PushLerper<FColor>(0.3f, (a,b,t) => FColor.Lerp(a, b, Easers.EOutSine(t)));
    private readonly Evented<int> sortingOffset = new Evented<int>(0);
    public CharacterSpeakingDisturbance(CharacterMimic mimic) {
        if (mimic.Char.Container.MainDialogue == null) return;
        mimic.Listen(mimic.Char.Container.MainDialogueOrThrow.Speaker, spk => {
            if (spk.speaker == mimic.Char || spk.speaker is JointCharacter jc && jc.parts.Contains(mimic.Char)) {
                heightOffset.Push(new Vector3(0, 0.1f, 0));
                tintMul.Push(new FColor(1, 1, 1, 1));
                sortingOffset.Value = 100;
            } else {
                heightOffset.Push(new Vector3(0, -0.1f, 0));
                tintMul.Push(new FColor(0.8f, 0.8f, 0.8f, 1));
                sortingOffset.Value = 0;
            }
        });
        mimic.AddToken(mimic.Char.Location.AddDisturbance(heightOffset));
        mimic.AddToken(mimic.Char.Tint.AddDisturbance(tintMul));
        mimic.AddToken(mimic.Char.SortingID.AddDisturbance(sortingOffset));
    }

    public void DoUpdate(float dT) {
        heightOffset.Update(dT);
        tintMul.Update(dT);
    }
}
}