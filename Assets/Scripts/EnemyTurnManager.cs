using UnityEngine;

public class EnemyTurnManager : MonoBehaviour
{
    [Tooltip("Enemies should be in turn order, this generally means fish last.")]
    [SerializeField]
    private AEnemy[] enemies;

    void Start()
    {
        TurnManager.Instance.OnTurnChanged += DoEnemyTurn;

    }

    private void DoEnemyTurn()
    {
        if (TurnManager.Instance.CurrentPhase != TurnManager.TurnPhase.Enemy)
        {
            return;
        }
        InputHandler.Instance?.BeginAnimation();
        // All animations can play at the same time (no async/await needed)
        // But actual turn order should be sequential to deal with
        // the case where an alligator may eat a frog that the fish
        // would've also tried to eat.
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].DoEnemyTurn();
        }

        // Once we add animations we may want a WhenAll await before switching turn
        // over to play. For now switch back immediately
        InputHandler.Instance?.EndAnimation();
        TurnManager.Instance.AdvanceTurn();
    }
}
