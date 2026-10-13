using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

/*
Fish AI:
Current: Fish moves toward the closest player and traverses lilypads to reach them.
Fish should move 1.5 lilypads each turn this is shown by having them move 1 lilypad 
in Turn1 then 2 lilypads in Turn2.
Fish should move 1 lilypad if croak is used.
*/
public class FishEnemy : AEnemy
{
    [SerializeField] private BasicNode currentFishNode;
    private FrogMove frogMove;
    private FrogActions frogActions;
    private GameObject currentClosestFrog;
    [SerializeField] private float swimSpeed = 1;
    [SerializeField] private int lilyPadsPerOddTurn = 1;
    [SerializeField] private int lilyPadsPerEvenTurn = 2;
    private bool oddTurn = true;

    private int progress = 0;
    private Vector3 lastPos;

    List<BasicNode> Path;

    public BasicNode CurrentFishNode => currentFishNode;

    void Awake()
    {
        reassignFrogComponents();
    }

    public override void DoEnemyTurn()
    {
        int timesDoEnemyTurn = oddTurn ? lilyPadsPerOddTurn : lilyPadsPerEvenTurn;

        SetClosestFrog();

        for (int i = 0; i < timesDoEnemyTurn; i++)
        {
            if (!currentClosestFrog) { SetClosestFrog(); }
            MoveTowards();
        }

        oddTurn = !oddTurn;
    }

    public override void CroakUsed()
    {
        if (!currentClosestFrog) { SetClosestFrog(); }
        MoveTowards();
    }

    private void SetClosestFrog()
    {
        currentClosestFrog = FindClosestFrog(TurnManager.Instance.GetAllFrogs());
        Debug.Log(currentClosestFrog);
        if (currentClosestFrog == null) { return; }
        reassignFrogComponents();
        Path = CalculatePath(currentFishNode, frogMove.GetLilyPadFrogOn());
        progress = 0;
    }

    //for future, only one frog in the scene so not needed.
    GameObject FindClosestFrog(List<FrogMove> frogs)
    {
        float minDist = float.MaxValue;
        GameObject closestFrog = null;
        foreach (FrogMove frogScript in frogs)
        {
            GameObject frog = frogScript.gameObject;
            Debug.Log(frog + " loop");
            float dist = Vector3.Distance(frog.transform.position, transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestFrog = frog;
                Debug.Log(closestFrog + " new closest frog");
            }
        }
        return closestFrog;
    }

    void reassignFrogComponents()
    {
        if (currentClosestFrog == null) { return; }
        frogMove = currentClosestFrog.GetComponent<FrogMove>();
        frogActions = currentClosestFrog.GetComponent<FrogActions>();
    }

    [ContextMenu("Move Towards")]
    public void MoveTowards()
    {
        if (currentClosestFrog == null) { Debug.LogWarning("No closest frog"); return; }
        lastPos = transform.position;
        progress++;
        UpdateCurrentNode(Path, progress);
        StartCoroutine(GlideToCurrentFishNode());
    }

    public void MoveTowards(BasicNode target)
    {
        Path = CalculatePath(currentFishNode, target);
        lastPos = transform.position;
        progress = 0;
        UpdateCurrentNode(Path, progress);
        StartCoroutine(GlideToCurrentFishNode());
    }

    void UpdateCurrentNode(List<BasicNode> path, int progress)
    {
        if (progress < path.Count)
        {
            currentFishNode = path[progress];
            CheckKillFrog();
        }
        //else
        //Debug.Log("progress: " + progress + " reached end of path");
    }

    void CheckKillFrog()
    {
        if (currentFishNode == frogMove.GetLilyPadFrogOn()) //frog caught
        {
            BasicNode respawn = frogActions.respawnNode;
            if (respawn) // if a respawn node exists => frog has layed an egg
            {
                currentClosestFrog.transform.position = respawn.transform.position; //teleport frog to respawn
            }
            else //frog did not lay egg
            {
                Destroy(currentClosestFrog);
                Debug.Log("Frog Eaten");
            }
        }
    }

    IEnumerator GlideToCurrentFishNode()
    {
        animationFinished = false;
        float elapsed = 0;
        while (Vector3.Distance(transform.position, currentFishNode.transform.position) > 0.1f)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(lastPos, currentFishNode.transform.position, elapsed / (1 / swimSpeed));
            yield return null;
        }

        transform.position = currentFishNode.transform.position;
        animationFinished = true;
    }

    //deprecated
    // BasicNode CalculateClosestNeighbor(BasicNode target) //has the issue of not being able to travel backwards
    // {
    //     BasicNode result = currentFishNode;
    //     float minDistance = Vector3.Distance(currentFishNode.transform.position, testTargetNode.transform.position);
    //     foreach(BasicNode neighbor in currentFishNode.Neighbors)
    //     {
    //         float distance = Vector3.Distance(neighbor.transform.position, target.transform.position);
    //         if(distance < minDistance)
    //         {
    //             minDistance = distance;
    //             result = neighbor;
    //         }
    //     }
    //     return result;
    // }

    List<BasicNode> CalculatePath(BasicNode start, BasicNode target) //uses djisktra
    {

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
                if (visited.Contains(neighbor)) continue;
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
        List<BasicNode> result = new List<BasicNode>();
        BasicNode trace = target;
        result.Insert(0, trace);
        BasicNode next = null;
        int step = 0;
        while (true)
        {
            if (parents.TryGetValue(trace, out next))
            {
                result.Insert(0, next);
                trace = next;
            }
            else
            {
                Debug.Log("error in back tracing in step: " + step);
                return null;
            }
            if (next == start)
            {
                return result;
            }
            step++;

        }

    }

}
