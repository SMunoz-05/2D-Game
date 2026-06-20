using UnityEngine;

public class PlayerJuiceEffects : MonoBehaviour
{
    public static PlayerJuiceEffects Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private ParticleSystem footParticles;

    private ParticleSystem.EmissionModule emissionModule;

    [SerializeField] private ParticleSystem chargeParticles; // Arrastra aquí el ChargeParticles

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (footParticles != null)
        {
            emissionModule = footParticles.emission;

            // Aseguramos que empiece limpio
            emissionModule.enabled = false;

            // Forzamos al sistema principal a estar en modo Play de forma infinita
            var main = footParticles.main;
            main.playOnAwake = false;
            main.loop = true;

            footParticles.Play();
        }
    }

    // ==========================================
    // 1. EFECTO AL CORRER (Flujo Continuo)
    // ==========================================
    public void SetRunningDust(bool isRunning, bool isFacingRight)
    {
        if (footParticles == null) return;

        // Si por algún motivo externo el sistema se pausó, lo despertamos
        if (!footParticles.isPlaying) footParticles.Play();

        if (isRunning)
        {
            emissionModule.enabled = true;

            // CORRECCIÓN MATEMÁTICA: Usamos rotación local absoluta.
            // Al cambiar la escala del padre, compensamos el cono de partículas.
            float angleY = isFacingRight ? 90f : 90f; // Al usar localScale negativo, 90 grados siempre apunta hacia atrás de su frente actual
            var shape = footParticles.shape;
            shape.rotation = new Vector3(0f, angleY, 0f);
        }
        else
        {
            emissionModule.enabled = false;
        }
    }

    // ==========================================
    // 2. EFECTO AL SALTAR (Bocanada de impacto)
    // ==========================================
    public void SpawnJumpDust()
    {
        if (footParticles == null) return;

        if (!footParticles.isPlaying) footParticles.Play();

        // Forzar alineación hacia el suelo de forma local independiente
        var shape = footParticles.shape;
        shape.rotation = new Vector3(-90f, 0f, 0f);

        // Emitir ráfaga instantánea
        footParticles.Emit(12);
    }

    // ==========================================
    // 3. EFECTO DEL DASH DE SOMBRA (Ráfaga horizontal)
    // ==========================================
    public void SpawnDashParticles(bool isFacingRight)
    {
        if (footParticles == null) return;

        if (!footParticles.isPlaying) footParticles.Play();

        // Alineación horizontal pura detrás del Dash
        var shape = footParticles.shape;
        shape.rotation = new Vector3(0f, 90f, 0f);

        footParticles.Emit(20);
    }
    // Enciende las partículas de absorción
    public void StartChargingEffect()
    {
        if (chargeParticles == null) return;

        if (!chargeParticles.isPlaying)
        {
            chargeParticles.Play();
        }
    }

    // Apaga el efecto (si suelta el botón o recibe daño)
    public void StopChargingEffect()
    {
        if (chargeParticles == null) return;
        chargeParticles.Stop();
    }

    // Un destello rápido de 30 partículas hacia afuera para avisar que ya está cargado al 100%
    public void PlayChargeReadyBurst()
    {
        if (chargeParticles == null) return;

        // Cambiamos temporalmente la velocidad a positiva para la explosión visual
        var main = chargeParticles.main;
        main.startSpeed = 5f;

        chargeParticles.Emit(30);

        // Devolvemos la velocidad a negativa en la siguiente rutina para que vuelva a absorber
        Invoke(nameof(ResetChargeSpeed), 0.1f);
    }

    private void ResetChargeSpeed()
    {
        if (chargeParticles == null) return;
        var main = chargeParticles.main;
        main.startSpeed = -3f;
    }
}