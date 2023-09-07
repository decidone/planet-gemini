using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragFunc : MonoBehaviour
{
    public GameObject[] selectedObjects;

    public virtual void LeftMouseUp(Vector2 startPos, Vector2 endPos) { }
    protected virtual List<GameObject> GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition, int layer)
    {
        Collider2D[] colliders = Physics2D.OverlapAreaAll(startPosition, endPosition, 1 << layer);

        List<GameObject> selectedObjectsList = new List<GameObject>();

        foreach (Collider2D collider in colliders)
        {
            selectedObjectsList.Add(collider.gameObject);
        }

        return selectedObjectsList;
    }
}
