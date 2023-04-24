using UnityEngine;

[CreateAssetMenu(fileName = "New Buliding", menuName = "Inventory/Buliding")]

public class Building : ScriptableObject
{
    public Item item = null;
    public GameObject gameObj = null;
}
