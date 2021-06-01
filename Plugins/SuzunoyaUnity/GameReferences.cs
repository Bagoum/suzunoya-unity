using System.Collections.Generic;
using System.Linq;
using BagoumLib;
using UnityEngine;

namespace SuzunoyaUnity.Scriptables {
[CreateAssetMenu(menuName = "Data/Game Data")]
public class GameReferences : ScriptableObject {
    public PrefabReferences prefabs = null!;

}
}