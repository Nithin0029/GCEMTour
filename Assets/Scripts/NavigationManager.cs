using System.Collections.Generic;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager Instance;

    [Header("References")]
    public NavigationGraph graph;        // Your graph object
    public PathRenderer pathRenderer;    // Handles arrow spawning

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Called when user selects a POI from ScrollView
    /// </summary>
    public void NavigateTo(Transform destinationPOI)
    {
        if (graph == null)
        {
            Debug.LogError("NavigationGraph reference missing in NavigationManager!");
            return;
        }
        if (pathRenderer == null)
        {
            Debug.LogError("PathRenderer reference missing in NavigationManager!");
            return;
        }

        // Step 1 — Find nearest node to the user (AR Camera)
        int startIndex = FindNearestNode(Camera.main.transform.position);

        // Step 2 — Find the graph node connected to this POI
        int endIndex = graph.GetNodeIndexFromPOI(destinationPOI);

        if (endIndex == -1)
        {
            Debug.LogError("No node linked to POI: " + destinationPOI.name);
            return;
        }

        // Step 3 — Get shortest path from Graph
        List<int> path = graph.FindPath(startIndex, endIndex);

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("No valid path found!");
            return;
        }

        // Step 4 — Convert node list to world positions
        List<Vector3> worldPoints = new List<Vector3>();
        foreach (int nodeIndex in path)
            worldPoints.Add(graph.nodes[nodeIndex].transform.position);

        // Step 5 — Render the path
        pathRenderer.RenderPath(worldPoints);
    }

    private int FindNearestNode(Vector3 userPosition)
    {
        float bestDist = Mathf.Infinity;
        int bestIndex = 0;

        for (int i = 0; i < graph.nodes.Count; i++)
        {
            float dist = Vector3.Distance(userPosition, graph.nodes[i].transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

}
