using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;

public class FishEnemy : MonoBehaviour
{
    [SerializeField] private LilyPadNode currentFishNode; //current node standing on
    [SerializeField] private LilyPadNode testTargetNode; //for testing
    [SerializeField] private float swimSpeed = 1;

    private int progress = 0;
    private Vector3 lastPos;

    List<LilyPadNode> Path;

    void Start()
    {
        Path = CalculatePath(currentFishNode, testTargetNode);
        progress = 0;
        for (int i = 0; i < Path.Count; i++)
        {
            Debug.Log($"path step {i}: {Path[i].gameObject.name}", Path[i].gameObject);
        }
    }

    void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            MoveTowards();
            Debug.Log("Move to platform " + progress);
        }
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
            currentFishNode = path[progress];
        else
            Debug.Log("progress: " + progress + " reached end of path");
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
    LilyPadNode CalculateClosestNeighbor(LilyPadNode target) //has the issue of not being able to travel backwards
    {
        LilyPadNode result = currentFishNode;
        float minDistance = Vector3.Distance(currentFishNode.transform.position, testTargetNode.transform.position);
        foreach(LilyPadNode neighbor in currentFishNode.Neighbors)
        {
            float distance = Vector3.Distance(neighbor.transform.position, target.transform.position);
            if(distance < minDistance)
            {
                minDistance = distance;
                result = neighbor;
            }
        }
        return result;
    }

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
            if (current == target) //path reached
            {

                break;
            }

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
        


        // Dictionary<float, LilyPadNode> map = new Dictionary<float, LilyPadNode>(); //<distance, reference to node>
        // //distances[current] = 0;
        // int trace = 0; //always start at 0
        // map.Add(0, currentFishNode);
        // //in a while loop
        // while(true)
        // {
        //     LilyPadNode current = map.ElementAt(trace).Value;
        //     float offsetDistance = map.ElementAt(trace).Key;
        //     foreach(LilyPadNode neighbor in current.Neighbors)
        //     {
        //         float distance = Vector3.Distance(neighbor.transform.position, current.transform.position) + offsetDistance;
        //         map.Add(distance, neighbor);
        //     }
        // }

        // List<int> distances = new List<int>();
        // List<LilyPadNode> parents = new List<LilyPadNode>();
        // Dictionary<int, LilyPadNode> map = new Dictionary<int, LilyPadNode>(); //<Distance parent pair index, node>
        //int[] distances = new int[nodes.Length];
        //LilyPadNode[] parents = new LilyPadNode[nodes.Length];
        // for(int i = 0; i < nodes.Length; i++)
        // {
        //     distances[i] = int.MaxValue;
        //     parents[i] = null;
        // }

        // distances.Add(0);
        // parents.Add(null);
        // map.Add(0, currentFishNode);
        // LilyPadNode current = currentFishNode; //=get current node
        // float distanceOfffset = 
        // foreach(LilyPadNode neighbor in current.Neighbors)
        // {
        //     float distance = Vector3.Distance(neighbor.transform.position, current.transform.position) + offsetDistance;
        //     map.Add(distance, neighbor);
        // }
        // List<LilyPadNode> result = new List<LilyPadNode>();
        // return result;


}
