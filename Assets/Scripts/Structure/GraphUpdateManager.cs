using System.Collections.Generic;
using UnityEngine;

public class GraphUpdateManager : MonoBehaviour
{
    public static GraphUpdateManager Instance;

    private readonly HashSet<MonsterAi> waitForGraphUpdateMonsters = new();

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        AstarPath.OnGraphsUpdated += HandleGraphsUpdated;
    }

    void OnDisable()
    {
        AstarPath.OnGraphsUpdated -= HandleGraphsUpdated;
    }

    void HandleGraphsUpdated(AstarPath astar)
    {
        foreach (var monster in waitForGraphUpdateMonsters)
        {
            monster.OnGraphsUpdated();
        }

        waitForGraphUpdateMonsters.Clear();
    }
}
