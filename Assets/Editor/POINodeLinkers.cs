// File: Assets/Editor/POIToNodeLinker.cs
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class POIToNodeLinker
{
    [MenuItem("Tools/Nav/Link POIs To Nearest Node")]
    public static void LinkPOIsToNodes()
    {
        // Find POIs parent
        var poisParent = GameObject.Find("POIs")?.transform;
        if (poisParent == null)
        {
            EditorUtility.DisplayDialog("Link POIs", "Couldn't find a GameObject named 'POIs' in the scene. Please ensure your POIs are parented under a GameObject named POIs.", "OK");
            return;
        }

        // Find NavGraph
        var navGraphGO = GameObject.FindObjectsOfType<NavigationGraph>().FirstOrDefault();
        if (navGraphGO == null)
        {
            EditorUtility.DisplayDialog("Link POIs", "Couldn't find a NavigationGraph in the scene. Create one (GameObject with NavigationGraph component) and populate nodes first.", "OK");
            return;
        }
        var graph = navGraphGO;

        if (graph.nodes == null || graph.nodes.Count == 0)
        {
            EditorUtility.DisplayDialog("Link POIs", "NavigationGraph.nodes is empty. Please create nodes first (Tools -> Nav -> Create Nodes From POIs).", "OK");
            return;
        }

        int linked = 0;
        foreach (Transform poi in poisParent)
        {
            NavigationNode nearest = null;
            float bestDist = Mathf.Infinity;
            foreach (var node in graph.nodes)
            {
                if (node == null) continue;
                float d = Vector3.Distance(poi.position, node.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    nearest = node;
                }
            }

            if (nearest != null)
            {
                nearest.linkedPOI = poi;
                EditorUtility.SetDirty(nearest);
                linked++;
            }
        }

        EditorUtility.SetDirty(graph);
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

        EditorUtility.DisplayDialog("Link POIs", $"Linked {linked} POIs to nearest nodes. (Graph node count: {graph.nodes.Count})", "OK");
    }
}
