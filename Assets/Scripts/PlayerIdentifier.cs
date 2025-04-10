using UnityEngine;
using Fusion;
using TMPro;
using System;

public class PlayerIdentifier : NetworkBehaviour
{
    public TMP_Text playerNameField;
    // Networked player ID
    [Networked] public NetworkString<_64> PlayerId { get; set; }

    // Player's display name

    // Network property to store and sync the player name
    [Networked(OnChanged = nameof(OnNameChanged))]
    public NetworkString<_64> DisplayName { get; set; }

    // Reference to the finish line for checking position (non-networked)
    private RaceFinishLine finishLine;

    [Networked] public NetworkBool HasFinished { get; set; }

    [Networked] public int CurrentPosition { get; set; }

    public override void Spawned()
    {
        base.Spawned();

        playerNameField.text = PlayerPrefs.GetString(AutoConnectionHandler.playerNameField);

        // Generate a unique ID if this is our local player
        if (Object.HasInputAuthority)
        {
            // Generate a unique ID based on player session ID or fusion player ID
            string uniqueId = Runner.LocalPlayer.ToString();

            // Set player ID through RPC to ensure host updates it
            RPC_SetPlayerId(Runner, Object.Id, uniqueId);
            
            DisplayName = PlayerPrefs.GetString(AutoConnectionHandler.playerNameField);
            RPC_SetDisplayName(Runner, Object.Id, DisplayName.ToString());
        }

        // Find finish line in the scene if not already set
        if (finishLine == null)
        {
            finishLine = FindObjectOfType<RaceFinishLine>();
        }
    }

    // Called on all clients when the name changes on the network
    public static void OnNameChanged(Changed<PlayerIdentifier> changed)
    {
        // Update the UI when the networked name changes
        changed.Behaviour.UpdateNameDisplay();
    }

    private void UpdateNameDisplay()
    {
        playerNameField.text = DisplayName.ToString();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public static void RPC_SetPlayerId(NetworkRunner runner, NetworkId objectId, string id)
    {
        if (runner.TryFindObject(objectId, out NetworkObject netObj))
        {
            PlayerIdentifier player = netObj.GetComponent<PlayerIdentifier>();
            if (player != null)
            {
                player.PlayerId = id;
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public static void RPC_SetDisplayName(NetworkRunner runner, NetworkId objectId, string name)
    {
        if (runner.TryFindObject(objectId, out NetworkObject netObj))
        {
            PlayerIdentifier player = netObj.GetComponent<PlayerIdentifier>();
            if (player != null)
            {
                player.DisplayName = name;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        // If we have a finish line reference, check our position
        if (finishLine != null)
        {
            int position = finishLine.GetPlayerPosition(PlayerId);
            if (position > 0)
            {
                // We've finished the race
                if (Object.HasStateAuthority)
                {
                    HasFinished = true;
                    CurrentPosition = position;
                }
                else
                {
                    RPC_SetFinished(Runner, Object.Id, position);
                }

                // You could display "You finished 1st/2nd/3rd!" message here
                DisplayFinishPosition(position);
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public static void RPC_SetFinished(NetworkRunner runner, NetworkId objectId, int position)
    {
        if (runner.TryFindObject(objectId, out NetworkObject netObj))
        {
            PlayerIdentifier player = netObj.GetComponent<PlayerIdentifier>();
            if (player != null)
            {
                player.HasFinished = true;
                player.CurrentPosition = position;
            }
        }
    }

    private void DisplayFinishPosition(int position)
    {
        // This could update a UI element on the player's screen
        string suffix = GetOrdinalSuffix(position);
        string positionDisplayStr = $"You finished {position}{suffix}!";
        Debug.Log(positionDisplayStr);

        var raceUIControl = FindObjectOfType<RaceResultsUIController>();
        raceUIControl.DisplayPosition(positionDisplayStr);
    }

    private string GetOrdinalSuffix(int position)
    {
        int remainder = position % 100;
        if (remainder >= 11 && remainder <= 13)
        {
            return "th";
        }

        remainder = position % 10;
        switch (remainder)
        {
            case 1: return "st";
            case 2: return "nd";
            case 3: return "rd";
            default: return "th";
        }
    }

    internal void StopCarControl()
    {
        var carController = GetComponent<RCC_CarControllerV4>();

        // Ensure the carController is not null
        if (carController != null)
        {
            // Set throttle input to zero to stop acceleration
            carController.throttleInput = 0f;

            // Apply full braking
            carController.brakeInput = 1f;

            carController.handbrakeInput = 1f;

            carController.KillEngine();

            RCC_InputManager.Instance.enabled = false;
        }
        //carController.enabled = false;
    }
}