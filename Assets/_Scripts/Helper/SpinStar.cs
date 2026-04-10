using UnityEngine;
using System.Collections.Generic;

public class SpinStar : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 90f; // Degrees per second

    private void Update()
    {
        transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
    }
}