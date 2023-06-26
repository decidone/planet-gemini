using UnityEngine;

[CreateAssetMenu(fileName = "New Buliding", menuName = "Inventory/Buliding")]

public class Building : ScriptableObject
{
    public string type = "Building";
    public string scienceName = "basic";
    public Item item = null;
    public GameObject gameObj = null;
    public int level = 0;

    //public float numRows = 0;
    //public float numColumns = 0;
}
