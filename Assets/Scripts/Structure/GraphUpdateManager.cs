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

    public void LockMonsters(IEnumerable<MonsterAi> list)
    {
        foreach (var m in list)
        {
            if (m == null) continue;
            waitForGraphUpdateMonsters.Add(m);  // 중복 방지
        }
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
