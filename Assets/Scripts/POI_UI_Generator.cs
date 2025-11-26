using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class POI_UI_Generator : MonoBehaviour
{
    public POIManager poiManager;
    public GameObject buttonPrefab;
    public Transform contentPanel;
    public NavigationManager nav;

    void Start()
    {
        GenerateButtons();
    }

    public void GenerateButtons()
    {
        foreach (Transform c in contentPanel)
            Destroy(c.gameObject);

        foreach (var poi in poiManager.POIs)   // ✔ FIXED HERE
        {
            GameObject btn = Instantiate(buttonPrefab, contentPanel);
            btn.transform.localScale = Vector3.one;

            TMP_Text tmp = btn.GetComponentInChildren<TMP_Text>();
            tmp.text = poi.name;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                nav.NavigateTo(poi.name);     // ✔ NAVIGATION WORKS
            });
        }
    }
}
