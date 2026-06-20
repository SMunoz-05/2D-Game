using UnityEngine;

public class FlyingEnemyAI : EnemyBase
{
    [Header("Configuración de Vuelo")]
    public float flySpeed = 4f;
    public float alertRange = 7f;
    public float stopDistance = 3.5f; // Distancia a la que se queda quieto a disparar

    [Header("Ataque a Distancia")]
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float fireRate = 1.5f;
    private float fireCooldown;

    private Transform playerTransform;
    private bool isChasing = false;

    protected override void Awake()
    {
        base.Awake();
        // Al ser volador, le quitamos el impacto de la gravedad normal para que flote
        rb.gravityScale = 0f;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void Update()
    {
        if (isDead || playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (!isChasing)
        {
            if (distanceToPlayer <= alertRange) isChasing = true;
        }
        else
        {
            HandleFlyingMovement(distanceToPlayer);
            HandleShooting(distanceToPlayer);
        }
    }

    private void HandleFlyingMovement(float distance)
    {
        // Si está más lejos de la distancia de disparo, se acerca al jugador flotando suavemente
        if (distance > stopDistance)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * flySpeed;
        }
        else
        {
            // Si está muy cerca, se frena en seco en el aire para apuntar
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.deltaTime * 3f);
        }

        // Girar el sprite para mirar al jugador
        if (playerTransform.position.x > transform.position.x && transform.localScale.x < 0) Flip();
        else if (playerTransform.position.x < transform.position.x && transform.localScale.x > 0) Flip();
    }

    private void HandleShooting(float distance)
    {
        if (fireCooldown > 0) fireCooldown -= Time.deltaTime;

        // Si está en rango y el cooldown está listo, dispara un proyectil lúgubre
        if (distance <= alertRange && fireCooldown <= 0)
        {
            fireCooldown = fireRate;
            ShootProjectile();
        }
    }

    private void ShootProjectile()
    {
        if (projectilePrefab != null && shootPoint != null)
        {
            GameObject proj = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
            Vector2 shootDirection = (playerTransform.position - shootPoint.position).normalized;

            // Inicializar el proyectil pasándole la dirección hacia el jugador
            EnemyProjectile script = proj.GetComponent<EnemyProjectile>();
            if (script != null) script.Setup(shootDirection, damageToPlayer);
        }
    }

    private void Flip()
    {
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }
}