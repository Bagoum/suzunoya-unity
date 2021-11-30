using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BagoumLib;
using TMPro;
using UnityEngine;

public static class TMPAlphaController {

    [ContextMenu("Debug characters")]
    public static void DebugChars(TMP_Text text) {
        var sb = new StringBuilder();
        foreach (var c in text.textInfo.characterInfo) {
            sb.Append($"{c.character}");
        }
        ModifyAlphas(text, 128.Range().Select(_ => (byte)128).ToList());
        Debug.Log(sb.ToString());
    }

    public static void SetAlphasZero(TMP_Text text) {
        var chars = text.textInfo.characterInfo;
        for (int ii = 0; ii < chars.Length; ++ii) {
            if (chars[ii].isVisible) {
                var colors = text.textInfo.meshInfo[chars[ii].materialReferenceIndex].colors32;
                var vi = chars[ii].vertexIndex;
                var c = colors[vi];
                c.a = 0;
                colors[vi] = colors[vi + 1] = colors[vi + 2] = colors[vi + 3] = c;
            }
        }
        text.UpdateVertexData();
    }
    public static void ModifyAlphas(TMP_Text text, List<byte> alphas, byte? deflt = null) {
        var chars = text.textInfo.characterInfo;
        void SetAlpha(int i, byte a) {
            //whitespace actually overlaps the verticies of other characters, this check is critical.
            if (!chars[i].isVisible) return;
            var colors = text.textInfo.meshInfo[chars[i].materialReferenceIndex].colors32;
            var vi = chars[i].vertexIndex;
            var c = colors[vi];
            c.a = a;
            colors[vi] = colors[vi + 1] = colors[vi + 2] = colors[vi + 3] = c;
        }
        int ii = 0;
        for (; ii < alphas.Count; ++ii)
            SetAlpha(ii, alphas[ii]);
        if (deflt.Try(out var d))
            for (; ii < text.textInfo.characterCount; ++ii)
                SetAlpha(ii, d);
        text.UpdateVertexData();
    }
}
