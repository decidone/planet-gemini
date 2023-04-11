using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recipe
{
    public string name;
    public List<Item> items;
    public List<int> amounts;
    public float cooldown;

    public Recipe() { }

    public Recipe(string _name, List<Item> _items, List<int> _amounts, float _cooldown)
    {
        name = _name;
        items = _items;
        amounts = _amounts;
        cooldown = _cooldown;
    }
}
