using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragFunc : MonoBehaviour
{
    public GameObject[] selectedObjects;
    protected GameManager gameManager;

    protected virtual void Start()
    {
        gameManager = GameManager.instance;
    }

    public virtual void LeftMouseUp(Vector2 startPos, Vector2 endPos) { }
    public virtual void RightMouseUp(Vector2 startPos, Vector2 endPos) { }

    protected virtual void GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition, int layer)
    {
        Collider2D[] colliders = Physics2D.OverlapAreaAll(startPosition, endPosition, 1 << layer);

        List<GameObject> selectedObjectsList = new List<GameObject>();

        foreach (Collider2D collider in colliders)
        {
            if (layer == LayerMask.NameToLayer("Obj") && collider.GetComponent<Structure>() == null)
                continue;
            if (collider.GetComponent<Portal>())
                continue;
            selectedObjectsList.Add(collider.gameObject);
        }

        selectedObjects = selectedObjectsList.ToArray();
    }
}
