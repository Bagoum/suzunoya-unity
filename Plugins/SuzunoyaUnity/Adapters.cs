using BagoumLib.DataStructures;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace SuzunoyaUnity {
public static class Adapters {
    public static Vector3 _(this UnityEngine.Vector3 v3) => new Vector3(v3.x, v3.y, v3.z);
    public static Vector2 _(this UnityEngine.Vector2 v2) => new Vector2(v2.x, v2.y);
    public static Color _(this FColor c) => new Color(c.r, c.g, c.b, c.a);
    
    public static UnityEngine.Vector3 _(this Vector3 v3) => new UnityEngine.Vector3(v3.X, v3.Y, v3.Z);
    public static UnityEngine.Vector2 _(this Vector2 v2) => new UnityEngine.Vector2(v2.X, v2.Y);
    public static FColor _(this Color c) => new FColor(c.r, c.g, c.b, c.a);

}
}