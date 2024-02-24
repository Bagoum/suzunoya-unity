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

    public static void ModifyAlphas(TMP_Text text, List<byte> alphas, byte? deflt = null) {
        var chars = text.textInfo.characterInfo;
        void SetAlpha(int i, byte a) {
            //whitespace actually overlaps the vertices of other characters, this check is critical.
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

    public readonly struct Vertices {
        public readonly Vector3 tl;
        public readonly Vector3 bl;
        public readonly Vector3 tr;
        public readonly Vector3 br;
        
        public Vertices(Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br) {
            this.tl = tl;
            this.bl = bl;
            this.tr = tr;
            this.br = br;
        }
    }

    public static void GetVertices(TMP_Text text, List<Vertices> vertices) {
        vertices.Clear();
        var chars = text.textInfo.characterInfo;
        for (var i = 0; i < text.textInfo.characterCount; ++i) {
            var verts = text.textInfo.meshInfo[chars[i].materialReferenceIndex].vertices;
            var vi = chars[i].vertexIndex;
            // 1 2
            // 0 3
            vertices.Add(new Vertices(verts[vi], verts[vi + 1], verts[vi + 2], verts[vi + 3]));
        }
    }
    public static void UpdateVertices(TMP_Text text, List<Vertices> vertices, List<float> lerps, float lerpFrom) {
        var chars = text.textInfo.characterInfo;
        for (var i = 0; i < text.textInfo.characterCount; ++i) {
            //whitespace actually overlaps the vertices of other characters, this check is critical.
            if (!chars[i].isVisible) continue;
            var verts = text.textInfo.meshInfo[chars[i].materialReferenceIndex].vertices;
            var vi = chars[i].vertexIndex;
            var mult = Mathf.Lerp(lerpFrom, 1, i < lerps.Count ? lerps[i] : 1f);
            var basev = vertices[i];
            // 1 2
            // 0 3
            verts[vi] = (basev.bl - basev.tr) * mult + basev.tr;
            verts[vi + 1] = (basev.tl - basev.br) * mult + basev.br;
            verts[vi + 2] = (basev.tr - basev.bl) * mult + basev.bl;
            verts[vi + 3] = (basev.br - basev.tl) * mult + basev.tl;
        }
    }
    
}
