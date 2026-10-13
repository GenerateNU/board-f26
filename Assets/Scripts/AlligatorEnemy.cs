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

        // TODO: Add art/death animation above if necessary and await on it
        if (!alligatorAwake)
        {
            alligatorAwake = makeAlligatorAwakeOnTurn;
            makeAlligatorAwakeOnTurn = false;
            animationFinished = true;
            return;
        }

        for (int i = 0; i < alligatorNodes.Length; i++)
        {
            alligatorNodes[i].KillFrogsOnAlligator();
        }

        alligatorAwake = false;
        animationFinished = true;
    }
}
