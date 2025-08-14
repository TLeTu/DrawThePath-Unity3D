using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Combines meshes of child cubes by material to reduce draw calls.
/// Attach this script to the parent GameObject containing all cube tiles as children.
/// </summary>
public class GridMeshCombiner : MonoBehaviour
{
    // Run automatically on Start
    public bool combineOnStart = true;

    void Start()
    {
        if (combineOnStart)
            CombineMeshesByMaterial();
    }

    /// <summary>
    /// Combines all child cube meshes by material.
    /// Can be called from the editor or at runtime.
    /// </summary>
    [ContextMenu("Combine Meshes By Material")]
    public void CombineMeshesByMaterial()
    {
        List<CombineInstance> combineList = new List<CombineInstance>();
        Material firstMaterial = null;
        string firstTag = "Untagged";
        bool foundMaterial = false;
        List<GameObject> cubesToDisable = new List<GameObject>();

        // Iterate over all child cubes
        foreach (Transform child in transform)
        {
            MeshFilter mf = child.GetComponent<MeshFilter>();
            MeshRenderer mr = child.GetComponent<MeshRenderer>();
            if (mf == null || mr == null || mf.sharedMesh == null)
                continue;

            CombineInstance ci = new CombineInstance();
            ci.mesh = mf.sharedMesh;
            ci.subMeshIndex = 0;
            ci.transform = child.localToWorldMatrix;
            combineList.Add(ci);

            if (!foundMaterial && mr.sharedMaterial != null)
            {
                firstMaterial = mr.sharedMaterial;
                firstTag = child.tag;
                foundMaterial = true;
            }
            cubesToDisable.Add(child.gameObject);
        }

        if (combineList.Count > 0 && firstMaterial != null)
        {
            GameObject combinedObj = new GameObject($"Combined_{firstMaterial.name}");
            combinedObj.transform.SetParent(transform, false);
            combinedObj.isStatic = true;

            Mesh combinedMesh = new Mesh();
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Support large meshes
            combinedMesh.CombineMeshes(combineList.ToArray(), true, true);

            MeshFilter mf = combinedObj.AddComponent<MeshFilter>();
            mf.sharedMesh = combinedMesh;

            MeshRenderer mr = combinedObj.AddComponent<MeshRenderer>();
            mr.sharedMaterial = firstMaterial;

            // Add a MeshCollider for click/touch detection
            MeshCollider meshCol = combinedObj.AddComponent<MeshCollider>();
            meshCol.sharedMesh = combinedMesh;

            // Set the tag to match the cubes' tag
            combinedObj.tag = firstTag;
        }

        // Disable original cubes
        foreach (var go in cubesToDisable)
        {
            go.SetActive(false);
        }
    }
}
