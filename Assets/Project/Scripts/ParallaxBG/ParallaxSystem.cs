using UnityEngine;
using System.Collections.Generic;

public sealed class ParallaxSystem : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private List<ParallaxLayer> layers = new();

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        Vector3 camPos = targetCamera.transform.position;

        foreach (var layer in layers)
            layer.Initialize(camPos);
    }

    private void LateUpdate()
    {
        Vector3 camPos = targetCamera.transform.position;

        foreach (var layer in layers)
            layer.Tick(camPos);
    }
}
