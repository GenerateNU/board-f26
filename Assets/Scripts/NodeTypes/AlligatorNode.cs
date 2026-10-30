using UnityEngine;
using System.Collections.Generic;


/*
Alligator AI logic is very closely tied to node logic. 

Alligator Node:
Frogs can step on the Alligator, this makes the Alligator angry immediately.

Alligator AI:
The Alligator becomes angry for 2 rounds (angryRounds on AlligatorEnemy):
- Immediately if a frog steps on it (restarts the countdown if already angry).
- Immediately if a frog croaks.
On its Turn:
If the Alligator is not angry it does nothing.
If the Alligator is angry it eats what's on it, then counts down a round.
When the countdown reaches 0 it cools down.
*/
public class AlligatorNode : BasicNode
{
    public override bool IsTraversable => !IsOccupied;
    public override NodeHazard NodeType => NodeHazard.Alligator;
    public override int MaxFrogsOnPad { get { return 1; } }
    public override int CurrentFrogsOnPad
    {
        get { return base.CurrentFrogsOnPad; }
        set { base.CurrentFrogsOnPad = value; if (value != 0) { alligatorEnemy.MakeAngry(); } }
    }
    [SerializeField]
    private AlligatorEnemy alligatorEnemy;
    [SerializeField] private Renderer alligatorRenderer; // (was SpriteRenderer)

    protected override void Start()
    {
        // search children to find 3d model
        if (alligatorRenderer == null && transform.parent != null)
        {
            alligatorRenderer = transform.parent.GetComponentInChildren<MeshRenderer>();
        }

        if (!alligatorRenderer)
        {
            Debug.Log("Alligator node is missing a renderer, will not be able to update materials.");
        }
        base.Start();
    }

    public override void CroakUsedOnPad()
    {
        alligatorEnemy.MakeAngry();
    }

    public void UpdateMaterial(bool alligatorAwake, Color alligatorAwakeColor, Color alligatorNormalColor)
    { // quick check to prevent NullReferenceException during all hands
        if (alligatorRenderer != null)
        {
            Color targetColor = alligatorAwake ? alligatorAwakeColor : alligatorNormalColor;
            
            // bc it's 3d now
            alligatorRenderer.material.SetColor("_BaseColor", targetColor);
        }
    }

    public void KillFrogsOnAlligator()
    {
        if (TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.Enemy)
        {
            return;
        }

        // Kill any frogs or eggs on the alligator
        List<FrogMove> allFrogs = TurnManager.Instance.GetAllFrogs();
        foreach (FrogMove frog in allFrogs)
        {
            if (frog.GetLilyPadFrogOn() == this)
            {
                frog.KillFrog();
            }
        }
    }
}
