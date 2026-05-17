using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;

    [Header("Invincibility")]
    [Tooltip("Hasar aldıktan sonra kaç saniye dokunulmaz olunacak")]
    [SerializeField] private float invincibilityDuration = 1f;
    [Tooltip("Dokunulmazlık sırasında sprite saniyede kaç kere yanıp sönsün")]
    [SerializeField] private float blinkFrequency = 10f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackDuration = 0.2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerMovement playerMovement;

    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private float knockbackTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        HandleInvincibility();
        HandleKnockback();
    }

    public void TakeDamage(int damage, Vector2 damageSourcePosition)
    {
        // Zaten dokunulmazsa hasar alma
        if (isInvincible) return;

        currentHealth -= damage;
        Debug.Log("Hasar alındı! Kalan can: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Knockback uygula (hasar kaynağının zıt yönüne savrul)
        ApplyKnockback(damageSourcePosition);

        // Dokunulmazlığı başlat
        StartInvincibility();
    }

    private void ApplyKnockback(Vector2 damageSourcePosition)
    {
        // Hasar kaynağından oyuncuya doğru yön vektörü
        Vector2 knockbackDirection = ((Vector2)transform.position - damageSourcePosition).normalized;

        // Yukarı doğru bir miktar da it (daha tatmin edici his)
        knockbackDirection.y = Mathf.Abs(knockbackDirection.y) + 0.5f;

        rb.linearVelocity = knockbackDirection * knockbackForce;
        knockbackTimer = knockbackDuration;

        // Knockback sırasında oyuncu kontrolü kapat
        if (playerMovement != null) playerMovement.enabled = false;
    }

    private void HandleKnockback()
    {
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0 && playerMovement != null)
            {
                playerMovement.enabled = true;
            }
        }
    }

    private void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    private void HandleInvincibility()
    {
        if (!isInvincible) return;

        invincibilityTimer -= Time.deltaTime;

        // Sprite yanıp söndür (görsel feedback)
        float blink = Mathf.PingPong(Time.time * blinkFrequency, 1f);
        Color color = spriteRenderer.color;
        color.a = blink > 0.5f ? 1f : 0.3f;
        spriteRenderer.color = color;

        if (invincibilityTimer <= 0)
        {
            isInvincible = false;
            // Sprite'ı normal opaklığa döndür
            Color color2 = spriteRenderer.color;
            color2.a = 1f;
            spriteRenderer.color = color2;
        }
    }

    private void Die()
    {
        Debug.Log("Oyuncu öldü!");
        GameManager.Instance.OnPlayerDeath();
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}