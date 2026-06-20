using System.Collections;
using UnityEngine;

public class HitStopEffect : MonoBehaviour
{
    public static HitStopEffect Instance { get; private set; }
    private bool isWaiting = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void TriggerStop(float duration)
    {
        if (!isWaiting && gameObject.activeInHierarchy)
        {
            StartCoroutine(WaitRoutine(duration));
        }
    }

    private IEnumerator WaitRoutine(float duration)
    {
        isWaiting = true;
        float originalTimeScale = Time.timeScale;

        // Congelar el tiempo físico por completo
        Time.timeScale = 0f;

        // Al estar el tiempo en 0, debemos usar WaitForSecondsRealtime para que el reloj corra en el mundo real
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;
        isWaiting = false;
    }
}