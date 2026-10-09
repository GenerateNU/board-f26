using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class FishEnemy : MonoBehaviour
{
    [SerializeField] private LilyPadNode currentFishNode;
    private FrogMove frogMove; 
    private FrogActions frogActions;
    [SerializeField] private GameObject frog; 
    [SerializeField] private float swimSpeed = 1;

    private int progress = 0;
    private Vector3 lastPos;

    List<LilyPadNode> Path;

    public LilyPadNode CurrentFishNode => currentFishNode;

    void Awake()
    {
       reassignFrogComponents();
    }

    void changeTurn()
    {
        if(TurnManager.Instance.CurrentPhase == TurnManager.TurnPhase.Enemy)
        {
            MoveTowards();
            TurnManager.Instance.AdvanceTurn();
        }
    }

    //for future, only one frog in the scene so not needed.
    GameObject FindClosestFrog(GameObject[] frogs)
    {
        float minDist = float.MaxValue;
        GameObject closestFrog = null;
        foreach(GameObject frog in frogs)
        {
            float dist = Vector3.Distance(frog.transform.position, transform.position);
            if(dist < minDist)
            {
                minDist = dist;
                closestFrog = frog;
            }
        } 
        return frog;
    }

    void reassignFrogComponents()
    {
        frogMove = frog.GetComponent<FrogMove>();
        frogActions = frog.GetComponent<FrogActions>();
    }

    void Start()
    {
        Path = CalculatePath(currentFishNode, frogMove.GetLilyPadFrogOn());
        progress = 0;
        for (int i = 0; i < Path.Count; i++)
        {
            Debug.Log($"path step {i}: {Path[i].gameObject.name}", Path[i].gameObject);
        }

        TurnManager.Instance.OnTurnChanged += changeTurn;
    }

    [ContextMenu("Move Towards")]
    public void MoveTowards()
    {
        lastPos = transform.position;
        progress++;
        UpdateCurrentNode(Path, progress);
        StartCoroutine(GlideToCurrentFishNode());
    }

    public void MoveTowards(LilyPadNode target)
    {
        Path = CalculatePath(currentFishNode, target);
        lastPos = transform.position;
        progress = 0;
        UpdateCurrentNode(Path, progress);
        StartCoroutine(GlideToCurrentFishNode());
    }

    void UpdateCurrentNode(List<LilyPadNode> path, int progress)
    {
        if(progress < path.Count)
        {
            currentFishNode = path[progress];
            CheckKillFrog();
        }  
        else
            Debug.Log("progress: " + progress + " reached end of path");
    }

    void CheckKillFrog()
    {
        if(currentFishNode == frogMove.GetLilyPadFrogOn()) //frog caught
        {
            LilyPadNode respawn = frogActions.respawnNode;
            if(respawn) // if a respawn node exists => frog has layed an egg
            {
                frog.transform.position = respawn.transform.position; //teleport frog to respawn
            }
            else //frog did not lay egg
            {
                Destroy(frog);
                Debug.Log("Frog Eaten");
            }
        }
    }

    IEnumerator GlideToCurrentFishNode()
    {
        float elapsed = 0;
        while(Vector3.Distance(transform.position, currentFishNode.transform.position) > 0.1f)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(lastPos, currentFishNode.transform.position,elapsed / (1 /swimSpeed));
            yield return null;
        }

        transform.position = currentFishNode.transform.position;
    }

    //deprecated
    // LilyPadNode CalculateClosestNeighbor(LilyPadNode target) //has the issue of not being able to travel backwards
    // {
    //     LilyPadNode result = currentFishNode;
    //     float minDistance = Vector3.Distance(currentFishNode.transform.position, testTargetNode.transform.position);
    //     foreach(LilyPadNode neighbor in currentFishNode.Neighbors)
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

    List<LilyPadNode> CalculatePath(LilyPadNode start, LilyPadNode target) //uses djisktra
    {
        
        Dictionary<LilyPadNode, float> distances = new Dictionary<LilyPadNode, float>{[start] = 0f};
        Dictionary<LilyPadNode, LilyPadNode> parents = new Dictionary<LilyPadNode, LilyPadNode>();
        HashSet<LilyPadNode> visited = new HashSet<LilyPadNode>();
        List<LilyPadNode> frontier = new List<LilyPadNode>{start};

        while(frontier.Count > 0) // while frontier is populated
        {
            float minDist = float.MaxValue;
            int toPop = -1;
            for(int i = 0; i < frontier.Count; i++)
            {
                if(distances[frontier[i]] < minDist)
                {
                    minDist = distances[frontier[i]];
                    toPop = i;
                }
            }
            LilyPadNode current = frontier[toPop];
            frontier.RemoveAt(toPop);
            if (!visited.Add(current)) continue; // already finalized (duplicate entry)
            if (current == target) break; //path reached

            foreach (LilyPadNode neighbor in current.Neighbors)
            {
                if(visited.Contains(neighbor)) continue;
                float edge = Vector3.Distance(current.transform.position, neighbor.transform.position);
                float newDist = distances[current] + edge;

                if(!distances.TryGetValue(neighbor, out float dist) || newDist < dist)
                {
                    distances[neighbor] = newDist;
                    parents[neighbor] = current;
                    frontier.Add(neighbor);
                }
            } 
        }
        // back track
        List<LilyPadNode> result = new List<LilyPadNode>();
        LilyPadNode trace = target;
        result.Insert(0, trace);
        LilyPadNode next = null;
        int step = 0;
        while(true)
        {
            
            if(parents.TryGetValue(trace, out next))
            {
                result.Insert(0, next);
                trace = next;
            }
            else
            {
                Debug.Log("error in back tracing in step: " + step);
                return null;
            }
            if(next == start)
            {
                return result;
            }
            step++;
            
        }

    }

}
