using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DuplicateCollidersTool : EditorWindow
{
    [MenuItem("Tools/Duplicate Colliders to New Objects")]
    static void DuplicateColliders()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select a parent GameObject first.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("Confirm Duplication",
            $"This will create NEW GameObjects containing only colliders for all children of '{selected.name}'.\n\nOriginal objects will remain UNCHANGED.\n\nContinue?", "Yes", "Cancel"))
        {
            return;
        }

        Undo.RecordObject(selected, "Duplicate Colliders");
        DuplicateCollidersFromHierarchy(selected);
        EditorUtility.ClearProgressBar();
        Debug.Log($"Duplication complete for '{selected.name}'");
    }

    static void DuplicateCollidersFromHierarchy(GameObject root)
    {
        // Create a parent to hold the new clean collider objects
        GameObject colliderParent = new GameObject("ExtractedColliders");
        colliderParent.transform.SetParent(root.transform, false);
        Undo.RegisterCreatedObjectUndo(colliderParent, "Create Collider Parent");

        // Get all children (snapshot list to avoid modification issues)
        List<Transform> children = new List<Transform>();
        foreach (Transform child in root.transform)
        {
            children.Add(child);
        }

        int total = children.Count;
        int processed = 0;

        foreach (Transform child in children)
        {
            EditorUtility.DisplayProgressBar("Duplicating Colliders", $"Processing: {child.name}", (float)processed / total);
            processed++;

            Collider[] colliders = child.GetComponents<Collider>();
            if (colliders.Length == 0) continue; // Skip if no collider

            // For each collider on this child, create a new clean GameObject
            foreach (Collider originalCollider in colliders)
            {
                // Create new GameObject with descriptive name
                GameObject newColliderGO = new GameObject($"{child.name}_{originalCollider.GetType().Name}");

                // CRITICAL: Match the transform EXACTLY
                // We parent to the SAME parent as the original to preserve local transform values
                newColliderGO.transform.SetParent(child.parent, false);

                // Copy local transform (preserves exact position/rotation/scale relative to parent)
                newColliderGO.transform.localPosition = child.localPosition;
                newColliderGO.transform.localRotation = child.localRotation;
                newColliderGO.transform.localScale = child.localScale;

                // Copy the collider component with all its properties
                CopyCollider(originalCollider, newColliderGO);

                Undo.RegisterCreatedObjectUndo(newColliderGO, "Duplicate Collider");
            }
        }

        // Select the new collider parent for easy inspection
        Selection.activeGameObject = colliderParent;
    }

    static void CopyCollider(Collider source, GameObject targetGO)
    {
        // Copy common properties first
        bool isTrigger = source.isTrigger;
        PhysicsMaterial material = source.sharedMaterial;
        string tag = source.tag;
        int layer = source.gameObject.layer;

        // Type-specific copying
        switch (source)
        {
            case BoxCollider box:
                BoxCollider newBox = Undo.AddComponent<BoxCollider>(targetGO);
                newBox.center = box.center;
                newBox.size = box.size;
                newBox.isTrigger = isTrigger;
                newBox.sharedMaterial = material;
                break;

            case SphereCollider sphere:
                SphereCollider newSphere = Undo.AddComponent<SphereCollider>(targetGO);
                newSphere.center = sphere.center;
                newSphere.radius = sphere.radius;
                newSphere.isTrigger = isTrigger;
                newSphere.sharedMaterial = material;
                break;

            case CapsuleCollider cap:
                CapsuleCollider newCap = Undo.AddComponent<CapsuleCollider>(targetGO);
                newCap.center = cap.center;
                newCap.radius = cap.radius;
                newCap.height = cap.height;
                newCap.direction = cap.direction;
                newCap.isTrigger = isTrigger;
                newCap.sharedMaterial = material;
                break;

            case MeshCollider mesh:
                MeshCollider newMesh = Undo.AddComponent<MeshCollider>(targetGO);
                newMesh.sharedMesh = mesh.sharedMesh; // Reference the same mesh asset
                newMesh.convex = mesh.convex;
                newMesh.cookingOptions = mesh.cookingOptions;
                newMesh.isTrigger = isTrigger;
                newMesh.sharedMaterial = material;
                break;

            case TerrainCollider terrain:
                // Terrain colliders are special; just copy reference
                TerrainCollider newTerrain = Undo.AddComponent<TerrainCollider>(targetGO);
                newTerrain.terrainData = terrain.terrainData;
                newTerrain.isTrigger = isTrigger;
                break;

            default:
                Debug.LogWarning($"Unsupported collider type: {source.GetType().Name} on {source.gameObject.name}");
                break;
        }

        // Apply common settings
        targetGO.tag = tag;
        targetGO.layer = layer;
    }
}