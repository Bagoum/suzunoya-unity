using BagoumLib.Events;
using UnityEngine;

namespace SuzunoyaUnity.Rendering {
public static class RenderHelpers {
    public static readonly Evented<(int w, int h)> PreferredResolution = new Evented<(int w, int h)>((3840, 2160));
    
    public static RenderTexture DefaultTempRT() => DefaultTempRT(PreferredResolution);

    public static RenderTexture DefaultTempRT((int w, int h) res) => RenderTexture.GetTemporary(res.w,
        //24 bit depth is required for sprite masks to work (used in dialogue handling)
        res.h, 24, RenderTextureFormat.ARGB32);
}
}