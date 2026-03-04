using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Range(0f, 10f)]
    [SerializeField] float sensitivity = 1f;
    [SerializeField] float minPitch = -80f;
    [SerializeField] float maxPitch = 80f;
    [SerializeField] GameControll gameManager;
    [SerializeField] PlayerController player;


    InputAction lookAction;
    Vector2 lookInput;
    Vector3 rotation = Vector3.zero; // x = pitch, y = yaw

    void Start()
    {
        
        lookAction = InputSystem.actions.FindAction("Look");
        lookAction.performed += Look;
        lookAction.canceled += Look;

        // Initialize rotation from current transform
        rotation.x = transform.eulerAngles.x;
        rotation.y = transform.eulerAngles.y;
    }

    void Update()
    {
        if (!gameManager.pause && !player.lockedByDying)
        {
            rotation.x -= lookInput.y * sensitivity;
            rotation.y += lookInput.x * sensitivity;
            rotation.x = Mathf.Clamp(rotation.x, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(rotation);
        }
    }

    void Look(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }
}