using UnityEngine;

public class PlayerInAirState : PlayerBaseState
{
    public PlayerInAirState(PlayerStateMachine currentContext) : base(currentContext) { }

    public override void EnterState()
    {
        if (stateMachine.JumpBufferCounter > 0 && stateMachine.CoyoteCounter > 0)
        {
            ExecuteJump();
        }
    }

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

        if (stateMachine.IsGrounded() && stateMachine.Rb.linearVelocity.y <= 0.01f)
        {
            if (Mathf.Abs(stateMachine.MovementInput.x) > 0.01f)
                stateMachine.SwitchState(stateMachine.RunningState);
            else
                stateMachine.SwitchState(stateMachine.IdleState);

            return;
        }
    }

    public override void FixedUpdateState()
    {
        float targetSpeed = stateMachine.MovementInput.x * stateMachine.moveSpeed;
        float speedDif = targetSpeed - stateMachine.Rb.linearVelocity.x;
        float movement = speedDif * (stateMachine.acceleration * 0.8f);

        stateMachine.Rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void ExecuteJump()
    {
        stateMachine.Rb.linearVelocity = new Vector2(stateMachine.Rb.linearVelocity.x, 0f);
        stateMachine.Rb.AddForce(Vector2.up * stateMachine.jumpForce, ForceMode2D.Impulse);

        if (PlayerJuiceEffects.Instance != null)
        {
            PlayerJuiceEffects.Instance.SpawnJumpDust();
        }

        stateMachine.JumpBufferCounter = 0;
        stateMachine.CoyoteCounter = 0;
    }

    public override void ExitState() { }
}