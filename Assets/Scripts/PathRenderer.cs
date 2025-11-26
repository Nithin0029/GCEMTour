using System.Collections.Generic;
using UnityEngine;

public class PathRenderer : MonoBehaviour
{
    [Header("Arrow / Indicator Prefab")]
    public GameObject arrowPrefab;

    [Header("Spacing between arrows")]
    public float spacing = 0.5f;

    private List<GameObject> spawnedArrows = new List<GameObject>();

    public void RenderPath(List<Vector3> worldPoints)
    {
        ClearPath();

        for (int i = 0; i < worldPoints.Count - 1; i++)
        {
            Vector3 start = worldPoints[i];
            Vector3 end = worldPoints[i + 1];
            float dist = Vector3.Distance(start, end);

            int steps = Mathf.FloorToInt(dist / spacing);

            for (int s = 0; s <= steps; s++)
            {
                float t = (float)s / steps;
                Vector3 pos = Vector3.Lerp(start, end, t);

                Quaternion rot = Quaternion.LookRotation(end - start);

                GameObject arrow = Instantiate(arrowPrefab, pos, rot);
                spawnedArrows.Add(arrow);
            }
        }
    }

    public void ClearPath()
    {
        foreach (GameObject obj in spawnedArrows)
            Destroy(obj);

        spawnedArrows.Clear();
    }
}
