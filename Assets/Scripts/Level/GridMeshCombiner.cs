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
        // Dictionary to group mesh instances by material
        Dictionary<Material, List<CombineInstance>> materialToMesh = new Dictionary<Material, List<CombineInstance>>();
        // Store original cubes to disable later
        List<GameObject> cubesToDisable = new List<GameObject>();

        // Iterate over all child cubes
        foreach (Transform child in transform)
        {
            MeshFilter mf = child.GetComponent<MeshFilter>();
            MeshRenderer mr = child.GetComponent<MeshRenderer>();
            if (mf == null || mr == null || mf.sharedMesh == null)
                continue;

            // For each material on the renderer
            for (int i = 0; i < mr.sharedMaterials.Length; i++)
            {
                Material mat = mr.sharedMaterials[i];
                if (!materialToMesh.ContainsKey(mat))
                    materialToMesh[mat] = new List<CombineInstance>();

                CombineInstance ci = new CombineInstance();
                ci.mesh = mf.sharedMesh;
                ci.subMeshIndex = i < mf.sharedMesh.subMeshCount ? i : 0;
                ci.transform = child.localToWorldMatrix;
                materialToMesh[mat].Add(ci);
            }
            cubesToDisable.Add(child.gameObject);
        }

        // Create a combined mesh GameObject for each material
        foreach (var kvp in materialToMesh)
        {
            Material mat = kvp.Key;
            List<CombineInstance> combineList = kvp.Value;

            GameObject combinedObj = new GameObject($"Combined_{mat.name}");
            combinedObj.transform.SetParent(transform, false);
            combinedObj.isStatic = true;

            Mesh combinedMesh = new Mesh();
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Support large meshes
            combinedMesh.CombineMeshes(combineList.ToArray(), true, true);

            MeshFilter mf = combinedObj.AddComponent<MeshFilter>();
            mf.sharedMesh = combinedMesh;

            MeshRenderer mr = combinedObj.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
        }

        // Disable original cubes
        foreach (var go in cubesToDisable)
        {
            go.SetActive(false);
        }
    }
}
