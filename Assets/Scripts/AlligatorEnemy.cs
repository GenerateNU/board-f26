using UnityEngine;

// This class primarily exists for the ease of the EnemyTurnManager. 
// See AlligatorNode for AI details, CroakUsedOnPad, and CurrentFrogsOnPad setter.
public class AlligatorEnemy : AEnemy
{
    [SerializeField]
    private AlligatorNode[] alligatorNodes;
    [Tooltip("How many enemy turns (rounds) the alligator stays awake and lethal after being stepped on or croaked at.")]
    [SerializeField, Min(1)]
    private int roundsUntilSleep = 2;
    private int roundsLeft = 0;
    private bool makeAlligatorAwakeOnTurn = false;
    private bool alligatorAwake = false;
    [SerializeField]
    private Color alligatorNormalColor;
    [SerializeField]
    private Color alligatorAwakeColor;
    private SpriteRenderer alligatorEnemyRenderer;

    protected void Start()
    {
        alligatorEnemyRenderer = this.gameObject.GetComponentInChildren<SpriteRenderer>();
        if (!alligatorEnemyRenderer)
        {
            Debug.Log("Alligator enemy is missing a renderer, will not be able to update materials.");
        }
    }

    public void MakeAlligatorAwake(bool awake)
    {
        if (awake)
        {
            roundsLeft = roundsUntilSleep;
        }
        alligatorAwake = awake;
        UpdateMaterial(alligatorAwake);
        if (awake) AudioManager.Instance?.PlayAlligatorAngry(); // woke up
        else AudioManager.Instance?.PlayAlligatorSleep(); //otherwise asleep
    }

    public void MakeAlligatorAwakeOnTurn(bool awake)
    {
        makeAlligatorAwakeOnTurn = awake;
    }

    public void UpdateMaterial(bool alligatorAwake)
    {
        alligatorEnemyRenderer.color = alligatorAwake ? alligatorAwakeColor : alligatorNormalColor;
        for (int i = 0; i < alligatorNodes.Length; i++)
        {
            alligatorNodes[i].UpdateMaterial(alligatorAwake, alligatorAwakeColor, alligatorNormalColor);
        }
    }

    public override void DoEnemyTurn()
    {
        animationFinished = false;

        // TODO: Add art/death animation above if necessary and await on it
        if (!alligatorAwake)
        {
            MakeAlligatorAwake(makeAlligatorAwakeOnTurn);
            makeAlligatorAwakeOnTurn = false;
            animationFinished = true;
            return;
        }
        else if (makeAlligatorAwakeOnTurn)
        {
            // Reset countdown
            MakeAlligatorAwake(makeAlligatorAwakeOnTurn);
            makeAlligatorAwakeOnTurn = false;
        }

        for (int i = 0; i < alligatorNodes.Length; i++)
        {
            alligatorNodes[i].KillFrogsOnAlligator();
        }

        roundsLeft--;
        if (roundsLeft == 0)
        {
            MakeAlligatorAwake(false);
        }

        animationFinished = true;
    }
}
