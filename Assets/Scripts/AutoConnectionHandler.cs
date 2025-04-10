using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoConnectionHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] GameObject canvas;
    [SerializeField] TMP_InputField playerName;
    private NetworkRunner _runner;

    public static string playerNameField = "PlayerName";

    private void Start()
    {
        playerName.text = PlayerPrefs.GetString(playerNameField);
    }

    public void ConnectToPhotonNetwork()
    {
        if (string.IsNullOrEmpty(playerName.text))
        {
            return;
        }

        PlayerPrefs.SetString(playerNameField, playerName.text);

        StartSharedMode();
    }

    private async void StartSharedMode()
    {
        // Create the NetworkRunner instance
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Start the Shared mode session
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "MySharedSession", // You can customize this
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    // INetworkRunnerCallbacks implementation
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        Debug.Log("Player joined");
        canvas.SetActive(false);
    }

    // Implement other required INetworkRunnerCallbacks methods
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        Debug.Log("Player left");
    }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) {

        Debug.Log("Connected to Photon Server");
    }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}