using System.Linq;
using Cinemachine;
using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] CinemachineVirtualCamera freeLookCamera;
    [SerializeField] int horizontalGap;
    [SerializeField] int verticalGap;
    
    //[Networked, Capacity(12)] private NetworkDictionary<PlayerRef, NetworkObject> Players => default;
    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            Debug.Log("Spawn player");
            NetworkObject playerObj = Runner.Spawn(playerPrefab, GetCarPosition(), spawnPoint.rotation);            
            //Players.Add(player, playerObj);
            SetCamera(playerObj.transform);
            
        }
    }
    Vector3 GetCarPosition()
    {
        int playerCount = Runner.ActivePlayers.Count() - 1;
        //determine the position of the car 
        float posX = 0;
        float posZ = 0;
        switch (playerCount % 3)
        {
            case 0:
                posZ = spawnPoint.transform.position.z;
                break;
            case 1:
                posZ = spawnPoint.transform.position.z + horizontalGap;
                break;
            case 2:
                posZ = spawnPoint.transform.position.z + 2 * horizontalGap;
                break;
        }
        posX = spawnPoint.transform.position.x + (playerCount / 3) * verticalGap;

        return new Vector3(posX, spawnPoint.transform.position.y, posZ);
    }

    private void SetCamera(Transform playerTransform)
    {
        freeLookCamera.m_Follow = playerTransform;
        freeLookCamera.m_LookAt = playerTransform;
    }
}