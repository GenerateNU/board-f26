using UnityEngine;
using System.Collections.Generic;


/*
Alligator AI logic is very closely tied to node logic. 

Alligator Node:
Frogs can step on the Alligator, this will make the Alligator wake up on its turn.
If the alligator is already awake this will reset the count down before it falls asleep.

Alligator AI:
The Alligator Wakes up:
- On its turn if a frog previously stepped on it.
- Immediately if a frog croaks within 2 lilypads.
Once the alligator wakes up it stays awake for at least two turns.
While the alligator is awake croaking within the alligator's radius or 
stepping on it resets the countdown before it falls asleep.

On its Turn:
If the Alligator asleep it does nothing.
If the Alligator is awake it eats what's on it, then counts down a round.
When the countdown reaches 0 it goes back to sleep.
*/
public class AlligatorNode : BasicNode
{
    public override bool IsTraversable => !IsOccupied;
    public override NodeHazard NodeType => NodeHazard.Alligator;
    public override int MaxFrogsOnPad { get { return 1; } }
    public override int CurrentFrogsOnPad
    {
        get { return base.CurrentFrogsOnPad; }
        set { base.CurrentFrogsOnPad = value; if (value != 0) { alligatorEnemy.MakeAlligatorAwakeOnTurn(true); } }
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
        alligatorEnemy.MakeAlligatorAwake(true);
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
        List<FrogUnit> allFrogs = TurnManager.Instance.GetAllFrogs();
        foreach (FrogUnit frog in allFrogs)
        {
            if (frog.GetLilyPadFrogOn() == this)
            {
                frog.KillFrog();
            }
        }
    }
}
