using Fusion;
using UnityEngine;

public class RaycastAttack : NetworkBehaviour
{
    [SerializeField] private float _damage;
    public PlayerMovement PlayerMovement;

    private void Update()
    {
        if (HasStateAuthority == false)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray = PlayerMovement.PlayerCamera.ScreenPointToRay(Input.mousePosition);
            ray.direction = PlayerMovement.PlayerCamera.transform.forward;
            Debug.DrawRay(ray.origin, ray.direction, Color.red, 1f);
            if (Runner.GetPhysicsScene().Raycast(ray.origin, ray.direction, out RaycastHit hit))
            {
                if (hit.transform.TryGetComponent(out Health health)) 
                    health.DealDamageRpc(_damage);
            }
        }
    }
}