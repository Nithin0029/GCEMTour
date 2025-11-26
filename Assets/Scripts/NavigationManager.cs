using UnityEngine;
using System.Collections.Generic;

public class NavigationManager : MonoBehaviour
{
    public POIManager poiManager;
    public NavigationGraph graph;
    public LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer.positionCount = 0;
    }

    public void NavigateTo(string targetName)
    {
        POI targetPOI = poiManager.GetPOI(targetName);

        if (targetPOI == null)
        {
            Debug.LogError("POI NOT FOUND: " + targetName);
            return;
        }

        NavigationNode start = graph.GetNearestNode(Camera.main.transform.position);
        NavigationNode end = graph.GetNearestNode(targetPOI.transform.position);

        List<NavigationNode> path = graph.FindPath(start, end);

        DrawPath(path);
    }

    void DrawPath(List<NavigationNode> path)
    {
        if (path == null || path.Count == 0) return;

        lineRenderer.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
            lineRenderer.SetPosition(i, path[i].position);
    }
}
