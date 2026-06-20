using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentContext) : base(currentContext) { }

    public override void EnterState() { }

    public override void UpdateState()
    {
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

        if (Mathf.Abs(stateMachine.MovementInput.x) > 0.01f)
        {
            stateMachine.SwitchState(stateMachine.RunningState);
            return;
        }
    }

    public override void FixedUpdateState()
    {
        float targetSpeed = 0f;
        float speedDif = targetSpeed - stateMachine.Rb.linearVelocity.x;
        float movement = speedDif * stateMachine.deceleration;
        stateMachine.Rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    public override void ExitState() { }
}