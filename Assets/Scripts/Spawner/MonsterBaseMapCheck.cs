using NUnit.Framework.Internal;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterBaseMapCheck : MonoBehaviour
{
    GameManager gameManager;
    protected Seeker seeker;
    protected int currentWaypointIndex;                 // 현재 이동 중인 경로 점 인덱스
    [SerializeField]
    protected List<Vector3> movePath = new List<Vector3>();
    public ABPath wavePath;

    #region Singleton
    public static MonsterBaseMapCheck instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    #endregion

    private void Start()
    {
        gameManager = GameManager.instance;
        seeker = GetComponent<Seeker>();
    }

    public IEnumerator CheckPath(Vector3 wavePos, bool isHostMap)
    {
        GraphMask mask;
        Vector3 targetPos;
        if (isHostMap)
        {
            mask = GraphMask.FromGraphName("Map1Wave");
            targetPos = gameManager.hostPlayerSpawnPos;
        }
        else
        {
            mask = GraphMask.FromGraphName("Map2Wave");
            targetPos = gameManager.clientPlayerSpawnPos;
        }
        seeker.graphMask = mask;
        wavePath = ABPath.Construct(wavePos, targetPos, null);
        seeker.CancelCurrentPathRequest();
        seeker.StartPath(wavePath);

        yield return StartCoroutine(wavePath.WaitForPath());

        currentWaypointIndex = 1;
        movePath = wavePath.vectorPath;
        WavePoint.instance.SetLine(true, movePath);
    }
}
