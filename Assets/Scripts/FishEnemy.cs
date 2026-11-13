using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Fish AI (Current):
On its turn:
 - Fish moves toward its target and traverses lilypads to reach it.
- If the target is a decoy egg, the fish will eat it. If the target is a frog, the fish will eat it.
 - Fish should move 1.5 lilypads each turn this is shown by having them move 1 lilypad
 - in Turn1 then 2 lilypads in Turn2.
 - A fish can only eat during its turn and only eats once per turn (an egg OR frogs).
   An egg on a pad is eaten before any frog on that pad, so frogs there survive.
   After eating, the fish keeps moving toward its next target but doesn't eat again.

On croak:
 - Fish should move 1 lilypad if croak is used. It should NOT eat whatever is on the lilypad.

To Add (According to Design Doc, Please Reference it not These Notes):
 - Fish eating area being a radius.
*/
public class FishEnemy : AEnemy
{
    [SerializeField] private BasicNode currentFishNode;
    [SerializeField] private float swimSpeed = 1;
    [Tooltip("Lilypads the fish moves for the first time it moves.")]
    [SerializeField] private int lilyPadsPerOddTurn = 1;
    [SerializeField] private int lilyPadsPerEvenTurn = 2;
    [Tooltip("Lilypads the fish moves when a frog croaks (it never eats on a croak).")]
    [SerializeField] private int lilyPadsWhenCroaked = 1;
    // Keep track of this separately in FishEnemy (not EnemyTurnManager)
    // This is since first fish turn is not necessarily first EnemyTurn
    // First fish turn should always be lilyPadsPerOddTurn
    private bool oddTurn = true;

    private Vector3 lastPos;
    private List<BasicNode> Path;
    private Coroutine _moveRoutine;

    public BasicNode CurrentFishNode => currentFishNode;

    public override void DoEnemyTurn()
    {
        int steps = oddTurn ? lilyPadsPerOddTurn : lilyPadsPerEvenTurn;
        oddTurn = !oddTurn;
        StartMoving(steps, true);
    }

    // Called when a frog uses the Croak action
    // Handle separately from DoEnemyTurn as we should not eat here (also we don't want to change oddTurn)
    public override void CroakUsed()
    {
        StartMoving(lilyPadsWhenCroaked, false);
    }

    [ContextMenu("Take Fish Turn")]
    public void MoveTowards()
    {
        DoEnemyTurn();
    }

    // Kept for other scripts / testing: take one step toward a specific node (eating allowed).
    public void MoveTowards(BasicNode target)
    {
        StartMoving(1, true, target);
    }

