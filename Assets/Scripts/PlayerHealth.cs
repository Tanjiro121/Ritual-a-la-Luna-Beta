using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth = 5;

    [Header("UI Vida")]
    [SerializeField] private MoonHeartUI moonHeartUI;

    [Header("Referencias")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [Header("Feedback")]
    [SerializeField] private float invulnerabilityTime = 0.4f;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color damageColor = new Color(1f, 0.6f, 0.6f, 1f);

    [Header("Muerte")]
    [SerializeField] private float restartDelay = 1.2f;

    [Header("Caida al vacio")]
    [SerializeField] private bool useFallDeath = true;
    [SerializeField] private float fallDeathY = -10f;

    private bool isDead;
    private bool isInvulnerable;
    private Color originalColor;
    private Coroutine flashRoutine;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        currentHealth = maxHealth;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        UpdateHealthUI(true);
    }

    private void Update()
    {
        if (isDead)
            return;

        if (useFallDeath && transform.position.y <= fallDeathY)
            InstantKill();
    }

    public void TakeDamage(int damage, Vector2 knockback)
    {
        if (isDead || isInvulnerable)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(DamageFlashRoutine());
        StartCoroutine(InvulnerabilityRoutine());

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
    }

    public void SetMaxHealth(int newMaxHealth, bool fillHealth = false)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);

        if (fillHealth)
            currentHealth = maxHealth;
        else
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI(true);
    }

    public void InstantKill()
    {
        if (isDead)
            return;

        currentHealth = 0;
        UpdateHealthUI();
        Die();
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (spriteRenderer == null)
            yield break;

        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(flashDuration);

        if (!isDead)
            spriteRenderer.color = originalColor;
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isInvulnerable = true;

        if (rb != null)
            rb.velocity = Vector2.zero;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        if (animator != null)
            animator.ResetTrigger("Attack");

        StartCoroutine(RestartSceneRoutine());
    }

    private IEnumerator RestartSceneRoutine()
    {
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateHealthUI(bool rebuild = false)
    {
        if (moonHeartUI == null)
            return;

        if (rebuild)
            moonHeartUI.BuildHearts(maxHealth);

        moonHeartUI.UpdateHearts(currentHealth);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }
}