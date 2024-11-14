using UnityEngine;

namespace SuzunoyaUnity.Rendering {
public static class PropConsts {
    public static readonly int RGTex = Shader.PropertyToID("_RGTex");
    public static readonly int RGTex2 = Shader.PropertyToID("_RGTex2");
    public static readonly int MaskTex = Shader.PropertyToID("_MaskTex");

    public static readonly int T = Shader.PropertyToID("_T");
    public static readonly int MaxT = Shader.PropertyToID("_MaxT");
}
}