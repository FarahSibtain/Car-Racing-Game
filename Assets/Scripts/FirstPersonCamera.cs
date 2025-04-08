using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [SerializeField] private float _mouseSensitivity = 10f;
    public Transform Target { get; set; }

    private float _verticalRotation;
    private float _horizontalRotation;

    private void LateUpdate()
    {
        if (Target == null)
            return;

        transform.position = Target.position;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        _verticalRotation -= mouseY * _mouseSensitivity;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -70f, 70f);

        _horizontalRotation += mouseX * _mouseSensitivity;

        transform.rotation = Quaternion.Euler(_verticalRotation, _horizontalRotation, 0);
    }
}