using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    PixelPerfectCamera pixelPerfectCamera;
    int zoomLevel = 1;

    void Awake()
    {
        pixelPerfectCamera = Camera.main.GetComponent<PixelPerfectCamera>();
    }

    void Update()
    {
        float scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheelInput != 0)
        {
            zoomLevel += Mathf.RoundToInt(scrollWheelInput * 10);
            zoomLevel = Mathf.Clamp(zoomLevel, 1, 7);
            pixelPerfectCamera.refResolutionX = Mathf.FloorToInt(Screen.width / zoomLevel);
            pixelPerfectCamera.refResolutionY = Mathf.FloorToInt(Screen.height / zoomLevel);
        }
    }

    void LateUpdate()
    {
        transform.position = target.position - offset;
    }
}
