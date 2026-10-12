using UnityEngine;
using System.Collections.Generic;


/*
Alligator AI logic is very closely tied to node logic. 

Alligator Node:
Frogs can step on the Alligaotr, this will make the Alligator wake up during its turn.

Alligator AI:
The Alligator Wakes Up:
- On its turn if a frog previously stepped on it.
- Immediately if a frog croaks.
On its Turn:
If the Alligator is not awake it does nothing.
If the Alligator is awake it eats what's on it.
*/
public class AlligatorNode : BasicNode
{
    public override bool IsTraversable => !IsOccupied;
    public override NodeHazard NodeType => NodeHazard.Alligator;
    public override int MaxFrogsOnPad { get { return 3; } }
    public override int CurrentFrogsOnPad { get { return base.CurrentFrogsOnPad; } set { base.CurrentFrogsOnPad = value; makeAlligatorAwakeOnTurn = true; } }
    private bool makeAlligatorAwakeOnTurn = false;
    private bool alligatorAwake = false;

    public override void CroakUsedOnPad()
    {
        alligatorAwake = true;
    }

    public void DoAlligatorTurn()
    {
        if (TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.Enemy)
        {
            return;
        }

        if (!alligatorAwake)
        {
            ChangeAlligatorAwake(makeAlligatorAwakeOnTurn);
            makeAlligatorAwakeOnTurn = false;
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

        ChangeAlligatorAwake(false);
    }

    // Will mainly be for if we want visual changes for this
    private void ChangeAlligatorAwake(bool awake)
    {
        alligatorAwake = awake;
    }
}
