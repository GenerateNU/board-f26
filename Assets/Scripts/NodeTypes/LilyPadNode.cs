using System.Collections.Generic;
using UnityEngine;

public enum NodeHazard { None, Mites, Alligator, Winning }

public class LilyPadNode : BasicNode
{
    public override bool IsTraversable => !IsOccupied && !isFishOnlyNode;
    public override NodeHazard NodeType => NodeHazard.None;
    [Header("Visual Variety")]
    [Tooltip("Randomize rotation, size and look when the game starts.")]
    [SerializeField] protected bool randomizeLook = true;
    [SerializeField] protected GameObject[] lilyPadVisualOption;
    [SerializeField] protected GameObject lilyPadForEditor;
    private Transform padTransform;

    protected virtual void Awake()
    {
        if (randomizeLook) RandomizeLook(lilyPadVisualOption);
    }

    public override void CroakUsedOnPad()
    {
        if (TryGetComponent<FlyPickup>(out var flyPickup))
        {
            flyPickup.RemoveFly();
        }
    }

    // Makes each pad look a little different: notch direction, size and leaf art.
    protected void RandomizeLook(GameObject[] visualOptions)
    {
        if (padTransform == null)
        {
            padTransform = GetComponentInChildren<Transform>();
            if (!padTransform) { Debug.Log("Couldn't get pad transform"); }
        }

        if (visualOptions.Length < 1) { return; }
        int randomLilyPad = Random.Range(0, visualOptions.Length);
        GameObject newObject = Instantiate(visualOptions[randomLilyPad], this.gameObject.transform);

        Destroy(lilyPadForEditor);
        lilyPadForEditor = newObject;

        // Lay the pad flat (90 on X) and point the notch in a random direction.
        //padTransform.localEulerAngles = new Vector3(90, 0, Random.Range(0f, 360f));

        // +-10% size, relative to the current size.
        //float size = Random.Range(0.9f, 1.1f);
        //padTransform.localScale *= size;
    }
}