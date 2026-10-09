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
    public override bool IsTraversable => !IsOccupied && currentTurnsUntilMitesActive != 0;
    public override NodeHazard NodeType => NodeHazard.Mites;
    [Tooltip("This refers to how many enemy actions before they reappear.")]
    [SerializeField]
    private int turnsForMitesToReappear = 3;
    private Renderer miteRenderer;

    // At 0 mites are considered active, stop decrementing.
    private int currentTurnsUntilMitesActive = 0;

    void Start()
    {
        miteRenderer = this.gameObject.GetComponent<Renderer>();
        if (!miteRenderer)
        {
            Debug.Log("Mite node is missing a renderer, will not be able to update materials.");
        }
    }

    public void CroakUsedOnPad()
    {
        currentTurnsUntilMitesActive = turnsForMitesToReappear;
    }

    private void UpdateMaterial(bool mitesActive)
    {
        miteRenderer.material = mitesActive ? _miteMat : _lilyPadMat;
    }
}
