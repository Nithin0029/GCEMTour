using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class POIListManager : MonoBehaviour
{
    public Transform POIsParent;          // Parent that holds all POIs
    public GameObject POIButtonPrefab;    // The button prefab
    public Transform ContentParent;       // ScrollView → Viewport → Content
    public NavigationManager navManager;  // Reference to NavigationManager

    void Start()
    {
        GeneratePOIButtons();
    }

    void GeneratePOIButtons()
    {
        foreach (Transform poi in POIsParent)
        {
            GameObject btnObj = Instantiate(POIButtonPrefab, ContentParent);

            // Set button label
            TextMeshProUGUI label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            label.text = poi.name;

            // Button click
            Button btn = btnObj.GetComponent<Button>();
            Transform target = poi;  // local copy for lambda capture

            btn.onClick.AddListener(() =>
            {
                navManager.NavigateTo(target);
                Debug.Log("Navigating to: " + target.name);
            });
        }
    }
}
