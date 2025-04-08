using Cinemachine;
using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] CinemachineVirtualCamera freeLookCamera;
    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            NetworkObject playerObj = Runner.Spawn(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            SetCamera(playerObj.transform);
        }
    }

    private void SetCamera(Transform playerTransform)
    {
        freeLookCamera.m_Follow = playerTransform;
        freeLookCamera.m_LookAt = playerTransform;
    }
}