using UnityEngine;
using System.Collections.Generic;

public class NavigationNode : MonoBehaviour
{
    public Transform linkedPOI;  // The POI this node belongs to
    public List<NavigationNode> neighbors = new List<NavigationNode>();
}
