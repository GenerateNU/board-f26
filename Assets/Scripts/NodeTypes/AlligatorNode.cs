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
    public override int MaxFrogsOnPad { get { return 1; } }
    public override int CurrentFrogsOnPad
    {
        get { return base.CurrentFrogsOnPad; }
        set { base.CurrentFrogsOnPad = value; alligatorEnemy.MakeAlligatorAwakeOnTurn(true); }
    }
    [SerializeField]
    private AlligatorEnemy alligatorEnemy;

    public override void CroakUsedOnPad()
    {
        alligatorEnemy.MakeAlligatorAwake(true);
    }

    public void DoAlligatorTurn(bool makeAlligatorAwakeOnTurn, bool alligatorAwake)
    {
        if (TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.Enemy)
        {
            return;
        }

        if (!alligatorAwake)
        {
            alligatorEnemy.MakeAlligatorAwake(makeAlligatorAwakeOnTurn);
            alligatorEnemy.MakeAlligatorAwakeOnTurn(false);
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

        alligatorEnemy.MakeAlligatorAwake(false);
    }
}
