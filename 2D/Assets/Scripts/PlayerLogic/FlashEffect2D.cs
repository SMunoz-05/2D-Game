using System.Collections;
using UnityEngine;

/// <summary>
/// FlashEffect2D: reusable visual feedback component for 2D sprites.
/// - Can flash using a temporary material (solid white) or by tinting the SpriteRenderer.color.
/// - Optional particle spawn helper that orients a particle prefab toward the hit direction.
/// Attach to Player or Enemy root and call from TakeDamage(...).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class FlashEffect2D : MonoBehaviour
{
    [Header("Flash Settings")]
    [Tooltip("Duration of the white flash in seconds.")]
    [SerializeField] private float flashDuration = 0.07f;

    [Tooltip("Color to use for the flash (usually solid white).")]
    [SerializeField] private Color flashColor = Color.white;

    [Tooltip("If true, switches material temporarily. If false, modifies SpriteRenderer.color.")]
    [SerializeField] private bool useMaterialFlash = true;

    [Tooltip("Optional material to use for the white flash. If null, will clone the current material and tint it.")]
    [SerializeField] private Material overrideFlashMaterial;

    [Header("Particles (optional)")]
    [Tooltip("Optional particle prefab (ParticleSystem) instantiated on hit.")]
    [SerializeField] private GameObject hitParticlePrefab;

    [Tooltip("Approximate lifetime in seconds for the spawned particle GameObject.")]
    [SerializeField] private float particleLifetime = 0.8f;

    [Tooltip("Optional multiplier for particle start speed (if particle system supports it).")]
    [SerializeField] private float particleSpeed = 3f;

    // Internal
    [Tooltip("Optional explicit targets. If empty, all SpriteRenderers in this object and children will be used.")]
    [SerializeField] private SpriteRenderer[] targetRenderers;

    private SpriteRenderer[] runtimeRenderers;
    private Material[] originalMaterials;
    private Color[] originalColors;
    private Coroutine flashCoroutine;
    private MaterialPropertyBlock[] runtimePropertyBlocks; // per-renderer property block

    private void Awake()
    {
        // Resolve target renderers: use explicit targets if provided, otherwise gather all SpriteRenderers in children
        if (targetRenderers != null && targetRenderers.Length > 0)
        {
            runtimeRenderers = targetRenderers;
        }
        else
        {
            runtimeRenderers = GetComponentsInChildren<SpriteRenderer>();
        }

        if (runtimeRenderers == null || runtimeRenderers.Length == 0)
        {
            Debug.LogWarning("FlashEffect2D: No SpriteRenderer found in target object or children. Disabling.");
            enabled = false;
            return;
        }

        // Cache original materials/colors per renderer
        originalMaterials = new Material[runtimeRenderers.Length];
        originalColors = new Color[runtimeRenderers.Length];
        runtimePropertyBlocks = new MaterialPropertyBlock[runtimeRenderers.Length];

        for (int i = 0; i < runtimeRenderers.Length; i++)
        {
            var sr = runtimeRenderers[i];
            originalMaterials[i] = sr.material;
            originalColors[i] = sr.color;
            runtimePropertyBlocks[i] = new MaterialPropertyBlock();
            sr.GetPropertyBlock(runtimePropertyBlocks[i]);
        }
    }

    private void OnDisable()
    {
        // Ensure we restore original visuals if the component is disabled
        RestoreOriginalVisuals();
    }

    /// <summary>
    /// Public: trigger a flash only (no particles).
    /// </summary>
    public void FlashOnly()
    {
        StartFlashInternal(null);
    }

    /// <summary>
    /// Public: trigger a flash and spawn hit particles oriented along hitDirection.
    /// </summary>
    public void FlashAndParticles(Vector2 hitDirection)
    {
        Debug.Log("FlashEffect2D: FlashAndParticles called. hitDirection=" + hitDirection);
        StartFlashInternal(hitDirection);
    }

    private void StartFlashInternal(Vector2? hitDirection)
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            RestoreOriginalVisuals();
        }

        if (hitDirection.HasValue)
        {
            SpawnHitParticles(hitDirection.Value);
        }

        flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        // Use MaterialPropertyBlock to flash color without instantiating materials
        for (int i = 0; i < runtimeRenderers.Length; i++)
        {
            var sr = runtimeRenderers[i];
            if (sr == null) continue;

            // If using color fallback, set SpriteRenderer.color directly (compat)
            if (!useMaterialFlash)
            {
                originalColors[i] = sr.color;
                sr.color = flashColor;
                continue;
            }

            var mpb = runtimePropertyBlocks[i] ?? new MaterialPropertyBlock();
            sr.GetPropertyBlock(mpb);
            // Try common property names first
            if (mpb != null)
            {
                if (mpb != null) mpb.SetColor("_Color", flashColor);
                sr.SetPropertyBlock(mpb);
                runtimePropertyBlocks[i] = mpb;
            }
            else
            {
                // fallback to directly tinting color
                originalColors[i] = sr.color;
                sr.color = flashColor;
            }
        }

        yield return new WaitForSecondsRealtime(flashDuration);

        RestoreOriginalVisuals();
        flashCoroutine = null;
    }

    private void RestoreOriginalVisuals()
    {
        if (runtimeRenderers == null) return;

        for (int i = 0; i < runtimeRenderers.Length; i++)
        {
            var sr = runtimeRenderers[i];
            if (sr == null) continue;

            // Restore property block color if we used it
            if (useMaterialFlash)
            {
                var mpb = runtimePropertyBlocks[i] ?? new MaterialPropertyBlock();
                mpb.SetColor("_Color", originalColors[i]);
                sr.SetPropertyBlock(mpb);
            }
            else
            {
                sr.color = originalColors[i];
            }
        }
    }

    private void SpawnHitParticles(Vector2 hitDirection)
    {
        // Instantiate at sprite pivot / transform position
        Vector3 spawnPos = (runtimeRenderers != null && runtimeRenderers.Length > 0 && runtimeRenderers[0] != null)
            ? runtimeRenderers[0].transform.position
            : transform.position;

        if (hitParticlePrefab != null)
        {
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, hitDirection.normalized);
            GameObject go = Instantiate(hitParticlePrefab, spawnPos, rot);
            var ps = go.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.startSpeed = particleSpeed;
                var pr = ps.GetComponent<ParticleSystemRenderer>() ?? ps.GetComponentInChildren<ParticleSystemRenderer>();
                if (pr != null && runtimeRenderers != null && runtimeRenderers.Length > 0 && runtimeRenderers[0] != null)
                {
                    pr.sortingLayerID = runtimeRenderers[0].sortingLayerID;
                    pr.sortingOrder = runtimeRenderers[0].sortingOrder + 1;
                }
                ps.Play();
                Debug.Log($"FlashEffect2D: Spawned hit particles on '{gameObject.name}' (ps={ps.name}).");
            }
            else
            {
                Debug.LogWarning($"FlashEffect2D: No ParticleSystem found in prefab '{hitParticlePrefab.name}'.");
            }
            Destroy(go, particleLifetime);
            return;
        }

        // If no prefab assigned, create a simple temporary ParticleSystem for quick testing
        Debug.Log("FlashEffect2D: hitParticlePrefab not assigned - spawning temporary test particles.");
        GameObject tmp = new GameObject("TempHitParticles");
        tmp.transform.position = spawnPos;
        tmp.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitDirection.normalized);
        var psTemp = tmp.AddComponent<ParticleSystem>();
        var mainTemp = psTemp.main;
        mainTemp.duration = 0.25f;
        mainTemp.startLifetime = 0.25f;
        mainTemp.startSpeed = particleSpeed;
        mainTemp.startSize = 0.12f;
        mainTemp.loop = false;
        mainTemp.simulationSpace = ParticleSystemSimulationSpace.World;

        var em = psTemp.emission;
        em.rateOverTime = 0;
        em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 6) });

        var sh = psTemp.shape;
        sh.shapeType = ParticleSystemShapeType.Cone;
        sh.angle = 20f;
        sh.radius = 0.05f;

        var prTemp = psTemp.GetComponent<ParticleSystemRenderer>();
        if (prTemp != null && runtimeRenderers != null && runtimeRenderers.Length > 0 && runtimeRenderers[0] != null)
        {
            prTemp.sortingLayerID = runtimeRenderers[0].sortingLayerID;
            prTemp.sortingOrder = runtimeRenderers[0].sortingOrder + 1;
        }

        psTemp.Play();
        Destroy(tmp, particleLifetime);
    }
}
