using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;           // Arrastra aquí a tu Player
    public float smoothing = 4f;       // Qué tan suave te sigue (números más bajos = más suave)
    public Vector3 offset = new Vector3(0f, 2f, -10f); // Separación ideal para ver el entorno

    [Header("Límites del Escenario (Bounds)")]
    public bool useBounds = false;     // Actívalo cuando definas los límites de tu mapa
    public Vector2 minCameraBounds;    // Coordenada X e Y mínima a la que puede ir la cámara
    public Vector2 maxCameraBounds;    // Coordenada X e Y máxima a la que puede ir la cámara

    private void Awake()
    {
        // Si se te olvida asignar el target, lo busca automáticamente por Tag
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 1. Calcular la posición deseada de la cámara con el offset
        Vector3 targetPosition = target.position + offset;

        // 2. Si activas los límites, restringe la posición matemática con un Clamp
        if (useBounds)
        {
            float clampedX = Mathf.Clamp(targetPosition.x, minCameraBounds.x, maxCameraBounds.x);
            float clampedY = Mathf.Clamp(targetPosition.y, minCameraBounds.y, maxCameraBounds.y);
            targetPosition = new Vector3(clampedX, clampedY, targetPosition.z);
        }

        // 3. Interpolación lineal (Lerp) para un deslizamiento suave de cineastas
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
    }
}