using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class RaycastUtility
{
    public static bool IsPointerOverUI(Vector2 screenPos)
    {
        var hit = RaycastCheck(ScreenPosToPointerData(screenPos));
        return hit != null && hit.layer == LayerMask.NameToLayer("UI");
    }

    private static GameObject RaycastCheck(PointerEventData pointerData)
    {
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results.Count < 1 ? null : results[0].gameObject;
    }

    static PointerEventData ScreenPosToPointerData(Vector2 screenPos)
       => new (EventSystem.current) { position = screenPos };
}