using UnityEngine;
using System.Collections.Generic;

public class PlayerPathVisualizer : MonoBehaviour
{
    [SerializeField] private Color pathColor = Color.yellow;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private float yOffset = 0.51f; // Slightly above ground
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = pathColor;
        lineRenderer.endColor = pathColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.enabled = false;
    }

    public void ShowPath(List<Vector3> path)
    {
        if (path == null || path.Count < 2)
        {
            lineRenderer.enabled = false;
            return;
        }
        lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 pos = path[i] + Vector3.up * yOffset;
            lineRenderer.SetPosition(i, pos);
        }
        lineRenderer.enabled = true;
    }

    public void HidePath()
    {
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 0;
    }
}
