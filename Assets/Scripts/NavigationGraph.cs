using System.Collections.Generic;
using UnityEngine;

public class NavigationGraph : MonoBehaviour
{
    public List<NavigationNode> nodes = new List<NavigationNode>();

    public int GetNodeIndexFromPOI(Transform poi)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].linkedPOI == poi)
                return i;
        }
        return -1;
    }

    // BFS for shortest path
    public List<int> FindPath(int startIndex, int endIndex)
    {
        Queue<int> queue = new Queue<int>();
        Dictionary<int, int> visited = new Dictionary<int, int>();

        queue.Enqueue(startIndex);
        visited[startIndex] = -1;

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();

            if (current == endIndex)
                return BuildPath(visited, endIndex);

            foreach (var neighbor in nodes[current].neighbors)
            {
                int nextIndex = nodes.IndexOf(neighbor);

                if (!visited.ContainsKey(nextIndex))
                {
                    visited[nextIndex] = current;
                    queue.Enqueue(nextIndex);
                }
            }
        }

        return new List<int>();
    }

    private List<int> BuildPath(Dictionary<int, int> visited, int endIndex)
    {
        List<int> path = new List<int>();
        int current = endIndex;

        while (current != -1)
        {
            path.Insert(0, current);
            current = visited[current];
        }

        return path;
    }
}
