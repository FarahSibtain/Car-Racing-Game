using Fusion;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = System.Diagnostics.Debug;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float _playerSpeed = 2f;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _gravityValue = -9.81f;

    private CharacterController _controller;
    private Vector3 _velocity;
    private bool _jumpPressed;
    
    public Camera PlayerCamera;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }
    
    private void Update()
    {
        if (Input.GetButtonDown("Jump")) 
            _jumpPressed = true;
    }

    public override void Spawned()
    {
        if (HasStateAuthority == false)
            return;
        
        //PlayerCamera = Camera.main;
        //Debug.Assert(PlayerCamera != null, nameof(PlayerCamera) + " != null");
        //PlayerCamera.GetComponent<FirstPersonCamera>().Target = GetComponent<NetworkTransform>().InterpolationTarget;
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority == false)
            return;
        
        if (_controller.isGrounded) 
            _velocity = new Vector3(0, -1, 0);

        //Quaternion cameraRotationY = Quaternion.Euler(0, PlayerCamera.transform.rotation.eulerAngles.y, 0);
        Vector3 move = /*cameraRotationY **/ new Vector3(-Input.GetAxis("Vertical"), 0, Input.GetAxis("Horizontal")) * Runner.DeltaTime * _playerSpeed;
        
        _velocity.y += _gravityValue * Runner.DeltaTime;
        if (_jumpPressed && _controller.isGrounded) 
            _velocity.y += _jumpForce;

        _controller.Move(move + _velocity * Runner.DeltaTime);

        if (move != Vector3.zero) 
            gameObject.transform.forward = move;

        _jumpPressed = false;
    }
}