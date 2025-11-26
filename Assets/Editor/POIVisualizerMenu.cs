// File: Assets/Editor/AutoGenerateNodesFromPOIs.cs
// Place this file in an Editor folder (Assets/Editor).
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class AutoGenerateNodesFromPOIs
{
    [MenuItem("Tools/POI/Auto-generate nodes from POIs")]
    public static void GenerateNodes()
    {
        // Config
        const float clusterTolerance = 1.6f;        // Z/X tolerance to consider same "row/column"
        const float neighborDistance = 3.0f;        // Max distance for nodes to be neighbors
        const float poiAssignDistance = 2.0f;       // Max distance to assign a linkedPOI to a node
        const float dedupeDistance = 0.6f;          // Combine nodes closer than this

        // Find POIs parent
        var poisParent = GameObject.Find("POIs");
        if (poisParent == null)
        {
            Debug.LogError("[AutoGen] Could not find GameObject named 'POIs' in the scene.");
            return;
        }

        // Find NavGraph
        var navGraphGO = GameObject.Find("NavGraph");
        if (navGraphGO == null)
        {
            Debug.LogError("[AutoGen] Could not find GameObject named 'NavGraph'. Create it first.");
            return;
        }

        var navGraphComp = navGraphGO.GetComponent("NavigationGraph");
        if (navGraphComp == null)
        {
            Debug.LogError("[AutoGen] NavGraph GameObject does not contain a component named 'NavigationGraph'.");
            return;
        }

        // Collect POI transforms (ignore hidden / inactive if desired)
        var poiTransforms = new List<Transform>();
        for (int i = 0; i < poisParent.transform.childCount; i++)
            poiTransforms.Add(poisParent.transform.GetChild(i));

        if (poiTransforms.Count == 0)
        {
            Debug.LogError("[AutoGen] No POIs found as children of 'POIs'.");
            return;
        }

        // Build clusters by rounding Z (rows) and by rounding X (columns)
        // Helper to cluster by an axis value
        List<List<Transform>> ClusterByAxis(List<Transform> list, Func<Transform, float> axisSelector)
        {
            var clusters = new List<List<Transform>>();
            foreach (var t in list)
            {
                var val = axisSelector(t);
                var found = clusters.FirstOrDefault(c => Mathf.Abs(axisSelector(c[0]) - val) <= clusterTolerance);
                if (found == null)
                    clusters.Add(new List<Transform> { t });
                else
                    found.Add(t);
            }
            return clusters;
        }

        var rowClusters = ClusterByAxis(poiTransforms, t => t.position.z);
        var colClusters = ClusterByAxis(poiTransforms, t => t.position.x);

        var generatedPositions = new List<Vector3>();

        // For each row, sort by X and create midpoints between neighbors
        foreach (var row in rowClusters)
        {
            var sorted = row.OrderBy(t => t.position.x).ToList();
            for (int i = 0; i < sorted.Count - 1; i++)
            {
                var a = sorted[i].position;
                var b = sorted[i + 1].position;
                var midpoint = Vector3.Lerp(a, b, 0.5f);
                midpoint.y = 0f; // keep nodes on ground; change if your POIs use different Y
                generatedPositions.Add(midpoint);
            }
        }

        // For each column, sort by Z and create midpoints between neighbors
        foreach (var col in colClusters)
        {
            var sorted = col.OrderBy(t => t.position.z).ToList();
            for (int i = 0; i < sorted.Count - 1; i++)
            {
                var a = sorted[i].position;
                var b = sorted[i + 1].position;
                var midpoint = Vector3.Lerp(a, b, 0.5f);
                midpoint.y = 0f;
                generatedPositions.Add(midpoint);
            }
        }

        // Also add POI-proximal nodes (one near each POI but offset into corridor):
        // For each POI, find nearest other POI and create a node slightly towards midpoint
        foreach (var poi in poiTransforms)
        {
            Transform nearest = null;
            float best = float.MaxValue;
            foreach (var other in poiTransforms)
            {
                if (other == poi) continue;
                var d = Vector3.Distance(poi.position, other.position);
                if (d < best) { best = d; nearest = other; }
            }
            if (nearest != null && best > 0.5f)
            {
                var mid = Vector3.Lerp(poi.position, nearest.position, 0.5f);
                mid.y = 0f;
                generatedPositions.Add(mid);
            }
        }

        // Deduplicate by merging positions that are very close
        var finalPositions = new List<Vector3>();
        foreach (var p in generatedPositions)
        {
            if (!finalPositions.Any(fp => Vector3.Distance(fp, p) < dedupeDistance))
                finalPositions.Add(p);
        }

        Debug.Log($"[AutoGen] Generated {finalPositions.Count} candidate node positions.");

        // Clear existing navgraph nodes under navGraphGO
        // Remove child nodes first (so scene is clean)
        var children = new List<GameObject>();
        for (int i = 0; i < navGraphGO.transform.childCount; i++)
            children.Add(navGraphGO.transform.GetChild(i).gameObject);
        foreach (var c in children) GameObject.DestroyImmediate(c);

        // Prepare list to assign into NavigationGraph component via reflection or direct field
        var createdNodes = new List<Component>();

        // For each final position, create a GameObject with NavigationNode component
        for (int i = 0; i < finalPositions.Count; i++)
        {
            var pos = finalPositions[i];
            var go = new GameObject($"Node_{i}");
            go.transform.SetParent(navGraphGO.transform, false);
            go.transform.position = pos;

            // Add NavigationNode component
            var navNode = go.AddComponent(Type.GetType("NavigationNode, Assembly-CSharp"));
            if (navNode == null)
            {
                // fallback by searching assemblies
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in assemblies)
                {
                    var t = asm.GetType("NavigationNode");
                    if (t != null)
                    {
                        navNode = (Component)go.AddComponent(t);
                        break;
                    }
                }
            }
            if (navNode == null)
            {
                Debug.LogError("[AutoGen] Could not find NavigationNode type. Make sure NavigationNode.cs exists and compiles.");
                GameObject.DestroyImmediate(go);
                return;
            }

            // optional: set icon so nodes are visible in Scene (Editor only)
#if UNITY_EDITOR
            var icon = EditorGUIUtility.ObjectContent(null, typeof(GameObject)).image;
            // set a small dot icon if desired - using default icon here
#endif

            createdNodes.Add(navNode);
        }

        // Now assign NavigationGraph.nodes field (supports List<NavigationNode> or NavigationNode[])
        var navGraphType = navGraphComp.GetType();
        var nodesField = navGraphType.GetField("nodes");
        if (nodesField == null)
        {
            Debug.LogWarning("[AutoGen] NavigationGraph has no public field 'nodes'. Attempting property 'Nodes'.");
            nodesField = null;
        }

        if (nodesField != null)
        {
            // Create a List of the correct element type
            var elementType = nodesField.FieldType.IsArray ? nodesField.FieldType.GetElementType() : nodesField.FieldType.GenericTypeArguments.FirstOrDefault();
            if (elementType == null)
            {
                Debug.LogWarning("[AutoGen] Unable to determine NavigationGraph.nodes element type. Will skip assigning field.");
            }
            else
            {
                // If field is array
                if (nodesField.FieldType.IsArray)
                {
                    var arr = Array.CreateInstance(elementType, createdNodes.Count);
                    for (int i = 0; i < createdNodes.Count; i++)
                        arr.SetValue(createdNodes[i], i);
                    nodesField.SetValue(navGraphComp, arr);
                }
                else
                {
                    // assume List<T>
                    var listType = typeof(List<>).MakeGenericType(elementType);
                    var listInstance = Activator.CreateInstance(listType);
                    var addMethod = listType.GetMethod("Add");
                    for (int i = 0; i < createdNodes.Count; i++)
                        addMethod.Invoke(listInstance, new object[] { createdNodes[i] });
                    nodesField.SetValue(navGraphComp, listInstance);
                }
            }
        }
        else
        {
            Debug.LogWarning("[AutoGen] Could not set navGraph.nodes automatically. The graph class may not expose 'nodes' publicly. Nodes are still created as children under NavGraph.");
        }

        // Now attempt to fill NavigationNode.linkedPOI and NavigationNode.neighbors by reflection
        // We'll cache types/fields we need
        Type navNodeType = createdNodes.Count > 0 ? createdNodes[0].GetType() : null;
        var linkedPOIField = navNodeType?.GetField("LinkedPOI") ?? navNodeType?.GetField("linkedPOI") ?? navNodeType?.GetField("linkedPoi") ?? navNodeType?.GetField("linked");
        // fallback to property
        var linkedPOIProperty = navNodeType?.GetProperty("LinkedPOI") ?? navNodeType?.GetProperty("linkedPOI");

        var neighborsField = navNodeType?.GetField("neighbors") ?? navNodeType?.GetField("Neighbors");
        var neighborsProp = navNodeType?.GetProperty("neighbors") ?? navNodeType?.GetProperty("Neighbors");

        // Build node list of Components to facilitate neighbor linking
        for (int i = 0; i < createdNodes.Count; i++)
        {
            var nodeComp = createdNodes[i];
            var nodePos = nodeComp.transform.position;

            // assign nearest POI within threshold
            Transform nearestPOI = null;
            float bestD = float.MaxValue;
            foreach (var p in poiTransforms)
            {
                var d = Vector3.Distance(p.position, nodePos);
                if (d < bestD) { bestD = d; nearestPOI = p; }
            }
            if (nearestPOI != null && bestD <= poiAssignDistance)
            {
                if (linkedPOIField != null)
                {
                    // assign transform if field expects Transform or GameObject
                    if (linkedPOIField.FieldType == typeof(Transform)) linkedPOIField.SetValue(nodeComp, nearestPOI);
                    else if (linkedPOIField.FieldType == typeof(GameObject)) linkedPOIField.SetValue(nodeComp, nearestPOI.gameObject);
                    else linkedPOIField.SetValue(nodeComp, nearestPOI);
                }
                else if (linkedPOIProperty != null)
                {
                    linkedPOIProperty.SetValue(nodeComp, nearestPOI);
                }
            }

            // build neighbor list (list of NavigationNode components)
            var closeNodes = new List<Component>();
            for (int j = 0; j < createdNodes.Count; j++)
            {
                if (i == j) continue;
                var other = createdNodes[j];
                var d = Vector3.Distance(nodeComp.transform.position, other.transform.position);
                if (d <= neighborDistance) closeNodes.Add(other);
            }

            // Assign neighbors to field/property if possible
            if (neighborsField != null)
            {
                var fType = neighborsField.FieldType;
                if (fType.IsArray)
                {
                    var elemType = fType.GetElementType();
                    var arr = Array.CreateInstance(elemType, closeNodes.Count);
                    for (int k = 0; k < closeNodes.Count; k++) arr.SetValue(closeNodes[k], k);
                    neighborsField.SetValue(nodeComp, arr);
                }
                else if (fType.IsGenericType && fType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var elemType = fType.GetGenericArguments()[0];
                    var listType = typeof(List<>).MakeGenericType(elemType);
                    var listObj = Activator.CreateInstance(listType);
                    var addM = listType.GetMethod("Add");
                    foreach (var cn in closeNodes) addM.Invoke(listObj, new object[] { cn });
                    neighborsField.SetValue(nodeComp, listObj);
                }
                else
                {
                    Debug.LogWarning($"[AutoGen] neighbors field found but unsupported type: {fType}");
                }
            }
            else if (neighborsProp != null)
            {
                // try property assignment for List<Component>
                try
                {
                    neighborsProp.SetValue(nodeComp, closeNodes);
                }
                catch { /* ignore */ }
            }
        }

        // Final: mark scene dirty and report
        EditorUtility.SetDirty(navGraphGO);
        foreach (var c in createdNodes) EditorUtility.SetDirty(c.gameObject);

        Debug.Log($"[AutoGen] Created {createdNodes.Count} nodes under NavGraph and attempted neighbor/link assignment.");
        Debug.Log("[AutoGen] Please inspect NavGraph children and NavigationNode components. Adjust neighborDistance/poiAssignDistance in the script if you need different connectivity.");
    }
}
