using System.Collections.Generic;
using UnityEngine;

// UTF-8 설정
[CreateAssetMenu(fileName = "New Buliding", menuName = "Inventory/Buliding")]

public class Building : ScriptableObject
{
    public string type = "Building";
    public string scienceName = "basic";
    public Item item = null;
    public GameObject gameObj = null;
    public GameObject sideObj = null;

    public bool dragCancel;
    public bool isGetAnim;
    public bool isGetDirection;
    public bool isUnderObj;
    public bool isEnergyUse;
    public bool isEnergyStr;

    public List<Sprite> sprites = new List<Sprite>();
    public List<RuntimeAnimatorController> animatorController = new List<RuntimeAnimatorController>();
    public int level = 0;

    public int height = 0;
    public int width = 0;
    public int dirCount = 0;
}
