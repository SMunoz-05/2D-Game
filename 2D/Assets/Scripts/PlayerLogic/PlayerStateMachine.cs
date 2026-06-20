using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    public Rigidbody2D Rb { get; private set; }
    public InputSystem_Actions InputActions { get; private set; }
    public PlayerHealth HealthSystem { get; private set; }

    [Header("Movimiento Horizontal")]
    public float moveSpeed = 9f;
    public float acceleration = 50f;
    public float deceleration = 60f;

    [Header("Salto Pro")]
    public float jumpForce = 13f;
    public float fallMultiplier = 3.5f;
    public float lowJumpMultiplier = 2.5f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    [Header("Mecánica de Dash")]
    public float dashForce = 22f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.6f;
    public float DashCooldownCounter { get; set; }

    [Header("Detección de Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Sistema de Combate")]
    public Transform attackPoint;
    public Transform attackPointUp;
    public Transform attackPointDown;
    public Vector2 attackRange = new Vector2(1.2f, 0.8f);
    public LayerMask enemyLayer;
    public float attackCooldown = 0.35f;
    public float pogoForce = 11f;
    public float knockbackForce = 5f;

    [Header("Costes y Ganancias de Alma")]
    public float chargedAttackSoulCost = 33f;
    public float soulGainOnNormalHit = 11f;
    public float soulGainOnChargedHit = 5f;

    [Header("Ataque Cargado")]
    public float timeToChargeAttack = 0.6f;
    public Vector2 chargedAttackRange = new Vector2(2.4f, 1.4f);
    public float chargedKnockbackForce = 12f;
    public float ChargeTimer { get; private set; }
    public bool IsChargingAttack { get; private set; }
    public bool ChargedAttackReady { get; private set; }

    public float CoyoteCounter { get; set; }
    public float JumpBufferCounter { get; set; }
    public float AttackCooldownCounter { get; set; }
    public Vector2 MovementInput { get; private set; }
    public bool IsJumpHeld { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool AttackReleased { get; private set; }
    public bool IsAttackHeld { get; private set; }
    public bool DashPressed { get; private set; }
    public bool HealHeld { get; private set; }
    public bool IsFacingRight { get; private set; } = true;

    private PlayerBaseState currentState;
    public PlayerIdleState IdleState { get; private set; }
    public PlayerRunningState RunningState { get; private set; }
    public PlayerInAirState InAirState { get; private set; }
    public PlayerAttackingState AttackingState { get; private set; }
    public PlayerDashingState DashingState { get; private set; }

    private float baseMoveSpeed;
    private Vector2 baseAttackRange;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        HealthSystem = GetComponent<PlayerHealth>();
        InputActions = new InputSystem_Actions();

        IdleState = new PlayerIdleState(this);
        RunningState = new PlayerRunningState(this);
        InAirState = new PlayerInAirState(this);
        AttackingState = new PlayerAttackingState(this);
        DashingState = new PlayerDashingState(this);
    }

    private void OnEnable() => InputActions.Enable();
    private void OnDisable() => InputActions.Disable();

    private void Start()
    {
        baseMoveSpeed = moveSpeed;
        baseAttackRange = attackRange;

        if (GameManager.Instance != null && GameManager.Instance.hasActivatedCheckpoint)
        {
            transform.position = GameManager.Instance.lastCheckpointPosition;
        }

        RecalculateStatsWithCharms();
        SwitchState(IdleState);
    }

    private void Update()
    {
        MovementInput = InputActions.Player.Move.ReadValue<Vector2>();
        IsJumpHeld = InputActions.Player.Jump.IsPressed();
        AttackPressed = InputActions.Player.Attack.WasPressedThisFrame();
        AttackReleased = InputActions.Player.Attack.WasReleasedThisFrame();
        IsAttackHeld = InputActions.Player.Attack.IsPressed();
        DashPressed = InputActions.Player.Sprint.WasPressedThisFrame();
        HealHeld = InputActions.Player.Interact.IsPressed();

        HandleChargeInput();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ToggleChargeIndicator(ChargedAttackReady);
        }

        if (InputActions.Player.Jump.WasPressedThisFrame()) JumpBufferCounter = jumpBufferTime;
        else JumpBufferCounter -= Time.deltaTime;

        CoyoteCounter = IsGrounded() ? coyoteTime : CoyoteCounter - Time.deltaTime;
        if (AttackCooldownCounter > 0) AttackCooldownCounter -= Time.deltaTime;
        if (DashCooldownCounter > 0) DashCooldownCounter -= Time.deltaTime;

        if (currentState != DashingState) HandleFlip();

        currentState.UpdateState();
    }

    private void FixedUpdate()
    {
        if (currentState != DashingState) ApplyBetterGravity();
        currentState.FixedUpdateState();
    }

    public void SwitchState(PlayerBaseState newState)
    {
        if (currentState != null) currentState.ExitState();
        currentState = newState;
        currentState.EnterState();
    }

    public bool IsGrounded() => Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

    private void HandleChargeInput()
    {
        bool hasEnoughSoul = HealthSystem != null && HealthSystem.currentSoul >= chargedAttackSoulCost;

        // Mientras dejes hundido el botón y tengas alma, sumará tiempo
        if (IsAttackHeld && AttackCooldownCounter <= 0 && !HealHeld && hasEnoughSoul)
        {
            if (PlayerJuiceEffects.Instance != null && ChargeTimer == 0f)
            {
                PlayerJuiceEffects.Instance.StartChargingEffect();
            }

            ChargeTimer += Time.deltaTime;

            if (ChargeTimer >= timeToChargeAttack && !ChargedAttackReady)
            {
                IsChargingAttack = true;
                ChargedAttackReady = true;

                if (PlayerJuiceEffects.Instance != null)
                {
                    PlayerJuiceEffects.Instance.PlayChargeReadyBurst();
                }

                Debug.LogWarning("¡ATAQUE CARGADO LISTO! (Suelta el botón para desatarlo)");
            }
        }
        else
        {
            // Apaga partículas si te interrumpen o te quedas sin alma.
            // IMPORTANTE: no reiniciar la carga en el mismo frame en que se soltó el botón
            // para que el estado actual pueda detectar `AttackReleased` y procesar el ataque cargado.
            if (!IsAttackHeld && !AttackReleased)
            {
                if (PlayerJuiceEffects.Instance != null && (ChargeTimer > 0f || ChargedAttackReady))
                {
                    PlayerJuiceEffects.Instance.StopChargingEffect();
                }
                ChargeTimer = 0f;
                IsChargingAttack = false;
                ChargedAttackReady = false;
            }
        }
    }

    public void ResetCharge()
    {
        ChargeTimer = 0f;
        IsChargingAttack = false;
        ChargedAttackReady = false;

        if (PlayerJuiceEffects.Instance != null)
        {
            PlayerJuiceEffects.Instance.StopChargingEffect();
        }
    }

    private void HandleFlip()
    {
        if (MovementInput.x > 0.1f && !IsFacingRight) Flip();
        else if (MovementInput.x < -0.1f && IsFacingRight) Flip();
    }

    private void Flip()
    {
        IsFacingRight = !IsFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void ApplyBetterGravity()
    {
        if (Rb.linearVelocity.y < 0) Rb.gravityScale = fallMultiplier;
        else if (Rb.linearVelocity.y > 0 && !IsJumpHeld) Rb.gravityScale = lowJumpMultiplier;
        else Rb.gravityScale = 1f;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPoint.position, attackRange);

            // Círculo de la Hitbox del Súper Ataque centrado perfectamente
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, chargedAttackRange.x);
        }
    }

    public void RecalculateStatsWithCharms()
    {
        if (GameManager.Instance == null) return;
        moveSpeed = baseMoveSpeed;
        attackRange = baseAttackRange;
        if (HealthSystem != null) HealthSystem.soulGainPerHit = 15f;

        foreach (CharmData charm in GameManager.Instance.equippedCharms)
        {
            moveSpeed *= charm.speedMultiplier;
            attackRange *= charm.attackRangeMultiplier;
            if (HealthSystem != null) HealthSystem.soulGainPerHit *= charm.soulGainMultiplier;
        }
    }
}