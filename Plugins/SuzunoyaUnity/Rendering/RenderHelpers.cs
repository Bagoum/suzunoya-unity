using BagoumLib.Events;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace SuzunoyaUnity.Rendering {
public static class RenderHelpers {
    public static readonly Evented<(int w, int h)> PreferredResolution = new Evented<(int w, int h)>((3840, 2160));

    public static RenderTexture DefaultTempRT((int w, int h) res, bool useDepth = true) {
        var rt = RenderTexture.GetTemporary(res.w, res.h, useDepth ? 24 : 0, RenderTextureFormat.ARGB32);
        rt.depthStencilFormat = useDepth ?
            //by default Unity might assign D32_SFloat_S8_UInt which is 8 bytes;
            // if possible we want to use D24_UNorm_S8_UInt which is 4 byte
            GraphicsFormatUtility.GetDepthStencilFormat(24, 8)
            : GraphicsFormat.None;
        return rt;
    }

    public static RenderTexture CloneRTFormat(RenderTexture baseRt) {
        var rt = RenderTexture.GetTemporary(baseRt.width, baseRt.height, baseRt.depth, baseRt.graphicsFormat);
        rt.depthStencilFormat = baseRt.depthStencilFormat;
        return rt;
    }
    
    //TODO: make depth bits an argument, use 24 for main camera/arbCam and 0 for ui, not sure about VN render groups
}
}