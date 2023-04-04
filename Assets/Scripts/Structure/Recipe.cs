using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recipe : MonoBehaviour
{
    public string name;
    public int space;
    public List<Item> items;
    public List<int> amounts;
    public float cooldown;

    public Recipe() { }

    public Recipe(string _name, int _space, List<Item> _items, List<int> _amounts, float _cooldown)
    {
        name = _name;
        space = _space;
        items = _items;
        amounts = _amounts;
        cooldown = _cooldown;
    }
}
