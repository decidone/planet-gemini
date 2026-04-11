using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PingManager : NetworkBehaviour
{
    public GameObject pingMarkerPrefab;
    public PingUI pingUI;

    public int maxPings = 5;
    public float holdTime = 0.3f;
    public float pingClickRadius = 0.5f;

    InputManager inputManager;
    readonly Dictionary<int, PingMarker> markers = new();
    bool wheelHold;
    float holdTimer;
    int nextId;

    #region Singleton
    public static PingManager instance;

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

    void OnEnable()
    {
        inputManager = InputManager.instance;
        inputManager.controls.Player.Ping.performed += OnWheelPerformed;
    }

    void OnDisable()
    {
        inputManager.controls.Player.Ping.performed -= OnWheelPerformed;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            wheelHold = false;
    }

    void Update()
    {
        if (!IsSpawned) return;

        if (wheelHold)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer > holdTime)
                pingUI.OpenUI();
        }
    }

    void OnWheelPerformed(InputAction.CallbackContext ctx)
    {
        bool pressed = ctx.ReadValueAsButton();
        wheelHold = pressed;

        if (wheelHold)
        {
            holdTimer = 0;
        }
        else
        {
            if (holdTimer < holdTime)
                PlacePing();
        }
    }

    void PlacePing()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 pos = Camera.main.ScreenToWorldPoint(screenPos);

        // 클릭 위치에 기존 핑이 있으면 제거
        foreach (var marker in markers)
        {
            if (marker.Value == null) continue;
            if (Vector2.Distance(pos, marker.Value.transform.position) <= pingClickRadius)
            {
                RequestRemoveServerRpc(marker.Key);
                return;
            }
        }

        RequestSpawnServerRpc(pos, pingUI.SelectedGroup, pingUI.SelectedSub);
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestSpawnServerRpc(Vector2 pos, int group, int sub)
    {
        while (markers.Count >= maxPings)
            RemoveOldest();

        SpawnClientRpc(nextId++, pos, group, sub);
    }

    [ClientRpc]
    void SpawnClientRpc(int id, Vector2 pos, int group, int sub)
    {
        var go = Instantiate(pingMarkerPrefab, (Vector3)pos, Quaternion.identity);
        var marker = go.GetComponent<PingMarker>();
        marker.Init(id, GetIcon(group, sub), OnMarkerDismiss);
        markers[id] = marker;
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestRemoveServerRpc(int id)
    {
        RemoveClientRpc(id);
    }

    [ClientRpc]
    void RemoveClientRpc(int id)
    {
        if (!markers.TryGetValue(id, out var marker)) return;

        markers.Remove(id);
        if (marker != null) Destroy(marker.gameObject);
    }

    void RemoveOldest()
    {
        int oldest = int.MaxValue;
        foreach (var id in markers.Keys)
            if (id < oldest) oldest = id;

        if (oldest < int.MaxValue)
            RemoveClientRpc(oldest);
    }

    void OnMarkerDismiss(int id) => RequestRemoveServerRpc(id);

    Sprite GetIcon(int group, int sub)
    {
        if (group < 0 || group >= pingUI.pingGroups.Count) return null;

        var icons = pingUI.pingGroups[group].icons;
        return (sub >= 0 && sub < icons.Count) ? icons[sub] : null;
    }
}
