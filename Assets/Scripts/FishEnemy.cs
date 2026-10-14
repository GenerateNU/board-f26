using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishEnemy : AEnemy
{
    [SerializeField] private BasicNode currentFishNode;
    [SerializeField] private float swimSpeed = 1;

    private Vector3 lastPos;
    private List<BasicNode> Path;

    public BasicNode CurrentFishNode => currentFishNode;

    public override void DoEnemyTurn()
    {
        animationFinished = false;

        // Every turn: chase whichever living frog is closest right now.
        FrogMove target = FindClosestFrog();
        if (target == null || target.GetLilyPadFrogOn() == null || currentFishNode == null)
        {
            animationFinished = true; // no frogs left (or no board): nothing to do
            return;
        }

        StepTowards(target.GetLilyPadFrogOn());
    }

    // Kept for other scripts / testing: take one step toward a specific node.
    public void MoveTowards(BasicNode target)
    {
        animationFinished = false;
        StepTowards(target);
    }

    [ContextMenu("Move Towards Closest Frog")]
    public void MoveTowards()
    {
        DoEnemyTurn();
    }

    // Recalculates the path from where the fish is now and moves ONE node along it.
    private void StepTowards(BasicNode targetNode)
    {
        if (targetNode == null)
        {
            animationFinished = true;
            return;
        }

        // Already on the frog's pad: just try to eat.
        if (targetNode == currentFishNode)
        {
            CheckKillFrogs();
            animationFinished = true;
            return;
        }

        Path = CalculatePath(currentFishNode, targetNode);
        if (Path == null || Path.Count < 2)
        {
            Debug.Log($"{name}: no path to {targetNode.name}, skipping this turn.");
            animationFinished = true;
            return;
        }

        lastPos = transform.position;
        currentFishNode = Path[1]; // Path[0] is where the fish is now
        CheckKillFrogs();
        StartCoroutine(GlideToCurrentFishNode());
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

    // Eats (or respawns) every frog on the fish's current node.
    private void CheckKillFrogs()
    {
        if (TurnManager.Instance == null) return;

        foreach (FrogMove frogMove in TurnManager.Instance.GetAllFrogs())
        {
            if (frogMove == null || frogMove.GetLilyPadFrogOn() != currentFishNode) continue;

            FrogActions frogActions = frogMove.GetComponent<FrogActions>();
            BasicNode respawn = frogActions != null ? frogActions.respawnNode : null;

            if (respawn) // frog has laid an egg
            {
                frogMove.RespawnAt(respawn); // moves the frog AND updates its current pad
                Debug.Log($"{frogMove.name} caught, respawning at egg.");
            }
            else // frog did not lay egg
            {
                Destroy(frogMove.gameObject);
                Debug.Log($"{frogMove.name} eaten");
            }
        }
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
        animationFinished = true;
    }

    List<BasicNode> CalculatePath(BasicNode start, BasicNode target) //uses djisktra
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
            if (current == target) break; //path reached

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