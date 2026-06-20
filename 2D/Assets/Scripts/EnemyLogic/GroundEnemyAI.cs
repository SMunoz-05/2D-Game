using UnityEngine;

public class GroundEnemyAI : EnemyBase
{
    public enum EnemyState { Patrolling, Chasing }
    [Header("Configuración de IA")]
    public EnemyState currentState = EnemyState.Patrolling;
    public float walkSpeed = 3f;
    public float chaseSpeed = 5.5f;
    public float targetDetectionRange = 6f;

    [Header("Sensores de Entorno")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private float sensorRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Transform playerTransform;
    private int walkDirection = 1; // 1 = Derecha, -1 = Izquierda

    protected override void Awake()
    {
        base.Awake();
        // Busca al jugador de forma segura por su Tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void Update()
    {
        if (isDead) return;

        // Máquina de estados de comportamiento
        switch (currentState)
        {
            case EnemyState.Patrolling:
                PatrolBehavior();
                CheckForPlayer();
                break;

            case EnemyState.Chasing:
                ChaseBehavior();
                break;
        }
    }

    private void PatrolBehavior()
    {
        // Detectar si va a chocar contra una pared o si se va a caer de la plataforma (Ledge)
        bool hittingWall = Physics2D.OverlapCircle(wallCheck.position, sensorRadius, groundLayer);
        bool nearLedge = !Physics2D.OverlapCircle(ledgeCheck.position, sensorRadius, groundLayer);

        if (hittingWall || nearLedge)
        {
            FlipDirection();
        }

        rb.linearVelocity = new Vector2(walkDirection * walkSpeed, rb.linearVelocity.y);
    }

    private void ChaseBehavior()
    {
        if (playerTransform == null) return;

        // Calcular la distancia y dirección hacia el jugador en X
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        float directionToPlayer = playerTransform.position.x - transform.position.x;

        // Si el jugador se aleja demasiado, pierde el rastro y vuelve a patrullar
        if (distanceToPlayer > targetDetectionRange * 1.3f)
        {
            currentState = EnemyState.Patrolling;
            return;
        }

        // Definir dirección de carrera hacia el jugador
        int newDirection = directionToPlayer > 0 ? 1 : -1;
        if (newDirection != walkDirection)
        {
            FlipDirection();
        }

        rb.linearVelocity = new Vector2(walkDirection * chaseSpeed, rb.linearVelocity.y);
    }

    private void CheckForPlayer()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        if (distance <= targetDetectionRange)
        {
            currentState = EnemyState.Chasing;
            Debug.LogWarning("¡Enemigo Terrestre en ALERTA! Persiguiendo al jugador.");
        }
    }

    private void FlipDirection()
    {
        walkDirection *= -1;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void OnDrawGizmos()
    {
        if (wallCheck != null && ledgeCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(wallCheck.position, sensorRadius);
            Gizmos.DrawWireSphere(ledgeCheck.position, sensorRadius);
        }

        // CORRECCIÓN: Usamos DrawWireSphere para pintar el radio de alerta en el editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, targetDetectionRange);
    }
}