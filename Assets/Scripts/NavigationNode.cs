using System.Collections.Generic;
using UnityEngine;

public class NavigationNode : MonoBehaviour
{
    public Vector3 position;
    public List<NavigationNode> neighbours = new List<NavigationNode>();
}
