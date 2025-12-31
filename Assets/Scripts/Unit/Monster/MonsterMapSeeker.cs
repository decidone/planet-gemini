using Pathfinding;
using System;
using System.Collections;
using UnityEngine;

public class MonsterMapSeeker : MonoBehaviour
{
    protected Seeker seeker;

    private void Start()
    {
        seeker = GetComponent<Seeker>();
    }

    public IEnumerator GetBlockingObjectToTarget(Vector3 monsterPos, Vector3 movePos, bool isHostMap, Action<GameObject, float> onComplete)
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


        bool isDone = false;

        ABPath path = ABPath.Construct(monsterPos, movePos, p =>
        {
            isDone = true;

            // 경로 계산 실패하거나 유효하지 않으면 무시
            if (this == null || p.error || p.vectorPath.Count == 0) return;
        });

        seeker.StartPath(path);
        yield return new WaitUntil(() => isDone);

        if (path.error || path.vectorPath.Count == 0)
        {
            onComplete?.Invoke(null, 0);
            yield break;
        }

        // 맵 기준 총 거리 계산
        float mapOnlyDistance = 0f;
        for (int i = 0; i < path.vectorPath.Count - 1; i++)
        {
            mapOnlyDistance += Vector3.Distance(
                path.vectorPath[i],
                path.vectorPath[i + 1]
            );
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

        onComplete?.Invoke(blockingObj, mapOnlyDistance);
    }
}
