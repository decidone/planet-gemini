using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
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

    public IEnumerator GetBlockingObjectToTarget(Vector3 monsterPos, Vector3 movePos, bool isHostMap, Action<GameObject> onComplete)
    {
        GameObject blockingObj = null;
        GraphMask mask;
        if (isHostMap)
        {
            mask = GraphMask.FromGraphName("Map1Wave");
        }
        else
        {
            mask = GraphMask.FromGraphName("Map2Wave");
        }
        seeker.graphMask = mask;

        ABPath path = ABPath.Construct(monsterPos, movePos);
        bool isDone = false;

        path.callback += _ => isDone = true;

        seeker.StartPath(path);
        yield return new WaitUntil(() => isDone);

        if (path.error || path.vectorPath.Count < 2)
        {
            onComplete?.Invoke(null);
            yield break;
        }

        for (int i = 0; i < path.vectorPath.Count - 1; i++)
        {
            Vector3 from = path.vectorPath[i];
            Vector3 to = path.vectorPath[i + 1];
            Vector3 dir = to - from;
            float dist = dir.magnitude;

            RaycastHit2D[] hits = Physics2D.RaycastAll(from, dir.normalized, dist, LayerMask.GetMask("Obj"));
            if (hits.Length > 0)
            {
                // 첫 번째 충돌 오브젝트를 가져오거나, 조건에 맞는 것을 필터링할 수 있음
                foreach (var hit in hits)
                {
                    if (hit.collider != null)
                    {
                        blockingObj = hit.collider.gameObject;
                        break;
                    }
                }
                if (blockingObj != null)
                    break;
            }
        }

        onComplete?.Invoke(blockingObj);
    }
}
