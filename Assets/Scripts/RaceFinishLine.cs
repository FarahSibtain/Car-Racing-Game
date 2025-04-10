using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using WebSocketSharp;

public class RaceFinishLine : NetworkBehaviour
{    
    // Modified declaration with proper capacity attribute
    [Networked, Capacity(16)]
    private NetworkLinkedList<PlayerFinishData> FinishedPlayers { get; }

    // Define a struct for player finish data
    public struct PlayerFinishData : INetworkStruct
    {
        public NetworkString<_64> PlayerId;
        public float FinishTime;
    }

    // Dictionary to cache player names/IDs locally
    private Dictionary<NetworkString<_64>, string> playerDisplayNames = new Dictionary<NetworkString<_64>, string>();

    // Event that will be raised when race results change
    public delegate void RaceResultsUpdatedDelegate(List<string> results);
    public event RaceResultsUpdatedDelegate OnRaceResultsUpdated;

    // UI references (optional)
    [SerializeField] private GameObject raceResultsUI;

    // Networked flag to notify clients when results change
    [Networked] private int ResultsVersion { get; set; }
    private int _cachedResultsVersion = -1;

    public override void Spawned()
    {
        base.Spawned();

        // Make sure this script is on an object with a collider set to trigger
        Collider collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
        {
            Debug.LogWarning("Finish line collider should have isTrigger set to true");
            collider.isTrigger = true;
        }

        // Initialize if we're the host
        if (Object.HasStateAuthority)
        {
            FinishedPlayers.Clear();
            ResultsVersion = 0;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Check if results have been updated
        if (_cachedResultsVersion != ResultsVersion)
        {
            _cachedResultsVersion = ResultsVersion;
            NotifyResultsChanged();
        }
    }

    private void NotifyResultsChanged()
    {
        // Update UI or notify other systems about race results
        List<string> results = GetResultsList();
        OnRaceResultsUpdated?.Invoke(results);

        // Optionally show UI
        if (raceResultsUI != null)
        {
            UpdateResultsUI(results);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Try to get the player NetworkObject component
        NetworkObject playerNetObj = other.GetComponentInParent<NetworkObject>();
        if (playerNetObj == null) return;

        // Try to get a player identifier component
        PlayerIdentifier playerIdentifier = other.GetComponentInParent<PlayerIdentifier>();
        if (playerIdentifier == null) return;        

        Debug.Log($"Player crossed the finish line. PlayerId = {playerIdentifier.PlayerId}, DisplayName = {playerIdentifier.DisplayName.ToString()}");

        if (Object.HasStateAuthority)
            playerIdentifier.StopCarControl();

        // Get player ID
        NetworkString<_64> playerId = playerIdentifier.PlayerId;

        // Store display name if available
        if (!string.IsNullOrEmpty(playerIdentifier.DisplayName.ToString()))
        {
            //playerDisplayNames[playerId] = playerIdentifier.DisplayName.ToString();
            playerDisplayNames[playerId] = playerIdentifier.playerNameField.text;
        }       

        // Call RPC to register this player's finish
        if (Object.HasStateAuthority)
        {
            // If we're the host, register directly
            RegisterPlayerFinish(playerId.ToString());
        }
        else
        {
            // Otherwise send RPC to host
            RPC_RegisterPlayerFinish(Runner, Object.Id, playerId.ToString());
        }
    }

    // Static RPC method
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, InvokeResim = true)]
    public static void RPC_RegisterPlayerFinish(NetworkRunner runner, NetworkId objectId, string playerIdStr)
    {
        // Find the RaceFinishLine object
        if (runner.TryFindObject(objectId, out NetworkObject netObj))
        {
            RaceFinishLine finishLine = netObj.GetComponent<RaceFinishLine>();
            if (finishLine != null)
            {
                finishLine.RegisterPlayerFinish(playerIdStr);
            }
        }
    }

    private void RegisterPlayerFinish(string playerIdStr)
    {
        // Only the host can modify the networked list
        if (!Object.HasStateAuthority) return;

        NetworkString<_64> playerId = playerIdStr;

        // Check if player already finished
        bool alreadyFinished = false;
        foreach (var finishData in FinishedPlayers)
        {
            if (finishData.PlayerId.Equals(playerId))
            {
                alreadyFinished = true;
                break;
            }
        }

        if (!alreadyFinished)
        {
            // Create finish data
            PlayerFinishData finishData = new PlayerFinishData
            {
                PlayerId = playerId,
                FinishTime = (float)Runner.SimulationTime
            };

            // Add player to finished list
            FinishedPlayers.Add(finishData);

            // Increment the results version to trigger an update on all clients
            ResultsVersion += 1;
        }
    }

    private List<string> GetResultsList()
    {
        List<string> results = new List<string>();

        // Convert player IDs to display names if available
        foreach (var finishData in FinishedPlayers)
        {
            string displayName;
            if (playerDisplayNames.TryGetValue(finishData.PlayerId, out displayName))
            {
                results.Add(displayName);
            }
            else
            {
                Debug.Log("displayName does not exist");
                results.Add(finishData.PlayerId.ToString());
            }
        }

        return results;
    }

    private void UpdateResultsUI(List<string> results)
    {
        if (raceResultsUI != null)
        {
            RaceResultsUIController uiController = raceResultsUI.GetComponent<RaceResultsUIController>();
            if (uiController != null)
            {
                uiController.DisplayResults(results);
            }
        }
    }

    public List<string> GetCurrentStandings()
    {
        return GetResultsList();
    }

    public int GetPlayerPosition(NetworkString<_64> playerId)
    {
        if (Object == null)
        {
            return 0;
        }
        // Find the player in the list
        for (int i = 0; i < FinishedPlayers.Count; i++)
        {
            if (FinishedPlayers[i].PlayerId.Equals(playerId))
            {
                return i + 1; // Position is 1-based
            }
        }

        return 0; // Return 0 if not finished yet
    }

    public bool IsRaceCompleted(int totalPlayers)
    {
        return FinishedPlayers.Count >= totalPlayers;
    }
}