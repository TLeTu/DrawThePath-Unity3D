using UnityEngine;
using System.Collections.Generic;

public class PlayerPathVisualizer : MonoBehaviour
{
    [SerializeField] private Color _pathColor = Color.yellow;
    [SerializeField] private float _lineWidth = 0.1f;
    [SerializeField] private float _yOffset = 0.51f; // Slightly above ground
    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = _pathColor;
        _lineRenderer.endColor = _pathColor;
        _lineRenderer.startWidth = _lineWidth;
        _lineRenderer.endWidth = _lineWidth;
        _lineRenderer.positionCount = 0;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.enabled = false;
    }

    public void ShowPath(List<Vector3> path)
    {
        if (path == null || path.Count < 2)
        {
            _lineRenderer.enabled = false;
            return;
        }
        _lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 pos = path[i] + Vector3.up * _yOffset;
            _lineRenderer.SetPosition(i, pos);
        }
        _lineRenderer.enabled = true;
    }

    public void HidePath()
    {
        _lineRenderer.enabled = false;
        _lineRenderer.positionCount = 0;
    }
}
