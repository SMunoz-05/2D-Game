using UnityEngine;

public class PlayerAttackingState : PlayerBaseState
{
    private float attackTimer;
    private Vector2 strikeDirection;
    // Reusable buffer to avoid allocations when checking overlaps
    private Collider2D[] overlapBuffer = new Collider2D[32];

    public PlayerAttackingState(PlayerStateMachine currentContext) : base(currentContext) { }

    public override void EnterState()
    {
        strikeDirection = stateMachine.IsFacingRight ? Vector2.right : Vector2.left;

        // Comprobación de origen: Si veníamos con el botón cargado listo
        if (stateMachine.ChargedAttackReady)
        {
            ExecuteChargedAttack();
        }
        else
        {
            ExecuteNormalAttack();
        }

        attackTimer = stateMachine.attackCooldown;
        stateMachine.AttackCooldownCounter = stateMachine.attackCooldown;
    }

    private void ExecuteNormalAttack()
    {
        Debug.Log("Ejecutando Ataque Normal.");
        int hitCount = Physics2D.OverlapBoxNonAlloc(stateMachine.attackPoint.position, stateMachine.attackRange, 0f, overlapBuffer, stateMachine.enemyLayer);
        for (int i = 0; i < hitCount; i++)
        {
            var enemy = overlapBuffer[i];
            if (enemy == null) continue;

            EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
            if (enemyScript == null) enemyScript = enemy.GetComponentInParent<EnemyBase>();

            if (enemyScript != null && HitStopEffect.Instance != null)
            {
                stateMachine.HealthSystem.AddSoul(stateMachine.soulGainOnNormalHit);
                enemyScript.TakeDamage(1, strikeDirection * stateMachine.knockbackForce);
                HitStopEffect.Instance.TriggerStop(0.07f);
            }
        }
    }

    private void ExecuteChargedAttack()
    {
        Debug.LogWarning("--- [INICIO] EJECUTANDO ATAQUE CARGADO ---");

        // 1. Cobrar coste de alma
        stateMachine.HealthSystem.UseSoul(stateMachine.chargedAttackSoulCost);

        // 2. Activar Hitbox circular
        float attackRadius = stateMachine.chargedAttackRange.x;
        int hitCount = Physics2D.OverlapCircleNonAlloc(stateMachine.attackPoint.position, attackRadius, overlapBuffer, stateMachine.enemyLayer);

        // RASTREADOR 1: ¿El círculo matemático de Unity encuentra algún objeto con Collider en esa capa?
        Debug.Log($"[Rastreador 1] Radio del círculo: {attackRadius}. Total de Colliders encontrados: {hitCount}");

        // 3. Dañar a todos los que entren en el círculo
        for (int i = 0; i < hitCount; i++)
        {
            var enemy = overlapBuffer[i];
            if (enemy == null) continue;

            // RASTREADOR 2: Ver qué objeto tocó y en qué capa está asignado en el Inspector
            Debug.Log($"[Rastreador 2] Collider detectado: '{enemy.name}' | Capa (Layer) del objeto: {LayerMask.LayerToName(enemy.gameObject.layer)}");

            EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();

            // RASTREADOR 3: ¿Tiene el script de vida/lógica base del enemigo?
            if (enemyScript == null)
            {
                // Si entra aquí, el objeto está en la capa correcta, pero no tiene el componente EnemyBase heredado
                Debug.LogError($"[Rastreador 3 ERROR] El objeto '{enemy.name}' NO tiene el script EnemyBase o uno que herede de él.");

                // Intento de rescate por si el Collider está en un hijo y el script en el padre:
                enemyScript = enemy.GetComponentInParent<EnemyBase>();
                if (enemyScript != null) Debug.LogWarning($"[Rastreador 3 RESCATE] Se encontró EnemyBase en el padre de '{enemy.name}'!");
            }

            if (enemyScript != null)
            {
                Vector2 pushDirection = (enemy.transform.position - stateMachine.attackPoint.position).normalized;

                stateMachine.HealthSystem.AddSoul(stateMachine.soulGainOnChargedHit);

                // RASTREADOR 4: Justo antes de mandar la orden de daño
                Debug.LogWarning($"[Rastreador 4] Aplicando 3 de daño exitosamente a '{enemy.name}'");

                enemyScript.TakeDamage(3, pushDirection * stateMachine.chargedKnockbackForce);
            }
        }

        // 4. Hitstop pesado
        if (HitStopEffect.Instance != null) HitStopEffect.Instance.TriggerStop(0.16f);

        // 5. Partículas finales de estallido
        if (PlayerJuiceEffects.Instance != null) PlayerJuiceEffects.Instance.PlayChargeReadyBurst();

        // 6. Limpieza absoluta
        stateMachine.ResetCharge();

        Debug.LogWarning("--- [FIN] ATAQUE CARGADO PROCESADO ---");
    }

    public override void UpdateState()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0)
        {
            if (stateMachine.IsGrounded())
                stateMachine.SwitchState(stateMachine.IdleState);
            else
                stateMachine.SwitchState(stateMachine.InAirState);
        }
    }

    public override void FixedUpdateState() { }
    public override void ExitState() { }
}