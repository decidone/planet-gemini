using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SteamManager : MonoBehaviour
{
    public string userName;
    public bool getData;
    public SteamId PlayerSteamId { get; set; }
    public SteamId opponentSteamId;
    //bool clientConnCheck;
    bool clientReceive;
    private const int MaxChunkSize = 1024;
    //[SerializeField] GameObject MainMenu;
    //[SerializeField] GameObject InLobbyMenu;

    #region Singleton
    public static SteamManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    private void Start()
    {
        PlayerSteamId = SteamClient.SteamId;
    }

    private void Update()
    {
        if (clientReceive)
        {
            ClientConnectGet();
        }
    }

    private void OnEnable()
    {
        SteamMatchmaking.OnLobbyCreated += LobbyCreated;
        SteamMatchmaking.OnLobbyEntered += LobbyEntered;
        SteamMatchmaking.OnLobbyMemberDisconnected += LobbyMemberLeft;
        SteamMatchmaking.OnLobbyMemberLeave += LobbyMemberLeft;
        SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
        SteamMatchmaking.OnLobbyMemberJoined += ClientConnent;
    }

    private void OnDisable()
    {
        SteamMatchmaking.OnLobbyCreated -= LobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= GameLobbyJoinRequested;
        SteamMatchmaking.OnLobbyMemberJoined -= ClientConnent;
    }

    private void LobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK)
        {
            lobby.SetPublic();
            lobby.SetJoinable(true);
            lobby.SetData("owner", lobby.Owner.Name);
            lobby.SetData("mapSize", MainGameSetting.instance.mapSizeIndex.ToString());

            //NetworkManager.Singleton.StartHost();
        }
        else
        {
            Debug.Log("Create Lobby Error");
        }
    }

    private void LobbyEntered(Lobby lobby)
    {
        LobbySaver.instance.currentLobby = lobby;
        userName = SteamClient.Name;
        Debug.Log("Entered");

        if (!NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;

            opponentSteamId = lobby.Owner.Id;
            AcceptP2P(opponentSteamId);
            Debug.Log("conn" + opponentSteamId);

            string data = lobby.GetData("mapSize");
            MainGameSetting.instance.MapSizeSet(int.Parse(data));
            LoadingUICtrl.Instance.LoadScene("GameScene", false);
        }
        else
        {
            //LoadingUICtrl.Instance.LoadScene("GameScene", true);
        }
    }

    void ClientConnent(Lobby lobby, Friend friend)
    {
        if (friend.Id != PlayerSteamId)
        {
            Debug.Log("Client Conn : " + friend.Id);
            opponentSteamId = friend.Id;
            AcceptP2P(opponentSteamId);
            clientReceive = true;
            //clientConnCheck = true;
        }
    }

    private void AcceptP2P(SteamId opponentId)
    {
        Debug.Log("AcceptP2P : " + opponentId);
        try
        {
            // For two players to send P2P packets to each other, they each must call this on the other player
            SteamNetworking.AcceptP2PSessionWithUser(opponentId);
        }
        catch
        {
            Debug.Log("Unable to accept P2P Session with user");
        }
    }

    public void ClientConnectSend()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            string message = "ClientConnect";
            byte[] data = Encoding.UTF8.GetBytes(message);
            SteamNetworking.SendP2PPacket(opponentSteamId, data);
            Debug.Log("ClientConnectSend");
        }
    }

    public void ClientConnectGet()
    {
        bool packetAvailable = SteamNetworking.IsP2PPacketAvailable();
        if (packetAvailable)
        {
            var packet = SteamNetworking.ReadP2PPacket();
            string opponentDataSent = Encoding.UTF8.GetString(packet.Value.Data);
            Debug.Log(opponentDataSent);

            if (opponentDataSent == "ClientConnect")
            {
                //Time.timeScale = 0;
                Debug.Log("Time.timeScale = 0 and send;");
                SendP2PPacket();
            }
            else if (opponentDataSent == "DataGetEnd")
            {
                clientReceive = false;
            }
            else
            {
                SendP2PPacket();
            }
        }
    }

    public void SendP2PPacket()
    {
        string message = GeminiNetworkManager.instance.RequestJson();
        byte[] data = Encoding.UTF8.GetBytes(message);
        int totalChunks = Mathf.CeilToInt((float)data.Length / MaxChunkSize);

        // Send each chunk
        for (int i = 0; i < totalChunks; i++)
        {
            int chunkSize = Mathf.Min(MaxChunkSize, data.Length - i * MaxChunkSize);
            byte[] chunk = new byte[chunkSize + 3]; // Extra 3 bytes for metadata (index, flag, total chunks)
            chunk[0] = (byte)i;  // Index of the chunk
            chunk[1] = (byte)(i == totalChunks - 1 ? 1 : 0); // Last chunk flag
            chunk[2] = (byte)totalChunks;  // Total number of chunks

            Array.Copy(data, i * MaxChunkSize, chunk, 3, chunkSize);

            bool success = SteamNetworking.SendP2PPacket(opponentSteamId, chunk, chunk.Length);
            if (success)
            {
                Debug.Log($"Packet {i + 1}/{totalChunks} Send Success!");
            }
            else
            {
                Debug.LogError($"Packet {i + 1}/{totalChunks} Send Failed!");
            }
        }
    }

    public void ReceiveP2PPacket()
    {
        bool packetAvailable = SteamNetworking.IsP2PPacketAvailable();
        List<byte> receivedData = new List<byte>();
        bool isLastChunkReceived = false;
        int totalChunks = 0;
        HashSet<int> receivedChunkIndices = new HashSet<int>();
        Debug.Log("ReceiveP2PPacket : " + packetAvailable);
        while (packetAvailable && !isLastChunkReceived)
        {
            var packet = SteamNetworking.ReadP2PPacket();
            if (packet.HasValue)
            {
                byte[] data = packet.Value.Data;
                int chunkIndex = data[0];  // Chunk index
                bool isLastChunk = data[1] == 1;  // Last chunk flag
                totalChunks = data[2];  // Total number of chunks

                // Add the chunk data to the receivedData list (excluding the first 3 bytes)
                receivedData.AddRange(data.Skip(3));
                receivedChunkIndices.Add(chunkIndex);

                // Check if it's the last chunk
                if (isLastChunk)
                {
                    isLastChunkReceived = true;
                }
            }
  
            packetAvailable = SteamNetworking.IsP2PPacketAvailable();
        }

        if (receivedChunkIndices.Count > 0)
        {
            Debug.Log("count : " + receivedChunkIndices.Count);
            foreach (var data in receivedChunkIndices)
            {
                Debug.Log("data : " + data);
            }
        }

        if (isLastChunkReceived)
        {
            if (receivedChunkIndices.Count == totalChunks)
            {            
                Debug.Log("GetDataEnd");
                HandleOpponentDataPacket(receivedData.ToArray());
                string message = "DataGetEnd";
                byte[] data = Encoding.UTF8.GetBytes(message);
                SteamNetworking.SendP2PPacket(opponentSteamId, data);
            }
            else
            {
                Debug.LogWarning($"Packet Loss: {receivedChunkIndices.Count}/{totalChunks} recive Call");
                RequestMissingPackets();
            }
        }
    }

    // 누락된 패킷 요청 함수
    private void RequestMissingPackets()
    {
        Debug.Log("RequestMissingPackets");
        // 누락된 패킷 요청 로직 (호스트에게 요청 메시지를 보냄)
        string message = "LossPacket";
        byte[] data = Encoding.UTF8.GetBytes(message);
        SteamNetworking.SendP2PPacket(opponentSteamId, data);
    }

    private void HandleOpponentDataPacket(byte[] dataPacket)
    {
        try
        {
            string opponentDataSent = ConvertByteArrayToString(dataPacket);
            Debug.Log($"Converted string: {opponentDataSent}");
            DataManager.instance.Load(opponentDataSent);
            Debug.Log("Get Data");
            getData = true;
        }
        catch
        {
            Debug.Log("Failed to process incoming opponent data packet");
        }
    }

    private string ConvertByteArrayToString(byte[] byteArrayToConvert)
    {
        return Encoding.UTF8.GetString(byteArrayToConvert);
    }

    private async void GameLobbyJoinRequested(Lobby lobby, SteamId SteamId)
    {
        await lobby.Join();
    }

    public async void HostLobby()
    {
        await SteamMatchmaking.CreateLobbyAsync(2);
    }

    //public async void JoinLobbyWithID()
    //{
    //    ulong Id;
    //    if (!ulong.TryParse(LobbyIdInputField.text, out Id))
    //        return;

    //    Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

    //    foreach (Lobby lobby in lobbies)
    //    {
    //        if (lobby.Id == Id)
    //        {
    //            await lobby.Join();
    //            return;
    //        }
    //    }
    //}

    public async void JoinLobby(Lobby _lobby)
    {
        Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

        foreach (Lobby lobby in lobbies)
        {
            if (lobby.Id == _lobby.Id)
            {
                await lobby.Join();
                return;
            }
        }
    }

    public void LeaveLobby()
    {
        LobbySaver.instance.currentLobby?.Leave();
        LobbySaver.instance.currentLobby = null;
    }

    private void LobbyMemberLeft(Lobby lobby, Friend friend)
    {
        if (!GameManager.instance.isHost)
        {
            Debug.Log("Host left");
            if (DisconnectedPopup.instance != null)
            {
                DisconnectedPopup.instance.OpenUI();
            }
            else
            {
                LeaveGame();
            }
        }
        //Debug.Log(lobby.Owner.Id);
        //Debug.Log(friend.Id);
    }

    public void LeaveGame()
    {
        SteamManager.instance.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);
        GameManager.instance.DestroyAllDontDestroyOnLoadObjects();
        SceneManager.LoadScene("MainMenuScene");
    }

    public async void GetLobbiesList()
    {
        Lobby[] lobbies = await SteamMatchmaking.LobbyList.RequestAsync();
        LobbiesListManager.instance.OpenUI();
        LobbiesListManager.instance.DestroyLobbies();

        if (lobbies != null)
        {
            foreach (Lobby lobby in lobbies)
            {
                Debug.Log(lobby.Id);
                LobbiesListManager.instance.DisplayLobby(lobby);
            }
        }
    }
}
