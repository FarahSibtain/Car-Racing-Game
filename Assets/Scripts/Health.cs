using Fusion;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [Networked(OnChanged = nameof(NetworkedHealthChanged))]
    public float NetworkedHealth { get; set; } = 100;
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void DealDamageRpc(float damage)
    {
        Debug.Log($"Received {damage} damage on StateAuthority, modifying Networked variable");
        NetworkedHealth -= damage;
    }
    
    private static void NetworkedHealthChanged(Changed<Health> changed)
    {
        Debug.Log($"Health changed to: {changed.Behaviour.NetworkedHealth}");
    }
}