    private void StartMoving(int steps, bool canEat, BasicNode fixedTarget = null)
    {
        animationFinished = false;

        if (currentFishNode == null || steps <= 0)
        {
            animationFinished = true;
            return;
        }

        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveSteps(steps, canEat, fixedTarget));
    }

    // Moves up to `steps` lilypads ONE AFTER ANOTHER (each swim finishes before the next starts).
    // Picks the target again before every step, so after eating it heads for the next egg/frog.
    private IEnumerator MoveSteps(int steps, bool canEat, BasicNode fixedTarget)
    {
        bool hasEaten = false;

        for (int i = 0; i < steps; i++)
        {
            BasicNode target = fixedTarget != null ? fixedTarget : FindTargetNode();
            if (target == null) break; // nothing left to chase

            // Already on the target's pad: try to eat there instead of moving.
            if (target == currentFishNode)
            {
                if (canEat && !hasEaten && EatWhateverIsHere())
                {
                    hasEaten = true;
                    continue; // meal used; next step heads for the next target
                }
                break; // can't eat here and nowhere better to go
            }

            Path = CalculatePath(currentFishNode, target);
            if (Path == null || Path.Count < 2)
            {
                Debug.Log($"{name}: no path to {target.name}, skipping the rest of this move.");
                break;
            }

            lastPos = transform.position;
            currentFishNode = Path[1]; // Path[0] is where the fish is now
            yield return GlideToCurrentFishNode();

            // Eat on arrival, at most once per turn and never on a croak.
            if (canEat && !hasEaten && EatWhateverIsHere()) hasEaten = true;
        }

        _moveRoutine = null;
        animationFinished = true;
    }

    // Decoy eggs first, otherwise the closest frog's pad.
    private BasicNode FindTargetNode()
    {
        BasicNode eggNode = FindDecoyTarget();
        if (eggNode != null) return eggNode;

        FrogUnit frog = FindClosestFrog();
        return frog != null ? frog.GetLilyPadFrogOn() : null;
    }

    // Nearest egg the fish can actually reach (by straight-line distance), or null.
    private BasicNode FindDecoyTarget()
    {
        foreach (DecoyEgg egg in DecoyEgg.GetNearestFirst(transform.position))
        {
            if (egg.Node == currentFishNode) return egg.Node;
            if (CalculatePath(currentFishNode, egg.Node) != null) return egg.Node;
        }
        return null;
    }

    private FrogUnit FindClosestFrog()
    {
        if (TurnManager.Instance == null) return null;

        float minDist = float.MaxValue;
        FrogUnit closest = null;
        foreach (FrogUnit candidate in TurnManager.Instance.GetAllFrogs())
        {
            if (candidate == null) continue; // destroyed
            float dist = Vector3.Distance(candidate.transform.position, transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = candidate;
            }
        }
        return closest;
    }

    // One meal: an egg on this pad if there is one (frogs here survive),
    // otherwise the frogs on this pad. Returns true if anything was eaten.
    private bool EatWhateverIsHere()
    {
        foreach (DecoyEgg egg in DecoyEgg.GetNearestFirst(transform.position))
        {
            if (egg.Node != currentFishNode) continue;
            egg.Eat();
            return true;
        }

        return CheckKillFrogs();
    }

    // Eats every frog on the fish's current node.
    // Returns if a frog has been killed
    private bool CheckKillFrogs()
    {
        bool frogKilled = false;
        if (TurnManager.Instance == null) return false;

        foreach (FrogUnit frog in TurnManager.Instance.GetAllFrogs())
        {
            if (frog == null || frog.GetLilyPadFrogOn() != currentFishNode) continue;
            frog.KillFrog();
            frogKilled = true;
        }

        return frogKilled;
    }

    IEnumerator GlideToCurrentFishNode()
    {
        float elapsed = 0;
        Vector3 targetPos = currentFishNode.transform.position;
        float duration = swimSpeed > 0 ? 1f / swimSpeed : 0f;

        while (Vector3.Distance(transform.position, targetPos) > 0.1f && elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(lastPos, targetPos, duration > 0 ? elapsed / duration : 1f);
            yield return null;
        }

        transform.position = targetPos;
    }

    List<BasicNode> CalculatePath(BasicNode start, BasicNode target) // uses dijkstra
    {
        if (start == null || target == null) return null;
        if (start == target) return new List<BasicNode> { start };

        Dictionary<BasicNode, float> distances = new Dictionary<BasicNode, float> { [start] = 0f };
        Dictionary<BasicNode, BasicNode> parents = new Dictionary<BasicNode, BasicNode>();
        HashSet<BasicNode> visited = new HashSet<BasicNode>();
        List<BasicNode> frontier = new List<BasicNode> { start };

        while (frontier.Count > 0) // while frontier is populated
        {
            float minDist = float.MaxValue;
            int toPop = -1;
            for (int i = 0; i < frontier.Count; i++)
            {
                if (distances[frontier[i]] < minDist)
                {
                    minDist = distances[frontier[i]];
                    toPop = i;
                }
            }
            BasicNode current = frontier[toPop];
            frontier.RemoveAt(toPop);
            if (!visited.Add(current)) continue; // already finalized (duplicate entry)
            if (current == target) break; // path reached

            foreach (BasicNode neighbor in current.Neighbors)
            {
                if (neighbor == null || visited.Contains(neighbor)) continue;
                float edge = Vector3.Distance(current.transform.position, neighbor.transform.position);
                float newDist = distances[current] + edge;

                if (!distances.TryGetValue(neighbor, out float dist) || newDist < dist)
                {
                    distances[neighbor] = newDist;
                    parents[neighbor] = current;
                    frontier.Add(neighbor);
                }
            }
        }

        // back track
        List<BasicNode> result = new List<BasicNode> { target };
        BasicNode trace = target;
        int safety = 0;
        while (trace != start)
        {
            if (!parents.TryGetValue(trace, out BasicNode next) || ++safety > 10000)
            {
                return null; // target not reachable from start
            }
            result.Insert(0, next);
            trace = next;
        }
        return result;
    }
}