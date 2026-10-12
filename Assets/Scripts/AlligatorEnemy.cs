using UnityEngine;

// This class primarily exists for the ease of the EnemyTurnManager. 
public class AlligatorEnemy : AEnemy
{
    private AlligatorNode alligatorNode;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        alligatorNode = this.GetComponent<AlligatorNode>();
        if (alligatorNode == null)
        {
            Debug.Log("Alligator enemy requires an alligator node.");
        }
    }

    public override void DoEnemyTurn()
    {
        animationFinished = false;
        // There isn't enough logic in AlligatorNode to justify 
        // dealing with the field sharing here.
        // TODO: Add art/death animation above if necessary and await on it
        alligatorNode.DoAlligatorTurn();
        animationFinished = true;
    }
}
