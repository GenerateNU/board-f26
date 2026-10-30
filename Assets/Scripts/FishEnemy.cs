using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Fish AI:
Each enemy turn the fish picks a target and swims toward it along the lilypads:
1. Decoy eggs come first: it chases the nearest egg it can reach.
2. If there are no eggs, it chases the closest frog.
Fish should move 1.5 lilypads each turn this is shown by having them move 1 lilypad
in Turn1 then 2 lilypads in Turn2.
Fish should move 1 lilypad if croak is used.
When the fish lands on a pad it eats what's there (an egg first, otherwise frogs)
and stops moving for the rest of that turn.
*/
public class FishEnemy : AEnemy
{
    [SerializeField] private BasicNode currentFishNode;
    [SerializeField] private float swimSpeed = 1;
    [SerializeField] private int lilyPadsPerOddTurn = 1;
    [SerializeField] private int lilyPadsPerEvenTurn = 2;
    [SerializeField] private int lilyPadsWhenCroaked = 1;

    private Vector3 lastPos;
    private List<BasicNode> Path;
    private Coroutine _moveRoutine;

    public BasicNode CurrentFishNode => currentFishNode;

    public override void DoEnemyTurn()
    {
        bool evenTurn = EnemyTurnManager.Instance != null && EnemyTurnManager.Instance.currentTurn % 2 == 0;
        TakeSteps(evenTurn ? lilyPadsPerEvenTurn : lilyPadsPerOddTurn);
    }

    // Called when a frog uses the Croak action
    public override void CroakUsed()
    {
        TakeSteps(lilyPadsWhenCroaked);
    }

    // Kept for other scripts / testing: take one step toward a specific node.
    public void MoveTowards(BasicNode target)
    {
        animationFinished = false;
        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveSteps(1, target));
    }

    [ContextMenu("Take Fish Turn")]
    public void MoveTowards()
    {
        DoEnemyTurn();
    }

    // Moves up to `steps` lilypads, one after another (never two swims at once).
    private void TakeSteps(int steps)
    {
        animationFinished = false;

        if (currentFishNode == null || steps <= 0)
        {
            animationFinished = true;
            return;
        }

        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveSteps(steps, null));
    }

    // fixedTarget: null = pick egg/frog target each step.
    private IEnumerator MoveSteps(int steps, BasicNode fixedTarget)
    {
        for (int i = 0; i < steps; i++)
        {
            BasicNode target = fixedTarget != null ? fixedTarget : FindTargetNode();
            if (target == null) break; // nothing to chase

            // Already on the target's pad: eat and stop.
            if (target == currentFishNode)
            {
                EatWhateverIsHere();
                break;
            }

            Path = CalculatePath(currentFishNode, target);
            if (Path == null || Path.Count < 2)
            {
                Debug.Log($"{name}: no path to {target.name}, skipping the rest of this turn.");
                break;
            }

            lastPos = transform.position;
            currentFishNode = Path[1]; // Path[0] is where the fish is now
            yield return GlideToCurrentFishNode();

            // Eat on arrival. Eating (an egg or a frog) ends the fish's movement this turn.
            if (EatWhateverIsHere()) break;
        }

        _moveRoutine = null;
        animationFinished = true;
    }

    // Decoy eggs first, otherwise the closest frog's pad.
    private BasicNode FindTargetNode()
    {
        BasicNode eggNode = FindDecoyTarget();
        if (eggNode != null) return eggNode;

        FrogMove frog = FindClosestFrog();
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

    private FrogMove FindClosestFrog()
    {
        if (TurnManager.Instance == null) return null;

        float minDist = float.MaxValue;
        FrogMove closest = null;
        foreach (FrogMove candidate in TurnManager.Instance.GetAllFrogs())
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

    // Eggs come first: if there's an egg on this pad the fish eats it, and frogs on the
    // same pad survive this time. Otherwise it eats every frog on this pad.
    // Returns true if it ate anything.
    private bool EatWhateverIsHere()
    {
        bool ateEgg = false;
        foreach (DecoyEgg egg in DecoyEgg.GetNearestFirst(transform.position))
        {
            if (egg.Node != currentFishNode) continue;
            egg.Eat();
            ateEgg = true;
        }
        if (ateEgg) return true;

        if (TurnManager.Instance == null) return false;

        bool ateFrog = false;
        foreach (FrogMove frogMove in TurnManager.Instance.GetAllFrogs())
        {
            if (frogMove == null || frogMove.GetLilyPadFrogOn() != currentFishNode) continue;
            frogMove.KillFrog();
            ateFrog = true;
        }
        return ateFrog;
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