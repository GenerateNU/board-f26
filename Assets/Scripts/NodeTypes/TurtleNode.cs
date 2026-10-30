using System.Collections.Generic;
using UnityEngine;

public class TurtleNode : LilyPadNode
{
    [Header("Turtle Positions")]
    [SerializeField] private Transform secondarySpot;
    [SerializeField] private List<BasicNode> primaryNeighbors = new();
    [SerializeField] private List<BasicNode> secondaryNeighbors = new();

    private Vector3 _primaryPosition;
    private bool _isAwake;
    private bool _isAtSecondarySpot;

    public override IReadOnlyList<BasicNode> Neighbors =>
        _isAtSecondarySpot ? secondaryNeighbors : primaryNeighbors;

    private void Awake()
    {
        _primaryPosition = transform.position;
    }

    public override void FrogCroakedOnPad(FrogMove frog)
    {
        base.FrogCroakedOnPad(frog);

        if (frog != null && frog.GetLilyPadFrogOn() == this)
        {
            _isAwake = true;
        }
    }

    public override bool CanBeEnteredFrom(BasicNode source)
    {
        IReadOnlyList<BasicNode> currentNeighbors = Neighbors;
        for (int i = 0; i < currentNeighbors.Count; i++)
        {
            if (currentNeighbors[i] == source) return true;
        }

        return false;
    }

    public void DoTurtleTurn()
    {
        if (!_isAwake) return;

        if (_isAtSecondarySpot)
        {
            MoveTo(_primaryPosition);
            _isAtSecondarySpot = false;
            return;
        }

        if (secondarySpot == null)
        {
            Debug.LogWarning($"{name} has no secondary turtle spot assigned.", this);
            return;
        }

        if (!HasFrogOnBack()) return;

        MoveTo(secondarySpot.position);
        _isAtSecondarySpot = true;
    }

    private bool HasFrogOnBack()
    {
        if (TurnManager.Instance == null) return false;

        List<FrogMove> frogs = TurnManager.Instance.GetAllFrogs();
        for (int i = 0; i < frogs.Count; i++)
        {
            if (frogs[i] != null && frogs[i].GetLilyPadFrogOn() == this)
            {
                return true;
            }
        }

        return false;
    }

    private void MoveTo(Vector3 position)
    {
        Vector3 movement = position - transform.position;
        transform.position = position;

        if (TurnManager.Instance == null) return;

        List<FrogMove> frogs = TurnManager.Instance.GetAllFrogs();
        for (int i = 0; i < frogs.Count; i++)
        {
            if (frogs[i] != null && frogs[i].GetLilyPadFrogOn() == this)
            {
                frogs[i].transform.position += movement;
            }
        }
    }
}