using UnityEngine;

// This class primarily exists for the ease of the EnemyTurnManager. 
public class AlligatorEnemy : AEnemy
{
    [SerializeField]
    private AlligatorNode[] alligatorNodes;
    [Tooltip("How many enemy turns (rounds) the alligator stays angry and lethal after being stepped on or croaked at.")]
    [SerializeField, Min(1)]
    private int angryRounds = 2;
    private int angryRoundsLeft = 0;
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

    // Stepping on or croaking at the alligator (re)starts its angry countdown.
    public void MakeAngry()
    {
        angryRoundsLeft = angryRounds;
        MakeAlligatorAwake(true);
    }

    public void MakeAlligatorAwake(bool awake)
    {
        alligatorAwake = awake;
        UpdateMaterial(alligatorAwake);
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
        if (angryRoundsLeft <= 0)
        {
            animationFinished = true;
            return;
        }

        for (int i = 0; i < alligatorNodes.Length; i++)
        {
            alligatorNodes[i].KillFrogsOnAlligator();
        }

        // Stays angry for angryRounds enemy turns, then cools down
        angryRoundsLeft--;
        if (angryRoundsLeft <= 0)
        {
            MakeAlligatorAwake(false);
        }
        animationFinished = true;
    }
}
