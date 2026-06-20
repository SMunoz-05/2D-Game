using System.Collections;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Estadísticas Base")]
    public int maxHealth = 3;
    protected int currentHealth;
    public int damageToPlayer = 1;
    public float knockbackForce = 6f;

    [Header("Feedback Visual (Pulido)")]
    public float flashDuration = 0.12f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Material originalMaterial;

    protected Rigidbody2D rb;
    protected bool isDead = false;
    private Coroutine flashCoroutine;
    private FlashEffect2D flashEffect;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Try to get an optional FlashEffect2D on the same object (or children)
        flashEffect = GetComponent<FlashEffect2D>();
        if (flashEffect == null)
            flashEffect = GetComponentInChildren<FlashEffect2D>();
    }

    // El método de daño ahora incluye el peso físico del HitStop
    public virtual void TakeDamage(int damageAmount, Vector2 attackDirection)
    {
        Debug.Log($"EnemyBase.TakeDamage called on '{gameObject.name}' dmg={damageAmount} dir={attackDirection}");

        if (isDead) return;

        currentHealth -= damageAmount;

        // 1. Activar el congelamiento de fotogramas (Hit Stop) para dar sensación de peso
        HitStopEffect.Instance.TriggerStop(0.08f);

        // 2. Activar el destello blanco de daño
        TriggerFlash();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Retroceso físico al enemigo
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(attackDirection * knockbackForce, ForceMode2D.Impulse);
        }
    }

    public void TriggerFlash()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        if (gameObject.activeInHierarchy) flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        if (spriteRenderer == null) yield break;

        // Versión Pro compatible con bloques: Lo pintamos de Rojo/Blanco brillante temporalmente
        spriteRenderer.color = Color.red; // Si usas Sprites finales, aquí cambiarías al Shader de Blanco Puro

        yield return new WaitForSeconds(flashDuration);

        spriteRenderer.color = originalColor;
    }

    protected virtual void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        // Pequeño impulso hacia arriba estilo muerte dramática antes de desaparecer
        rb.AddForce(Vector2.up * 4f, ForceMode2D.Impulse);
        if (spriteRenderer != null) spriteRenderer.color = Color.black; // Se oscurece al morir

        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(0.2f); // Retraso de muerte para apreciar el impacto
        Destroy(gameObject);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damageToPlayer);
            }
        }
    }
}