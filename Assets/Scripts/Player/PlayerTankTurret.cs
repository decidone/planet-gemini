using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerTankTurret : NetworkBehaviour
{
    [SerializeField]
    Sprite[] sprites = new Sprite[8]; // 8방향 스프라이트 배열
    [SerializeField]
    SpriteRenderer spriteRenderer; // 스프라이트 렌더러
    int turretIndex;
    [SerializeField]
    Transform[] bulletSpawnPos = new Transform[8];
    [HideInInspector]
    public bool onTank;
    private Vector3[] directions = new Vector3[8]
    {
        Vector3.up,                             // 0: 위
        (Vector3.up + Vector3.right).normalized, // 1: 오른쪽 위
        Vector3.right,                          // 2: 오른쪽
        (Vector3.right + Vector3.down).normalized, // 3: 오른쪽 아래
        Vector3.down,                           // 4: 아래
        (Vector3.down + Vector3.left).normalized, // 5: 왼쪽 아래
        Vector3.left,                           // 6: 왼쪽
        (Vector3.left + Vector3.up).normalized  // 7: 왼쪽 위
    };

    private int lastSyncedIndex = -1;
    private float syncInterval = 0.1f;
    private float syncTimer = 0f;

    void Update()
    {
        if (!IsOwner || !onTank) return;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        Vector3 direction = (mousePosition - transform.position).normalized;
        turretIndex = GetDirectionIndex(direction);

        spriteRenderer.sprite = sprites[turretIndex];

        syncTimer += Time.deltaTime;
        if (syncTimer >= syncInterval)
        {
            syncTimer = 0f;
            if (turretIndex != lastSyncedIndex) // 변화가 있으면 동기화
            {
                lastSyncedIndex = turretIndex;
                SyncTurretIndexServerRpc(turretIndex);
            }
        }
    }

    [ServerRpc]
    private void SyncTurretIndexServerRpc(int index)
    {
        SyncTurretIndexClientRpc(index);
    }

    [ClientRpc]
    private void SyncTurretIndexClientRpc(int index)
    {
        if (IsOwner) return;
        turretIndex = index;
        spriteRenderer.sprite = sprites[turretIndex];
    }

    int GetDirectionIndex(Vector3 direction)
    {
        int bestIndex = 0;
        float maxDot = -Mathf.Infinity;
        for (int i = 0; i < directions.Length; i++)
        {
            float dot = Vector3.Dot(direction, directions[i]);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public Vector2 TurretAttackPos()
    {
        return bulletSpawnPos[turretIndex].position;
    }
}
