using UnityEngine;

namespace SuzunoyaUnity.Mimics {
public class OneSpriteMimic : RenderedMimic {
    public SpriteRenderer sr = null!;
    private Color baseColor;
    public override string SortingLayerFromPrefab => sr.sortingLayerName;

    protected override void Awake() {
        base.Awake();
        baseColor = sr.color;
    }
    protected override void SetSortingLayer(int layer) => sr.sortingLayerID = layer;

    protected override void SetSortingID(int id) => sr.sortingOrder = id;

    protected override void SetVisible(bool visible) => sr.enabled = visible;

    protected override void SetTint(Color c) => sr.color = baseColor * c;
}
}