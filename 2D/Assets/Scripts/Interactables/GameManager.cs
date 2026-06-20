using System;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Persistencia de Posición")]
    public Vector3 lastCheckpointPosition = Vector3.zero;
    public bool hasActivatedCheckpoint = false;

    [Header("Sistema de Inventario de Amuletos")]
    public List<CharmData> unlockedCharms = new List<CharmData>();
    public List<CharmData> equippedCharms = new List<CharmData>();

    [Header("Muescas (Notches)")]
    public int totalNotchSlots = 3;
    public int occupiedNotchSlots = 0;

    [Header("Estado del Jugador")]
    public bool isSittingOnBench = false;

    // Evento global para notificar cambios en los amuletos equipados
    public static Action OnCharmsChanged;

    private void Awake()
    {
        // Implementación de Singleton estricto para portafolio
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Evita que se destruya al cambiar de escena
    }

    // Métodos globales para equipar/desequipar desde el menú o el banco

    public bool CanEquipCharm(CharmData charm)
    {
        if (charm == null) return false;
        if (!isSittingOnBench) return false;
        if (equippedCharms.Contains(charm)) return false;
        if (occupiedNotchSlots + charm.notchCost > totalNotchSlots) return false;
        return true;
    }

    public void EquipCharm(CharmData charm)
    {
        if (charm == null) return;
        if (!CanEquipCharm(charm)) return;

        // If not unlocked yet, unlock it first
        if (!unlockedCharms.Contains(charm)) unlockedCharms.Add(charm);

        equippedCharms.Add(charm);
        occupiedNotchSlots += charm.notchCost;
        OnCharmsChanged?.Invoke();
        UpdatePlayerModifiers();
    }

    public void UnequipCharm(CharmData charm)
    {
        if (charm == null) return;
        if (!isSittingOnBench) return; // only while at bench
        if (!equippedCharms.Contains(charm)) return;

        equippedCharms.Remove(charm);
        occupiedNotchSlots = Mathf.Max(0, occupiedNotchSlots - charm.notchCost);
        OnCharmsChanged?.Invoke();
        UpdatePlayerModifiers();
    }

    public void UnlockCharm(CharmData charm)
    {
        if (charm == null) return;
        if (!unlockedCharms.Contains(charm)) unlockedCharms.Add(charm);
    }

    public void UpdatePlayerModifiers()
    {
        // Busca al jugador actual en la escena y le ordena recalcular sus estadísticas
        PlayerStateMachine player = UnityEngine.Object.FindAnyObjectByType<PlayerStateMachine>();
        if (player != null)
        {
            player.RecalculateStatsWithCharms();
        }
    }
}