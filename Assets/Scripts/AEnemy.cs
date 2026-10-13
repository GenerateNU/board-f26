using UnityEngine;

// This is an abstract class so it is serializeable, 
// otherwise it'd be much better as an interface.
public abstract class AEnemy : MonoBehaviour
{
    protected bool animationFinished;
    public bool AnimationFinished { get { return animationFinished; } set { animationFinished = value; } }
    public virtual void CroakUsed() { return; }
    public abstract void DoEnemyTurn();
}
