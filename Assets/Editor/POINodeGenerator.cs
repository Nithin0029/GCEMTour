// File: Assets/Editor/POINodeGenerator.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public static class POINodeGenerator
{
    // Menu: select NavGraph in Hierarchy, then run Tools -> Nav -> Create Nodes From POIs
    [MenuItem("Tools/Nav/Create Nodes From POIs")]
    public static void CreateNodesFromPOIs()
    {
        // Find POIs parent
        var poisParent = GameObject.Find("POIs")?.transform;
        if (poisParent == null)
        {
            EditorUtility.DisplayDialog("Create Nodes", "Couldn't find a GameObject named 'POIs' in the scene. Please ensure your POIs are parented under a GameObject named POIs.", "OK");
            return;
        }

        // Find selected NavGraph or create one
        GameObject navGraphGO = Selection.activeGameObject;
        if (navGraphGO == null || navGraphGO.GetComponent<NavigationGraph>() == null)
        {
            // Offer to create NavGraph
            if (!EditorUtility.DisplayDialog("NavGraph not selected", "No NavGraph selected. Create a new NavGraph GameObject in the scene?", "Create", "Cancel"))
                return;

            navGraphGO = new GameObject("NavGraph");
            navGraphGO.AddComponent<NavigationGraph>();
            Undo.RegisterCreatedObjectUndo(navGraphGO, "Create NavGraph");
        }

        var navGraph = navGraphGO.GetComponent<NavigationGraph>();

        // Options dialog
        if (!EditorUtility.DisplayDialog("Create Nodes", "This will create one NavigationNode per POI position and optional midpoint nodes between close POIs. Continue?", "Yes", "Cancel"))
            return;

        // Parameters (you can tweak here)
        float midpointMaxDistance = 6f;  // if two POIs are closer than this, create a midpoint node between them
        float neighborConnectDist = 4f;  // nodes within this distance will be connected as neighbors

        // Create a container under NavGraph
        Transform container = navGraphGO.transform.Find("NodesContainer");
        if (container == null)
        {
            GameObject cont = new GameObject("NodesContainer");
            cont.transform.SetParent(navGraphGO.transform, false);
            container = cont.transform;
            Undo.RegisterCreatedObjectUndo(cont, "Create NodesContainer");
        }

        var createdNodes = new List<NavigationNode>();

        // 1) Create node at each POI
        foreach (Transform poi in poisParent)
        {
            GameObject nodeGO = new GameObject("Node_POI_" + poi.name);
            Undo.RegisterCreatedObjectUndo(nodeGO, "Create Node");
            nodeGO.transform.SetParent(container, false);
            nodeGO.transform.position = poi.position;
            var n = nodeGO.AddComponent<NavigationNode>();
            n.linkedPOI = poi;
            createdNodes.Add(n);
        }

        // 2) Create midpoints between POIs that are closer than midpointMaxDistance
        var poiTransforms = poisParent.Cast<Transform>().ToArray();
        for (int i = 0; i < poiTransforms.Length; i++)
        {
            for (int j = i + 1; j < poiTransforms.Length; j++)
            {
                float d = Vector3.Distance(poiTransforms[i].position, poiTransforms[j].position);
                if (d > 0.001f && d <= midpointMaxDistance)
                {
                    Vector3 mid = (poiTransforms[i].position + poiTransforms[j].position) * 0.5f;
                    // avoid duplicates: check if a node already exists close to this midpoint
                    if (!createdNodes.Any(x => Vector3.Distance(x.transform.position, mid) < 0.5f))
                    {
                        GameObject nodeGO = new GameObject("Node_mid_" + i + "_" + j);
                        Undo.RegisterCreatedObjectUndo(nodeGO, "Create Midpoint Node");
                        nodeGO.transform.SetParent(container, false);
                        nodeGO.transform.position = mid;
                        var n = nodeGO.AddComponent<NavigationNode>();
                        createdNodes.Add(n);
                    }
                }
            }
        }

        // 3) Optionally connect nodes that are within neighborConnectDist
        foreach (var n in createdNodes)
        {
            n.neighbors = new List<NavigationNode>();
            foreach (var m in createdNodes)
            {
                if (m == n) continue;
                float dd = Vector3.Distance(n.transform.position, m.transform.position);
                if (dd <= neighborConnectDist)
                {
                    if (!n.neighbors.Contains(m)) n.neighbors.Add(m);
                }
            }
        }

        // 4) Assign to NavigationGraph.nodes list
        navGraph.nodes = createdNodes.ToList();

        // Mark scene dirty and focus
        EditorUtility.SetDirty(navGraph);
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

        EditorUtility.DisplayDialog("Create Nodes", $"Created {createdNodes.Count} nodes under {navGraphGO.name}. Nodes were auto-connected by distance {neighborConnectDist}.", "OK");
    }
}
