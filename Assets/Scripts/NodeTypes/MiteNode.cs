using UnityEngine;

public class MiteNode : LilyPadNode
{
    [SerializeField]
    private Material _lilyPadMat;
    [SerializeField]
    private Material _miteMat;
    // Design note: I believe Mites are technically traversable but they kill you? We may want to make this doable if a player
    // falls very far behind mid-level and wants to give up so that way they don't carry the frustration with them
    // up to game design though.
    public override bool IsTraversable => !IsOccupied && turnsUntilMitesActive != 0;
    public override NodeHazard NodeType => NodeHazard.Mites;
    private Renderer miteRenderer;

    // At 0 mites are considered active, stop decrementing.
    private int turnsUntilMitesActive = 0;

    void Start()
    {
        miteRenderer = this.gameObject.GetComponent<Renderer>();
        if (!miteRenderer)
        {
            Debug.Log("Mite node is missing a renderer, will not be able to update materials.");
        }
    }

    private void UpdateMaterial(bool mitesActive)
    {
        miteRenderer.material = mitesActive ? _miteMat : _lilyPadMat;
    }
}
