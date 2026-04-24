using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Referencias")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private float moveInput;
    private bool jumpPressed;
    private bool isGrounded;
    private bool isFacingRight = true;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        CheckGround();

        moveInput = 0f;

        if (Input.GetKey(KeyCode.A))
            moveInput = -1f;
        else if (Input.GetKey(KeyCode.D))
            moveInput = 1f;

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && isGrounded)
            jumpPressed = true;

        if (Input.GetKeyDown(KeyCode.L) && !IsAttacking())
            animator.SetTrigger("Attack");

        if (moveInput > 0f && !isFacingRight)
            Flip();
        else if (moveInput < 0f && isFacingRight)
            Flip();

        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if (jumpPressed)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpPressed = false;
        }

        UpdateAnimations();
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void Flip()
{
    isFacingRight = !isFacingRight;

    Vector3 scale = transform.localScale;
    scale.x *= -1f;
    transform.localScale = scale;
}

    private void UpdateAnimations()
    {
        if (animator == null || rb == null)
            return;

        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("Grounded", isGrounded);
        animator.SetFloat("YVelocity", rb.velocity.y);
    }

    public bool IsAttacking()
    {
        if (animator == null)
            return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("attack");
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}