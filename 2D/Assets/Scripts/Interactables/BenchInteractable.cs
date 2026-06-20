using UnityEngine;

public class BenchInteractable : MonoBehaviour
{
    [Header("Configuración Visual Visual")]
    [SerializeField] private Color activeColor = Color.yellow;
    private SpriteRenderer spriteRenderer;
    private bool playerInRange = false;
    private bool isActivated = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Presiona 'Interactuar' (E / Botón Mando) para descansar en el Banco.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        // Si el jugador está cerca y presiona el botón de interacción (mapeado en el Módulo A)
        if (playerInRange)
        {
            PlayerStateMachine player = Object.FindAnyObjectByType<PlayerStateMachine>();

            if (player != null && player.HealHeld) // Detecta que mantiene presionado interactuar
            {
                SaveAndHealPlayer(player);
            }
        }
    }

    private void SaveAndHealPlayer(PlayerStateMachine player)
    {
        if (!isActivated)
        {
            isActivated = true;
            if (spriteRenderer != null) spriteRenderer.color = activeColor; // Feedback en bloques
        }

        // 1. Guardar la posición exacta en el GameManager global
        GameManager.Instance.lastCheckpointPosition = transform.position;
        GameManager.Instance.hasActivatedCheckpoint = true;

        // 2. Curar por completo al jugador de forma gratuita
        if (player.HealthSystem != null)
        {
            player.HealthSystem.currentHealth = player.HealthSystem.maxHealth;
            // Opcionalmente rellenar el Alma al descansar
            player.HealthSystem.currentSoul = player.HealthSystem.maxSoul;
        }

        Debug.LogWarning("¡Partida Guardada! Salud y Alma restauradas en el Banco.");
    }
}