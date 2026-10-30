using UnityEngine;

/*
On a LilyPadNode which has Mites Frogs cannot use the hop again action.
Mites are permanently dispelled by a Frog croaking within 2 nodes of it.
*/
public class MiteNode : LilyPadNode
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField]
    private Color _lilyPadColor;
    [SerializeField]
    private Color _miteColor;
    public override bool IsTraversable => !IsOccupied;
    public override NodeHazard NodeType => MitesActive ? NodeHazard.Mites : NodeHazard.None;
    private MeshRenderer miteRenderer;
    // Tint per pad without changing the shared LilyPadNormal material
    private MaterialPropertyBlock colorBlock;
    public bool MitesActive { get; private set; }

    protected override void Start()
    {
        MitesActive = true;
        miteRenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
        if (!miteRenderer)
        {
            Debug.Log("Mite node is missing a renderer, will not be able to update materials.");
        }
        UpdateMaterial(true);
        base.Start();
    }

    public override void CroakUsedOnPad()
    {
        MitesActive = false;
        UpdateMaterial(false);
    }

    private void UpdateMaterial(bool mitesActive)
    {
        if (!miteRenderer) { return; }

        colorBlock ??= new MaterialPropertyBlock();
        miteRenderer.GetPropertyBlock(colorBlock);
        colorBlock.SetColor(BaseColorId, mitesActive ? _miteColor : _lilyPadColor);
        miteRenderer.SetPropertyBlock(colorBlock);
    }
}
