using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Estadísticas del Personaje")]
    public int maxHealth = 5;            // Corazones máximos
    public int currentHealth;
    public float maxSoul = 100f;         // Contenedor de alma/energía máxima
    public float currentSoul;
    public float soulGainPerHit = 15f;   // Cuánto ganas al pegarle a un enemigo
    public float soulCostToHeal = 33f;   // Cuánta energía cuesta curarse 1 vida

    [Header("Tiempos de Invulnerabilidad")]
    public float iFramesDuration = 1f;   // Tiempo de parpadeo sin recibir daño tras golpe
    private float iFramesCounter;
    public bool IsInvulnerable { get; set; }

    private float healTimer;
    private float timeRequiredToHeal = 0.8f; // Cuánto debes dejar presionado curar
    private PlayerStateMachine stateMachine;

    private void Awake()
    {
        stateMachine = GetComponent<PlayerStateMachine>();
        currentHealth = maxHealth;
        currentSoul = 0f;
    }

    private void Start()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.InitializeHealthUI(maxHealth);
            UIManager.Instance.UpdateHealthUI(currentHealth);
            UIManager.Instance.UpdateSoulUI(currentSoul, maxSoul);
        }
    }

    private void Update()
    {
        if (iFramesCounter > 0)
        {
            iFramesCounter -= Time.deltaTime;
            if (iFramesCounter <= 0) IsInvulnerable = false;
        }

        HandleSoulHealing();
    }

    public void TakeDamage(int damageAmount)
    {
        if (IsInvulnerable) return;

        currentHealth -= damageAmount;

        if (currentHealth > 0 && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealthUI(currentHealth);
        }

        Debug.Log($"¡Daño recibido! Vida restante: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            IsInvulnerable = true;
            iFramesCounter = iFramesDuration;

            float direction = stateMachine.IsFacingRight ? -1f : 1f;
            stateMachine.Rb.linearVelocity = Vector2.zero;
            stateMachine.Rb.AddForce(new Vector2(direction * 7f, 5f), ForceMode2D.Impulse);
        }
    }

    public void AddSoul(float amount)
    {
        currentSoul = Mathf.Clamp(currentSoul + amount, 0f, maxSoul);
        if (UIManager.Instance != null) UIManager.Instance.UpdateSoulUI(currentSoul, maxSoul);
        Debug.Log($"Alma recolectada: {currentSoul}/{maxSoul}");
    }

    public void UseSoul(float amount)
    {
        currentSoul = Mathf.Clamp(currentSoul - amount, 0f, maxSoul);
        if (UIManager.Instance != null) UIManager.Instance.UpdateSoulUI(currentSoul, maxSoul);
    }

    private void HandleSoulHealing()
    {
        if (stateMachine.HealHeld && stateMachine.IsGrounded() && currentHealth < maxHealth && currentSoul >= soulCostToHeal)
        {
            stateMachine.Rb.linearVelocity = new Vector2(0f, stateMachine.Rb.linearVelocity.y);
            healTimer += Time.deltaTime;

            if (healTimer >= timeRequiredToHeal)
            {
                currentHealth++;
                currentSoul -= soulCostToHeal;
                healTimer = 0f;
                Debug.Log($"¡Curación exitosa! Vida: {currentHealth}. Alma restante: {currentSoul}");

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateHealthUI(currentHealth);
                    UIManager.Instance.UpdateSoulUI(currentSoul, maxSoul);
                }
            }
        }
        else
        {
            healTimer = 0f;
        }
    }

    private void Die()
    {
        Debug.Log("¡El jugador ha muerto!");
        currentHealth = maxHealth;
        currentSoul = 0f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealthUI(currentHealth);
            UIManager.Instance.UpdateSoulUI(currentSoul, maxSoul);
        }

        if (GameManager.Instance != null && GameManager.Instance.hasActivatedCheckpoint)
        {
            transform.position = GameManager.Instance.lastCheckpointPosition;
        }
        else
        {
            transform.position = Vector3.zero;
        }
    }
}