using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

// Creates a custom Label on the inspector for all the scripts named ScriptName, true is so inherited classes also get this
[CustomEditor(typeof(BasicNode), true)]
public class BasicNodeEditor : Editor
{
    // Recommended Reading: https://docs.unity3d.com/6000.1/Documentation/ScriptReference/SerializedProperty.html
    SerializedProperty neighborsProp;

    void OnEnable()
    {
        // Setup the SerializedProperties.
        neighborsProp = serializedObject.FindProperty("neighbors");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        BasicNode node = (BasicNode)target;

        // Store the previous neighbors
        var previousNeighbors = new List<BasicNode>(node.Neighbors);

        bool changed = DrawDefaultInspector();

        // Update other node's neighbor list
        if (changed)
        {
            // Not too worried about performance issues since this should only be happening in editor on node add 
            // But if we start to get performance spikes in that specific case we should like just make a public setter for
            // neighbors even though it's bad practice imo.
            // Loop through all neighbors to see if previousNeighbors had them
            for (int i = 0; i < neighborsProp.arraySize; i++)
            {
                // Get neighbor we are looking at
                BasicNode neighbor = neighborsProp
                    .GetArrayElementAtIndex(i)
                    .objectReferenceValue as BasicNode;

                // If previous neighbors did not have them add node to the neighbor's list of neighbors.
                if (neighbor != null && !previousNeighbors.Contains(neighbor))
                {
                    // Create a SerializedObject for the neighbor
                    SerializedObject neighborObject = new SerializedObject(neighbor);

                    neighborObject.Update();

                    // Access its private neighbors field
                    SerializedProperty neighborNeighbors =
                        neighborObject.FindProperty("neighbors");

                    // Do not add this node again to the list of neighbors 
                    bool alreadyAdded = false;
                    for (int j = 0; j < neighborNeighbors.arraySize; j++)
                    {
                        if (neighborNeighbors
                        .GetArrayElementAtIndex(j)
                        .objectReferenceValue as BasicNode == target)
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }
                    // No changes to neighborObject so no need to applymodifiedproperties
                    if (alreadyAdded) { break; }

                    // Add the current node to its neighbors list
                    int index = neighborNeighbors.arraySize;
                    // Value initially undefined just resizes. 
                    neighborNeighbors.InsertArrayElementAtIndex(index);
                    neighborNeighbors.GetArrayElementAtIndex(index).objectReferenceValue = node;

                    neighborObject.ApplyModifiedProperties();
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}