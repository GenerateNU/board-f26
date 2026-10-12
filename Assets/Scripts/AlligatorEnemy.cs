using UnityEngine;

// This class primarily exists for the ease of the EnemyTurnManager. 
public class AlligatorEnemy : AEnemy
{
    [SerializeField]
    private AlligatorNode[] alligatorNodes;
    private bool makeAlligatorAwakeOnTurn = false;
    private bool alligatorAwake = false;

    public void MakeAlligatorAwakeOnTurn(bool awake)
    {
        makeAlligatorAwakeOnTurn = awake;
    }

    public void MakeAlligatorAwake(bool awake)
    {
        alligatorAwake = awake;
    }

    public override void DoEnemyTurn()
    {
        animationFinished = false;
        // There isn't enough logic in AlligatorNode to justify 
        // dealing with the field sharing here.
        // TODO: Add art/death animation above if necessary and await on it
        for (int i = 0; i < alligatorNodes.Length; i++)
        {
            alligatorNodes[i].DoAlligatorTurn(makeAlligatorAwakeOnTurn, alligatorAwake);
        }
        animationFinished = true;
    }
}
