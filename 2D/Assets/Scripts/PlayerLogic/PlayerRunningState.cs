using UnityEngine;

public class PlayerRunningState : PlayerBaseState
{
    public PlayerRunningState(PlayerStateMachine currentContext) : base(currentContext) { }

    public override void EnterState()
    {
        if (PlayerJuiceEffects.Instance != null)
        {
            PlayerJuiceEffects.Instance.SetRunningDust(true, stateMachine.IsFacingRight);
        }
    }

    public override void UpdateState()
    {
        if (PlayerJuiceEffects.Instance != null)
        {
            PlayerJuiceEffects.Instance.SetRunningDust(true, stateMachine.IsFacingRight);
        }

        if (stateMachine.AttackReleased && stateMachine.ChargedAttackReady)
        {
            stateMachine.SwitchState(stateMachine.AttackingState);
            return;
        }

        if (stateMachine.AttackPressed && !stateMachine.IsChargingAttack && stateMachine.AttackCooldownCounter <= 0)
        {
            stateMachine.SwitchState(stateMachine.AttackingState);
            return;
        }

        if (stateMachine.DashPressed && stateMachine.DashCooldownCounter <= 0)
        {
            stateMachine.SwitchState(stateMachine.DashingState);
            return;
        }

        if (!stateMachine.IsGrounded())
        {
            stateMachine.SwitchState(stateMachine.InAirState);
            return;
        }

        if (stateMachine.JumpBufferCounter > 0 && stateMachine.CoyoteCounter > 0)
        {
            stateMachine.SwitchState(stateMachine.InAirState);
            return;
        }

        if (Mathf.Abs(stateMachine.MovementInput.x) < 0.01f)
        {
            stateMachine.SwitchState(stateMachine.IdleState);
            return;
        }
    }

    public override void FixedUpdateState()
    {
        float targetSpeed = stateMachine.MovementInput.x * stateMachine.moveSpeed;
        float speedDif = targetSpeed - stateMachine.Rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? stateMachine.acceleration : stateMachine.deceleration;
        float movement = speedDif * accelRate;

        stateMachine.Rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    public override void ExitState()
    {
        if (PlayerJuiceEffects.Instance != null)
        {
            PlayerJuiceEffects.Instance.SetRunningDust(false, stateMachine.IsFacingRight);
        }
    }
}