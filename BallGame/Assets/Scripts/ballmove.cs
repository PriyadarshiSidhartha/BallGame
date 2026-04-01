using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ballmove : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("If checked, the ball is controlled using Arrow Keys. Otherwise, controlled using WASD.")]
    public bool player2;
    
    [Header("Movement Settings")]
    [Tooltip("Movement acceleration speed.")]
    public float speed = 25f;
    [Tooltip("How quickly the ball stops when no input is provided.")]
    public float friction = 8f;
    [Tooltip("Maximum horizontal velocity for the ball.")]
    public float maxSpeed = 10f;

    // We keep an internal reference to the InputAction.
    // By creating the action in code instead of using an InputActionReference to the global InputActions asset,
    // we can strictly separate WASD from the Arrow keys based on the boolean player2 flag without editing global control schemes.
    private InputAction moveAction;
    
    private Rigidbody rb;
    private Vector2 currentInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // 1. Manually create the Move input action
        moveAction = new InputAction("Move", InputActionType.Value, "<Vector2>");

        // 2. Assign the specific bindings based on the player2 flag
        if (player2)
        {
            // Player 2 uses Arrow Keys
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
        }
        else
        {
            // Player 1 uses WASD
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
        }
    }

    // 3. Subscribe in OnEnable
    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.Enable();
            moveAction.performed += OnMoveInput;
            moveAction.canceled += OnMoveInput;
        }
    }

    // 4. Unsubscribe in OnDisable
    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.Disable();
            moveAction.performed -= OnMoveInput;
            moveAction.canceled -= OnMoveInput;
        }
    }

    // Callback when input is detected or released
    private void OnMoveInput(InputAction.CallbackContext context)
    {
        // Read the Vector2 value from the input action
        currentInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            // Calculate movement direction
            Vector3 movement = new Vector3(currentInput.x, 0f, currentInput.y);

            // Apply acceleration instead of regular force for a snappier feeling, ignoring rigidbody mass
            rb.AddForce(movement * speed, ForceMode.Acceleration);

            // Apply custom friction when no input is pressed to kill inertia more quickly
            if (movement == Vector3.zero)
            {
                // Counteract current horizontal velocity
                Vector3 counterForce = -rb.linearVelocity * friction;
                counterForce.y = 0f; // Do not apply friction on the Y axis (falling)
                rb.AddForce(counterForce, ForceMode.Acceleration);
            }

            // Clamp maximum speed to remain fully responsive
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (horizontalVelocity.magnitude > maxSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
            }
        }
    }
}
