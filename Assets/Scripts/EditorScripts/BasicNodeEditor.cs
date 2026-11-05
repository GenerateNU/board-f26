using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

// Creates a custom Label on the inspector for all the scripts named ScriptName, true is so inherited classes also get this
[CustomEditor(typeof(BasicNode), true)]
public class BasicNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        /*
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "neighbors"); there is literally no documentation on this other than 1 Unity Hub question post 
        serializedObject.ApplyModifiedProperties();
        */
        var node = (BasicNode)target;

        var previousNeighbors = new List<BasicNode>(node.Neighbors);

        bool changed = DrawDefaultInspector();

        if (changed)
        {
            foreach (var neighbor in node.Neighbors)
            {
                if (neighbor != null && !previousNeighbors.Contains(neighbor))
                {
                    neighbor.AddNeighbor(node);
                }
            }
        }
        //GUILayout.Label("This is a Label in a Custom Editor");
    }
}