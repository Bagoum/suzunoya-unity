using UnityEngine;

namespace SuzunoyaUnity.Rendering {
public static class Helpers {
    
    //TODO resolution
    public static RenderTexture DefaultTempRT() => DefaultTempRT((3840, 2160));

    private static RenderTexture DefaultTempRT((int w, int h) res) => RenderTexture.GetTemporary(res.w,
        //24 bit depth is required for sprite masks to work (used in dialogue handling)
        res.h, 24, RenderTextureFormat.ARGB32);
    
    
    public static ArbitraryCapturer CreateArbitraryCapturer(Transform tr) =>
        Object.Instantiate(SuzunoyaGlobals.Prefabs.arbitraryCapturer, tr, false)
            .GetComponent<ArbitraryCapturer>();
}
}