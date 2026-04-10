using UnityEngine;
using System.Collections.Generic;

public class PlayerPathVisualizer : MonoBehaviour
{
    [SerializeField] private Color _pathColor = Color.yellow;
    [SerializeField] private float _lineWidth = 0.1f;
    [SerializeField] private float _yOffset = 0.1f; 
    [SerializeField] private LayerMask _groundLayer; // Assign your "Environment" layer here!

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        // Use an Unlit shader so it glows nicely and doesn't get weird shadows
        _lineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        _lineRenderer.startColor = _pathColor;
        _lineRenderer.endColor = _pathColor;
        _lineRenderer.startWidth = _lineWidth;
        _lineRenderer.endWidth = _lineWidth;
        
        // FIX 1: Make the corners and ends rounded instead of pinched
        _lineRenderer.numCornerVertices = 5; 
        _lineRenderer.numCapVertices = 5;

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
            Vector3 pos = path[i];
            
            // FIX 2: Raycast down from high up to find the exact floor height of this specific tile
            Vector3 rayStart = new Vector3(pos.x, pos.y + 10f, pos.z);
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 20f, _groundLayer))
            {
                // Snap the point to the actual floor geometry, then add the offset
                pos.y = hit.point.y + _yOffset;
            }
            else
            {
                // Fallback just in case the raycast misses
                pos.y += _yOffset;
            }

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