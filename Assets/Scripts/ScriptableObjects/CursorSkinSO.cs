using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CursorSkinSO", menuName = "SOList/CursorSkinSO")]

public class CursorSkinSO : ScriptableObject
{
    public Texture2D baseCursor;
    public List<Texture2D> dragCursor;
    public Texture2D buildingCursor;
    public List<Texture2D> unitCursor;
}
