using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Fish AI (Current):
On its turn:
 - Fish moves toward the closest player and traverses lilypads to reach them.
 - Fish should move 1.5 lilypads each turn this is shown by having them move 1 lilypad 
 - in Turn1 then 2 lilypads in Turn2.
 - A fish can only eat frogs during its turn and should only eat 1 frog during its turn.

On croak:
 - Fish should move 1 lilypad if croak is used. It should NOT eat whatever is on the lilypad.

To Add (According to Design Doc, Please Reference it not These Notes):
 - Decoy eggs which the Fish prioritizes eating over frogs.
 - Fish eating area being a radius.
*/
public class FishEnemy : AEnemy
{
    [SerializeField] private BasicNode currentFishNode;
    [SerializeField] private float swimSpeed = 1;
    [Tooltip("Lilypads the fish moves for the first time it moves.")]
    [SerializeField] private int lilyPadsPerOddTurn = 1;
    [SerializeField] private int lilyPadsPerEvenTurn = 2;
    // Keep track of this separately in FishEnemy (not EnemyTurnManager)
    // This is since first fish turn is not necessarily first EnemyTurn
    // First fish turn should always be lilyPadsPerOddTurn
    private bool oddTurn = true;

    private Vector3 lastPos;
    private List<BasicNode> Path;

    public BasicNode CurrentFishNode => currentFishNode;

    public override void DoEnemyTurn()
    {
        int timesDoEnemyTurn = oddTurn ? lilyPadsPerOddTurn : lilyPadsPerEvenTurn;
        animationFinished = false;

        // Every turn: chase whichever living frog is closest right now.
        FrogUnit target = FindClosestFrog();
        if (target == null || target.GetLilyPadFrogOn() == null || currentFishNode == null)
        {
            animationFinished = true; // no frogs left (or no board): nothing to do
            return;
        }

        bool needNewFrog = false;
        for (int i = 0; i < timesDoEnemyTurn; i++)
        {
            // This means we killed frog. Move to the next frog but do not kill it.
            // Note: This code atm only works with up to 2 moves for enemy
            // If we ever need fish to move more than 3 lilypads at once then adjust code
            // to have separate variable for ifFrogAlreadyKilled so false is always passed in
            // for kill
            if (needNewFrog)
            {
                target = FindClosestFrog();
                // No frogs left
                if (!target) { break; }
                needNewFrog = StepTowards(target.GetLilyPadFrogOn(), false);
            }
            else
            {
                needNewFrog = StepTowards(target.GetLilyPadFrogOn(), true);
            }
        }

        oddTurn = !oddTurn;
    }

    // Called when a frog uses the Croak action
    // Handle separately from DoEnemyTurn as we should not eat frogs here (also we don't want to change oddTurn)
    public override void CroakUsed()
    {
        FrogUnit target = FindClosestFrog();
        if (target == null || target.GetLilyPadFrogOn() == null || currentFishNode == null)
        {
            // Something is broken this shouldn't happen but we have it as a fail safe anyway
            Debug.Log("Fish target is null on croak.");
            return;
        }
        StepTowards(target.GetLilyPadFrogOn(), false);
    }

    // Kept for other scripts / testing: take one step toward a specific node.
    public void MoveTowards(BasicNode target)
    {
        animationFinished = false;
        StepTowards(target, true);
    }

    [ContextMenu("Move Towards Closest Frog")]
    public void MoveTowards()
    {
        DoEnemyTurn();
    }

    // Recalculates the path from where the fish is now and moves ONE node along it.
    // Returns in a frog has been killed.
    private bool StepTowards(BasicNode targetNode, bool killFrogs)
    {
        bool frogKilled = false;
        if (targetNode == null)
        {
            animationFinished = true;
            return false;
        }

        // Already on the frog's pad: just try to eat.
        if (targetNode == currentFishNode)
        {
            if (killFrogs) { frogKilled = CheckKillFrogs(); }
            animationFinished = true;
            return frogKilled;
        }

        Path = CalculatePath(currentFishNode, targetNode);
        if (Path == null || Path.Count < 2)
        {
            Debug.Log($"{name}: no path to {targetNode.name}, skipping this turn.");
            animationFinished = true;
            return false;
        }

        lastPos = transform.position;
        currentFishNode = Path[1]; // Path[0] is where the fish is now
        if (killFrogs) { frogKilled = CheckKillFrogs(); }
        StartCoroutine(GlideToCurrentFishNode());
        return frogKilled;
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

    // Eats (or respawns) every frog on the fish's current node.
    // Returns if a frog has been killed
    private bool CheckKillFrogs()
    {
        bool frogKilled = false;
        if (TurnManager.Instance == null) return false;

        foreach (FrogUnit frogMove in TurnManager.Instance.GetAllFrogs())
        {
            if (frogMove == null || frogMove.GetLilyPadFrogOn() != currentFishNode) continue;
            frogMove.KillFrog();
            AudioManager.Instance?.PlayFishChomp(); //SFX; needs accompanying animation + newspaper stuff but this is here for now
            frogKilled = true;
        }

        return frogKilled;
    }

    IEnumerator GlideToCurrentFishNode()
    {
        animationFinished = false;
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
        animationFinished = true;
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