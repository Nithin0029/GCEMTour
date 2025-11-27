using System.Collections.Generic;
using UnityEngine;

public class PathRenderer : MonoBehaviour
{
    [Header("Arrow Settings")]
    public GameObject arrowPrefab;     // the arrow to place on path
    public float arrowSpacing = 1.0f;  // spacing between arrows

    private List<GameObject> spawnedArrows = new List<GameObject>();

    /// <summary>
    /// Renders the navigation path using arrow prefabs.
    /// </summary>
    public void RenderPath(List<Vector3> points)
    {
        ClearPath();

        if (points == null || points.Count < 2)
            return;

        Vector3 previous = points[0];

        for (int i = 1; i < points.Count; i++)
        {
            Vector3 current = points[i];

            // Compute total distance between nodes
            float distance = Vector3.Distance(previous, current);

            // Number of arrows between the 2 graph nodes
            int arrowCount = Mathf.FloorToInt(distance / arrowSpacing);

            for (int a = 0; a <= arrowCount; a++)
            {
                float t = (float)a / arrowCount;
                Vector3 position = Vector3.Lerp(previous, current, t);

                // Spawn arrow
                GameObject arrow = Instantiate(arrowPrefab, position, Quaternion.identity, transform);

                // Rotate arrow to face next point
                Vector3 direction = (current - previous).normalized;
                if (direction != Vector3.zero)
                    arrow.transform.rotation = Quaternion.LookRotation(direction);

                spawnedArrows.Add(arrow);
            }

            previous = current;
        }
    }

    /// <summary>
    /// Removes previously spawned arrows.
    /// </summary>
    public void ClearPath()
    {
        foreach (GameObject arrow in spawnedArrows)
        {
            if (arrow != null)
                Destroy(arrow);
        }

        spawnedArrows.Clear();
    }
}
