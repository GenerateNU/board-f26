using UnityEngine;

/*
On a LilyPadNode which has Mites Frogs cannot use the hop again action.
Mites are permanently dispelled by a Frog croaking within 2 nodes of it.
*/
public class MiteNode : LilyPadNode
{
    [SerializeField]
    private Material _lilyPadMat;
    [SerializeField]
    private Material _miteMat;
    public override bool IsTraversable => !IsOccupied;
    public override NodeHazard NodeType => NodeHazard.Mites;
    private Renderer miteRenderer;
    public bool MitesActive { get; private set; }

    protected override void Start()
    {
        MitesActive = true;
        miteRenderer = this.gameObject.GetComponentInChildren<Renderer>();
        if (!miteRenderer)
        {
            Debug.Log("Mite node is missing a renderer, will not be able to update materials.");
        }
        base.Start();
    }

    public override void CroakUsedOnPad()
    {
        MitesActive = false;
        UpdateMaterial(false);
    }

    private void UpdateMaterial(bool mitesActive)
    {
        miteRenderer.material = mitesActive ? _miteMat : _lilyPadMat;
    }
}
