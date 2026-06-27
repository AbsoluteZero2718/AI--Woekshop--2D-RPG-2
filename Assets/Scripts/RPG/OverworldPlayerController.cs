using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class OverworldPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private RandomEncounterTrigger encounterTrigger;

    private Rigidbody2D body;
    private Vector2 moveInput;
    private bool movementEnabled = true;
    private SpriteRenderer spriteRenderer;

    public bool IsMoving => moveInput.sqrMagnitude > 0.01f;
    public Vector2 MoveInput => moveInput;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (encounterTrigger == null)
            encounterTrigger = GetComponent<RandomEncounterTrigger>();
    }

    private void Update()
    {
        if (!movementEnabled)
        {
            moveInput = Vector2.zero;
            return;
        }

#if ENABLE_INPUT_SYSTEM
        moveInput = ReadKeyboardMoveInput();
#else
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical"));
#endif

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        if (moveInput.x != 0f && spriteRenderer != null)
            spriteRenderer.flipX = moveInput.x < 0f;
    }

#if ENABLE_INPUT_SYSTEM
    private static Vector2 ReadKeyboardMoveInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return Vector2.zero;

        float x = 0f;
        float y = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            x += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            y -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            y += 1f;

        return new Vector2(x, y);
    }
#endif

    private void FixedUpdate()
    {
        if (!movementEnabled)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        body.linearVelocity = moveInput * moveSpeed;

        if (IsMoving && encounterTrigger != null)
            encounterTrigger.RegisterMovement(body.linearVelocity.magnitude * Time.fixedDeltaTime);
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;

        if (!enabled)
        {
            moveInput = Vector2.zero;
            body.linearVelocity = Vector2.zero;
        }
    }
}
