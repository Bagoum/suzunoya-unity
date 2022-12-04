using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using BagoumLib.Culture;
using SuzunoyaUnity.Derived;
using SuzunoyaUnity.Mimics;

namespace SZYU.Examples {
//This is a pure data class. It is NOT a MonoBehavior or a GameObject
public class ExampleCharacter : SZYUCharacter {
    public override Color TextColor => new(1f, 0.9f, 0.9f);
    public override Color UIColor => new(.7f, .4f, .4f);
    public override LString Name { get; set; } = "Alice";

    public override void RollEvent() { }
    //If you have some service that allows making sound effects, you could call it here
    // to make a sound effect for dialogue
    //ISFXService.SFXService.Request("x-bubble-2", SFXType.TypingSound);
}

//Mimic classes are MonoBehaviors.
public class ExampleCharacterMimic : SpriteCharacterMimic {
    public override Type[] CoreTypes => new[] { typeof(ExampleCharacter) };
}

}