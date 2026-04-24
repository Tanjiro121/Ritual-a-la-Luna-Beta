using System.Collections;
using UnityEngine;

public class GuerreroDeCera : MonoBehaviour
{
    [Header("Referencias del boss")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D bodyCollider;

    [Header("Chequeo de borde")]
    [SerializeField] private Transform edgeCheck;
    [SerializeField] private float edgeCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Vida")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth = 50;

    [Header("Movimiento base")]
    [SerializeField] private float idleMoveSpeed = 1.2f;
    [SerializeField] private bool moveInIdle = true;

    [Header("Deteccion")]
    [SerializeField] private float detectRange = 8f;
    [SerializeField] private float dashRange = 4f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashTime = 0.4f;
    [SerializeField] private float dashCooldown = 1.2f;

    [Header("Rest por fase")]
    [SerializeField] private int phaseTwoHealthThreshold = 25;
    [SerializeField] private float restDuration = 2f;

    [Header("Daño al jugador por contacto")]
    [SerializeField] private int touchDamage = 1;
    [SerializeField] private float touchDamageInterval = 0.5f;
    [SerializeField] private float knockbackForceX = 7f;
    [SerializeField] private float knockbackForceY = 4f;

    [Header("Daño recibido del jugador")]
    [SerializeField] private int damageFromPlayerAttack = 1;
    [SerializeField] private float receiveHitCooldown = 0.25f;
    [SerializeField] private float hitFlashDuration = 0.12f;
    [SerializeField] private Color hitColor = new Color(0.5f, 0.9f, 1f, 1f);

    private bool isDead;
    private bool isResting;
    private bool isPreparingDash;
    private bool isDashing;
    private bool canDash = true;
    private bool facingRight = true;
    private bool enteredPhaseTwoRest;

    private float dashDirection;
    private float dashTimer;
    private float nextTouchDamageTime;
    private float nextReceiveHitTime;

    private Coroutine dashCooldownRoutine;
    private Coroutine restRoutine;
    private Coroutine flashRoutine;

    private Color originalColor;
    private PlayerHealth playerHealth;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bodyCollider = GetComponent<Collider2D>();
    }

    private void Awake()
    {
        currentHealth = maxHealth;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (isDead || player == null)
            return;

        if (playerHealth == null)
            playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null && playerHealth.IsDead())
        {
            rb.velocity = Vector2.zero;
            return;
        }

        FacePlayer();
        CheckPhaseTwoRest();

        if (isResting)
            return;

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (!HasGroundAhead())
            {
                StopDash();
                return;
            }

