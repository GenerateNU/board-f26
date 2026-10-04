using UnityEngine;

public class FrogActions : MonoBehaviour
{
    // sprint 2 skel
    // needs to be public to check for respawn
    public LilyPadNode respawnNode; 

    public bool HasActedThisTurn { get; set; } = false;

    // croak, lay egg, hop again etc.

}
