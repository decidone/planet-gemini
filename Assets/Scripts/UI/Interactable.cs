using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] float offsetX;
    [SerializeField] float offsetY;
    [SerializeField] GameObject interactIconPref;
    GameObject interactIcon;

    public void SpawnIcon()
    {
        Vector3 pos = transform.position;
        pos.x += offsetX;
        pos.y += offsetY;

        if (interactIcon == null)
            interactIcon = Instantiate(interactIconPref, pos, Quaternion.identity);
    }

    public void DespawnIcon()
    {
        if (interactIcon != null)
            Destroy(interactIcon);
    }
}
