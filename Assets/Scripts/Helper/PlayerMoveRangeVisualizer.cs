using UnityEngine;
using System.Collections.Generic;

public class PlayerMoveRangeVisualizer : MonoBehaviour
{
    [SerializeField] private Color highlightColor = new Color(0, 1, 0, 0.3f);
    [SerializeField] private int maxMoveDistance = 4;
    [SerializeField] private GameObject highlightPrefab; // Optional: assign a transparent quad or similar

    private List<GameObject> highlights = new List<GameObject>();

    public void ShowMoveRange(Vector2Int playerCoords)
    {
        ClearHighlights();
        if (GridManager.Instance == null) return;
        int width = GridManager.Instance.gridWidth;
        int height = GridManager.Instance.gridHeight;
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                int dRow = Mathf.Abs(playerCoords.x - row);
                int dCol = Mathf.Abs(playerCoords.y - col);
                // Only straight lines, up to maxMoveDistance
                if (((dRow == 0 && dCol > 0 && dCol <= maxMoveDistance) || (dCol == 0 && dRow > 0 && dRow <= maxMoveDistance))
                    && GridManager.Instance.IsWalkable(row, col))
                {
                    Vector3 pos = GridManager.Instance.GridToWorld(new Vector2Int(row, col));
                    CreateHighlightAt(pos);
                }
            }
        }
    }

    private void CreateHighlightAt(Vector3 position)
    {
        if (highlightPrefab != null)
        {
            GameObject go = Instantiate(highlightPrefab, position, Quaternion.identity);
            go.transform.SetParent(transform);
            highlights.Add(go);
        }
        else
        {
            // Draw a simple gizmo if no prefab is assigned
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.transform.position = position + Vector3.up * 1.01f; // Slightly above ground
            go.transform.rotation = Quaternion.Euler(90, 0, 0); // Face up for top-down view
            go.transform.localScale = Vector3.one * GridManager.Instance.GetTileSize();
            go.layer = 2; // Ignore Raycast layer
            var renderer = go.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Unlit/Color"));
            renderer.material.color = highlightColor;
            go.transform.SetParent(transform);
            highlights.Add(go);
        }
    }

    public void ClearHighlights()
    {
        foreach (var go in highlights)
        {
            if (go != null) Destroy(go);
        }
        highlights.Clear();
    }
}
