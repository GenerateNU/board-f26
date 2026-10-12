using UnityEngine;

// This is an abstract class so it is serializeable, 
// otherwise it'd be much better as an interface.
public abstract class AEnemy : MonoBehaviour
{
    public abstract void DoEnemyTurn();
}
