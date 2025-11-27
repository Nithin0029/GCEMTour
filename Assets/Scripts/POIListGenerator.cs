using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class POIListGenerator : MonoBehaviour
{
    public Transform poiParent;
    public Transform bottomDrawer;
    public GameObject buttonPrefab;
    public NavigationManager navManager;

    void Start()
    {
        foreach (Transform poi in poiParent)
        {
            GameObject btn = Instantiate(buttonPrefab, bottomDrawer);
            btn.GetComponentInChildren<TMP_Text>().text = poi.name;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                navManager.NavigateTo(poi);   // FIXED 🔥
            });
        }
    }
}
