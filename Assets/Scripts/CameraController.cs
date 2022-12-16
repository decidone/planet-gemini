using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera cam;
    public Transform target;
    public Vector3 offset;

    private float zoomSpeed = 5f;
    private float minZoom = 5f;
    private float maxZoom = 15f;
    private float currentZoom = 10f;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        currentZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    }

    void LateUpdate()
    {
        transform.position = target.position - offset;
        cam.orthographicSize = currentZoom;
    }
}
