using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class AlphaCameraController : MonoBehaviour
{
    Camera targetCamera;
    public Camera alphaRenderCamera;
    public Transform mask;
    [SerializeField] Vector3 offset;

    #region Singleton
    public static AlphaCameraController instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    public void SetTargetCamera(Camera cam)
    {
        targetCamera = cam;
        transform.SetParent(targetCamera.transform);
        transform.localPosition = offset;
    }

    public void ChangeSize(PixelPerfectCamera pixelPerfectCam)
    {
        int scaleX = Screen.width / pixelPerfectCam.refResolutionX;
        int scaleY = Screen.height / pixelPerfectCam.refResolutionY;
        int scale = Mathf.Max(1, Mathf.Min(scaleX, scaleY));
        float size = (Screen.height * 0.5f) / (pixelPerfectCam.assetsPPU * scale);

        if (alphaRenderCamera != null)
        {
            alphaRenderCamera.orthographicSize = size;
        }

        if (mask != null)
        {
            mask.localScale = new Vector3(
                size * 2 / 9 * 16,
                size * 2,
                1
            );
        }
    }
}