            if (dashTimer <= 0f)
            {
                StopDash();
                return;
            }

            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= dashRange && distance <= detectRange && canDash && !isPreparingDash)
            StartDashAnimation();
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (playerHealth != null && playerHealth.IsDead())
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (isResting)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        if (isDashing)
        {
            if (!HasGroundAhead())
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
                StopDash();
                return;
            }

            rb.velocity = new Vector2(dashDirection * dashSpeed, rb.velocity.y);
            return;
        }

        if (isPreparingDash)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        HandleIdleMovement();
    }

    private void HandleIdleMovement()
    {
        if (!moveInIdle || player == null)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > detectRange)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        float dir = player.position.x >= transform.position.x ? 1f : -1f;

        if (!HasGroundAheadForDirection(dir))
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        rb.velocity = new Vector2(dir * idleMoveSpeed, rb.velocity.y);
    }

    private void StartDashAnimation()
    {
        if (isDead || isResting || isPreparingDash || isDashing || !canDash)
            return;

        dashDirection = player.position.x >= transform.position.x ? 1f : -1f;
        UpdateFacingFromDirection(dashDirection);

        isPreparingDash = true;
        canDash = false;

        animator.SetTrigger("Dash");
    }

    private void StopDash()
    {
        isPreparingDash = false;
        isDashing = false;
        rb.velocity = new Vector2(0f, rb.velocity.y);

        if (dashCooldownRoutine != null)
            StopCoroutine(dashCooldownRoutine);

        dashCooldownRoutine = StartCoroutine(DashCooldownRoutine());
    }

    private IEnumerator DashCooldownRoutine()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void CheckPhaseTwoRest()
    {
        if (enteredPhaseTwoRest)
            return;

        if (currentHealth > phaseTwoHealthThreshold)
            return;

        enteredPhaseTwoRest = true;

        if (restRoutine != null)
            StopCoroutine(restRoutine);

        restRoutine = StartCoroutine(RestRoutine());
    }

    private IEnumerator RestRoutine()
    {
        isResting = true;
        isPreparingDash = false;
        isDashing = false;
        rb.velocity = Vector2.zero;

        animator.SetBool("Rest", true);

        yield return new WaitForSeconds(restDuration);

        animator.SetBool("Rest", false);
        isResting = false;
    }

    private void FacePlayer()
    {
        if (isPreparingDash || isDashing)
            return;

        if (player.position.x > transform.position.x && !facingRight)
            Flip();
        else if (player.position.x < transform.position.x && facingRight)
            Flip();
    }

    private void UpdateFacingFromDirection(float direction)
    {
        if (direction > 0f && !facingRight)
            Flip();
        else if (direction < 0f && facingRight)
            Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private bool HasGroundAhead()
    {
        return HasGroundAheadForDirection(dashDirection);
    }

    private bool HasGroundAheadForDirection(float direction)
    {
        if (edgeCheck == null)
            return true;

        Vector3 localPos = edgeCheck.localPosition;
        localPos.x = Mathf.Abs(localPos.x) * (direction >= 0f ? 1f : -1f);

        Vector3 worldPos = transform.TransformPoint(localPos);

        return Physics2D.OverlapCircle(worldPos, edgeCheckRadius, groundLayer);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandlePlayerContact(collision.collider);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        HandlePlayerContact(other);
    }

    private void HandlePlayerContact(Collider2D other)
    {
        if (isDead || other == null || !other.CompareTag("Player"))
            return;

        if (playerHealth != null && playerHealth.IsDead())
            return;

        Animator playerAnimator = other.GetComponent<Animator>();
        bool playerIsAttacking = false;

        if (playerAnimator != null)
        {
            AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
            playerIsAttacking = stateInfo.IsName("attack");
        }

        if (playerIsAttacking)
        {
            TryReceiveDamageFromPlayerAttack(other);
            return;
        }

        TryDamagePlayer(other);
    }

    private void TryDamagePlayer(Collider2D playerCollider)
    {
        if (Time.time < nextTouchDamageTime)
            return;

        PlayerHealth health = playerCollider.GetComponent<PlayerHealth>();
        Rigidbody2D playerRb = playerCollider.GetComponent<Rigidbody2D>();

        if (health == null || playerRb == null)
            return;

        nextTouchDamageTime = Time.time + touchDamageInterval;

        float dir = playerCollider.transform.position.x >= transform.position.x ? 1f : -1f;
        Vector2 knockback = new Vector2(dir * knockbackForceX, knockbackForceY);

        health.TakeDamage(touchDamage, knockback);
    }

    private void TryReceiveDamageFromPlayerAttack(Collider2D playerCollider)
    {
        if (Time.time < nextReceiveHitTime)
            return;

        Animator playerAnimator = playerCollider.GetComponent<Animator>();
        if (playerAnimator == null)
            return;

        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName("attack"))
            return;

        nextReceiveHitTime = Time.time + receiveHitCooldown;
        TakeDamage(damageFromPlayerAttack);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(HitFlashRoutine());

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator HitFlashRoutine()
    {
        if (spriteRenderer == null)
            yield break;

        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);

        if (!isDead)
            spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isPreparingDash = false;
        isDashing = false;
        isResting = false;
        canDash = false;

        rb.velocity = Vector2.zero;

        if (bodyCollider != null)
            bodyCollider.enabled = false;

        if (dashCooldownRoutine != null)
            StopCoroutine(dashCooldownRoutine);

        if (restRoutine != null)
            StopCoroutine(restRoutine);

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        animator.SetBool("Rest", false);
        animator.SetBool("Dead", true);
    }

    public void StartDashFromAnimationEvent()
    {
        if (isDead)
            return;

        isPreparingDash = false;
        isDashing = true;
        dashTimer = dashTime;
    }

    public void StopDashFromAnimationEvent()
    {
        if (isDead)
            return;

        StopDash();
    }

    public void DestroyAfterDeathAnimation()
    {
        Destroy(gameObject);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dashRange);

        if (edgeCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(edgeCheck.position, edgeCheckRadius);
        }
    }
    public bool IsDead()
    {
        return isDead;
    }
}