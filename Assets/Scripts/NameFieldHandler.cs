using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

public class NameFieldHandler : NetworkBehaviour
{
    [SerializeField] TMP_Text playerNameField;

    // Network property to store and sync the player name
    [Networked(OnChanged = nameof(OnNameChanged))]
    public NetworkString<_32> PlayerName { get; set; }

    // Called on all clients when the name changes on the network
    public static void OnNameChanged(Changed<NameFieldHandler> changed)
    {
        // Update the UI when the networked name changes
        changed.Behaviour.UpdateNameDisplay();
    }

    private void UpdateNameDisplay()
    {
        playerNameField.text = PlayerName.ToString();
    }

    public void SetPlayerNameField(string playerName)
    {
        PlayerName = playerName;
        playerNameField.text = playerName;
    }    
}
