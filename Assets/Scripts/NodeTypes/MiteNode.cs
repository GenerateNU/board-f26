using UnityEngine;

/*
On a LilyPadNode which has Mites Frogs cannot use the hop again action.
Mites are permanently dispelled by a Frog croaking within 2 nodes of it.
*/
public class MiteNode : LilyPadNode
{
    [SerializeField] private GameObject[] lilyPadMiteVisualOption;
    public override bool IsTraversable => !IsOccupied;
    public override NodeHazard NodeType => MitesActive ? NodeHazard.Mites : NodeHazard.None;
    public bool MitesActive { get; private set; }

    protected override void Awake()
    {
        if (randomizeLook)
        {
            base.RandomizeLook(lilyPadMiteVisualOption);
        }
    }

    protected override void Start()
    {
        MitesActive = true;
        base.Start();
    }

    public override void CroakUsedOnPad()
    {
        MitesActive = false;
        UpdateMaterial(false);
    }

    private void UpdateMaterial(bool mitesActive)
    {
        if (mitesActive)
        {
            RandomizeLook(lilyPadMiteVisualOption);
        }
        else
        {
            RandomizeLook(lilyPadVisualOption);
        }
    }
}
