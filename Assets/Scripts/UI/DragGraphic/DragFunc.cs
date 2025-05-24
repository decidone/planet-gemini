using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragFunc : MonoBehaviour
{
    protected int interactLayer;

    public GameObject[] selectedObjects;
    protected GameManager gameManager;
    protected SoundManager soundManager;

    protected virtual void Start()
    {
        gameManager = GameManager.instance;
        soundManager = SoundManager.instance;
        interactLayer = LayerMask.NameToLayer("Interact");
    }

    public virtual void LeftMouseUp(Vector2 startPos, Vector2 endPos) { }
    public virtual void RightMouseUp(Vector2 startPos, Vector2 endPos) { }

    protected virtual void GroupSelectedObjects(Vector2 startPosition, Vector2 endPosition) { }
}
