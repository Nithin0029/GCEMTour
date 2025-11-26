using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NavigationGraph : MonoBehaviour
{
    public List<NavigationNode> nodes = new List<NavigationNode>();

    public NavigationNode GetNearestNode(Vector3 position)
    {
        NavigationNode best = null;
        float bestDist = Mathf.Infinity;

        foreach (var n in nodes)
        {
            float dist = Vector3.Distance(position, n.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = n;
            }
        }
        return best;
    }

    public List<NavigationNode> FindPath(NavigationNode start, NavigationNode goal)
    {
        Queue<NavigationNode> queue = new Queue<NavigationNode>();
        Dictionary<NavigationNode, NavigationNode> cameFrom = new Dictionary<NavigationNode, NavigationNode>();

        queue.Enqueue(start);
        cameFrom[start] = null;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (var nb in current.neighbours)
            {
                if (!cameFrom.ContainsKey(nb))
                {
                    queue.Enqueue(nb);
                    cameFrom[nb] = current;
                }
            }
        }

        return null;
    }

    List<NavigationNode> ReconstructPath(Dictionary<NavigationNode, NavigationNode> cameFrom,
                                         NavigationNode cur)
    {
        List<NavigationNode> path = new List<NavigationNode>();
        while (cur != null)
        {
            path.Add(cur);
            cur = cameFrom[cur];
        }
        path.Reverse();
        return path;
    }
}
