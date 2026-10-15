using System.Threading.Tasks;
using UnityEngine;

public class EnemyTurnManager : MonoBehaviour
{
    [Tooltip("Enemies should be in turn order, this generally means fish last.")]
    [SerializeField]
    private AEnemy[] enemies;
    [SerializeField]
    private int turnFishStartsActions;
    private int currentTurn = 0;
    public static EnemyTurnManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple TurnManagers found; destroying the duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        TurnManager.Instance.OnTurnChanged += DoEnemyTurn;
    }

    public void CroakUsed()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (ShouldSkipFishTurn(enemies[i])) { continue; }
            enemies[i].CroakUsed();
        }
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
            if (ShouldSkipFishTurn(enemies[i]))
            {
                enemies[i].AnimationFinished = true;
                continue;
            }
            enemies[i].DoEnemyTurn();
        }

        currentTurn++;
        _ = SwitchTurn();
    }

    private bool ShouldSkipFishTurn(AEnemy enemy)
    {
        return currentTurn < turnFishStartsActions && enemy is FishEnemy;
    }

    private async Task SwitchTurn()
    {
        // Would love to do an AwaitAll async here but this allows more flexibility
        // if people wanna do coroutines. Although the code here is worse to read.
        // Need small time between enemy and player turn otherwise
        // highlighting gets messed up. Wait for all animations to finish before
        // switching turn
        for (int i = 0; i < enemies.Length; i++)
        {
            while (!enemies[i].AnimationFinished)
            {
                await Awaitable.WaitForSecondsAsync(0.1f);
            }
        }

        InputHandler.Instance?.EndAnimation();
        TurnManager.Instance.AdvanceTurn();
    }
}
