using System.Collections.Generic;
using UnityEngine;

public class POIManager : MonoBehaviour
{
    [System.Serializable]
    public class POIData
    {
        public string name;
        public Vector3 position;
    }

    public GameObject poiPrefab;

    public List<POIData> POIs = new List<POIData>();   // ✔ THIS MUST EXIST

    public List<POI> spawnedPOIs = new List<POI>();    // for reference

    void Start()
    {
        SpawnPOIs();
    }

    public void SpawnPOIs()
    {
        foreach (Transform t in transform)
            Destroy(t.gameObject);

        spawnedPOIs.Clear();

        foreach (var data in POIs)
        {
            GameObject p = Instantiate(poiPrefab, data.position, Quaternion.identity, transform);
            POI poi = p.GetComponent<POI>();
            poi.LabelText = data.name;
            poi.ApplyLabel();

            spawnedPOIs.Add(poi);
        }
    }

    public POI GetPOI(string name)
    {
        return spawnedPOIs.Find(x => x.LabelText == name);
    }
}
