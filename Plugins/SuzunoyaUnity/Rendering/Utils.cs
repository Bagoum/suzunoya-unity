using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace SuzunoyaUnity.Rendering {
public static class Utils {


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte Float01ToByte(float f) =>
        f <= 0 ? byte.MinValue :
        f >= 1 ? byte.MaxValue :
        (byte) (f * 256f);
    
    /// <summary>
    /// Create a texture mask. Caller must destroy the texture.
    /// </summary>
    /// <param name="w">Width in pixels.</param>
    /// <param name="h">Height in pixels.</param>
    /// <param name="filter">UV -> alpha function.</param>
    public static Texture2D CreateMask(int w, int h, Func<float, float, float> filter) {
        //Note: maye use TextureFormat.A8 instead
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapModeU = TextureWrapMode.Clamp;
        tex.wrapModeV = TextureWrapMode.Clamp;
        return UpdateMask(tex, filter);
    }

    public static Texture2D UpdateMask(Texture2D tex, Func<float, float, float> filter) {
        var w = tex.width;
        var h = tex.height;
        var pixels_n = tex.GetRawTextureData<Color32>();
        unsafe {
            var pixels = (Color32*) pixels_n.GetUnsafePtr();
            var index = 0;
            for (int y = 0; y < h; ++y) {
                for (int x = 0; x < w; ++x) {
                    ref var pixel = ref pixels[index++];
                    pixel.r = byte.MaxValue;
                    pixel.g = byte.MaxValue;
                    pixel.b = byte.MaxValue;
                    pixel.a = Float01ToByte(filter(x / (w - 1f), y / (h - 1f)));
                }
            }
        }
        tex.Apply();
        return tex;
    }
}
}