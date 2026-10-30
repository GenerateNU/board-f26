using UnityEngine;

public class FlyPickup : MonoBehaviour
{
    private const int FlyScore = 20;

    [SerializeField] private GameObject flyVisual;
    public bool IsCollected { get; private set; }

    public void Collect(FrogMove frog)
    {
        if (IsCollected || frog == null || frog.GetLilyPadFrogOn() != GetComponent<BasicNode>()) return;

        frog.AddScore(FlyScore);
        RemoveFly();
    }

    public void RemoveFly()
    {
        IsCollected = true;
        if (flyVisual != null)
        {
            flyVisual.SetActive(false);
        }
    }
}