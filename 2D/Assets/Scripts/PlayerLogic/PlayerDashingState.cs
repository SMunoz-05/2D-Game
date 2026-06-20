using UnityEngine;

public class PlayerDashingState : PlayerBaseState
{
    private float dashTimer;
    private float originalGravity;

    public PlayerDashingState(PlayerStateMachine currentContext) : base(currentContext) { }

    public override void EnterState()
    {
        // CORRECCIÓN: Llamamos al nuevo método Pro del Sistema de Partículas esféricas
        if (PlayerJuiceEffects.Instance != null)
        {
            PlayerJuiceEffects.Instance.SpawnDashParticles(stateMachine.IsFacingRight);
        }

        dashTimer = stateMachine.dashDuration;
        stateMachine.DashCooldownCounter = stateMachine.dashCooldown;

        // Guardamos gravedad original y la congelamos en 0 para un dash perfectamente horizontal
        originalGravity = stateMachine.Rb.gravityScale;
        stateMachine.Rb.gravityScale = 0f;

        // Hacer al jugador invulnerable a los enemigos mientras use el Dash de Sombra
        stateMachine.HealthSystem.IsInvulnerable = true;

        // Ejecutar el impulso en la dirección correcta en la que está mirando
        float dashDirection = stateMachine.IsFacingRight ? 1f : -1f;
        stateMachine.Rb.linearVelocity = new Vector2(dashDirection * stateMachine.dashForce, 0f);

        stateMachine.Rb.gravityScale = originalGravity;
        stateMachine.HealthSystem.IsInvulnerable = false;
        // La velocidad horizontal acumulada se queda intacta y "resbala"

        Debug.Log("¡DASH DE SOMBRA ACTIVADO!");
    }

    public override void UpdateState()
    {
        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0)
        {
            if (stateMachine.IsGrounded())
                stateMachine.SwitchState(stateMachine.IdleState);
            else
                stateMachine.SwitchState(stateMachine.InAirState);
        }
    }

    public override void FixedUpdateState()
    {
        // Mantener la fuerza constante durante toda la duración corta del Dash
        float dashDirection = stateMachine.IsFacingRight ? 1f : -1f;
        stateMachine.Rb.linearVelocity = new Vector2(dashDirection * stateMachine.dashForce, 0f);
    }

    public override void ExitState()
    {
        // 1. Restauramos la gravedad y apagamos la invulnerabilidad (Tu lógica original)
        stateMachine.Rb.gravityScale = originalGravity;
        stateMachine.HealthSystem.IsInvulnerable = false;

        // 2. CORTE DE INERCIA PRO: Si el jugador NO está presionando movimiento horizontal al salir del Dash...
        if (Mathf.Abs(stateMachine.MovementInput.x) < 0.01f)
        {
            // Frenamos en seco la velocidad en X (la dejamos al 15%) para un frenado de emergencia limpio
            stateMachine.Rb.linearVelocity = new Vector2(stateMachine.Rb.linearVelocity.x * 0.15f, stateMachine.Rb.linearVelocity.y);
        }
        else
        {
            // Si el jugador SÍ está presionando hacia una dirección, suavizamos la transición 
            // reduciendo la velocidad supersónica del dash a la velocidad máxima de carrera normal
            float clampedX = Mathf.Clamp(stateMachine.Rb.linearVelocity.x, -stateMachine.moveSpeed, stateMachine.moveSpeed);
            stateMachine.Rb.linearVelocity = new Vector2(clampedX, stateMachine.Rb.linearVelocity.y);
        }
    }
}