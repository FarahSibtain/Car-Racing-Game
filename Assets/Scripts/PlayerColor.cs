using Fusion;
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;

    [Networked(OnChanged = nameof(NetworkColorChanged))]
    public Color NetworkedColor { get; set; }
    
    private void Update()
    {
        if (HasStateAuthority && Input.GetKeyDown(KeyCode.E))
        {
            NetworkedColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
        }
    }
    
    private static void NetworkColorChanged(Changed<PlayerColor> changed)
    {
        changed.Behaviour._meshRenderer.material.color = changed.Behaviour.NetworkedColor;
    }
}