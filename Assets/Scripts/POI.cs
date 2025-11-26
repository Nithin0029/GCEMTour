using UnityEngine;
using TMPro;

public class POI : MonoBehaviour
{
    [Header("Assigned from Prefab")]
    public TextMeshPro label;     // The 3D label object
    public string LabelText;      // Name to display

    [HideInInspector]
    public Vector3 position;      // Used for navigation

    void Awake()
    {
        position = transform.position;
    }

    public void ApplyLabel()
    {
        if (label != null)
        {
            label.text = LabelText;
        }
        else
        {
            Debug.LogWarning("Label reference missing on POI: " + gameObject.name);
        }
    }
}
