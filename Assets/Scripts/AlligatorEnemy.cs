using UnityEngine;

// This class primarily exists for the ease of the EnemyTurnManager. 
public class AlligatorEnemy : AEnemy
{
    [SerializeField]
    private AlligatorNode[] alligatorNodes;
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

    public void MakeAlligatorAwakeOnTurn(bool awake)
    {
        makeAlligatorAwakeOnTurn = awake;
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
        if (!alligatorAwake)
        {
            MakeAlligatorAwake(makeAlligatorAwakeOnTurn);
            makeAlligatorAwakeOnTurn = false;
            animationFinished = true;
            return;
        }

        for (int i = 0; i < alligatorNodes.Length; i++)
        {
            alligatorNodes[i].KillFrogsOnAlligator();
        }

        MakeAlligatorAwake(false);
        // If the alligator just killed someone it shouldn't wake up next turn
        makeAlligatorAwakeOnTurn = false;
        animationFinished = true;
    }
}
